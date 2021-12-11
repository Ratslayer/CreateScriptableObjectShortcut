using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
public class CreateScriptableObjectWindow : EditorWindow
{ 
    #region Customize
    //name of created asset before renaming
    private const string InitialAssetName = "Asset";
    //window dimensions
    private const float Width = 200, Height = 300;
    //base type whose children are searched through.
    //I recommend creating your own base scriptable object so that you don't have to go through windows and stuff
    private Type ParentType => typeof(ScriptableObject);
    //display name that is used in search
    private string GetTypeDisplayName(Type type) => type.Name;
    //matching method that filters through type names based on input
    private bool Matches(string name, string typeName)
    {
        //first do a basic caseless comparison
        var result = typeName.ToLower().StartsWith(name.ToLower());
        if (!result)
        {
            //then do capital case matching
            var nameSplits = Split(name);
            var typeSplits = Split(typeName);
            static string[] Split(string s) => Regex.Replace(s, "([A-Z\\d])", " $1").Trim().Split();
            if (nameSplits.Length <= typeSplits.Length)
            {
                result = true;
                for (int i = 0; i < nameSplits.Length; i++)
                    if (!typeSplits[i].StartsWith(nameSplits[i]))
                    {
                        result = false;
                        break;
                    }
            }
        }
        return result;
    }
    //shortcut params
    [Shortcut("Create Scriptable Object", KeyCode.C, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
    private static void Shortcut() => OpenWindow();
    #endregion
    #region Functionality
    private const string TextContolName = "search_text_control";//could be anything, does not matter
    private Type[] _types;
    private string _name;
    private string _path;
    private bool _hasDrawn;
    private static void OpenWindow()
    {
        var path = GetCurrentSelectionPath();
        //open window if an object is selected in project view
        if (path.Length > 0)
        {
            var window = CreateInstance<CreateScriptableObjectWindow>();
            window._path = path;
            //move the window top left corner to mouse
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var size = new Vector2(Width, Height);
            window.position = new Rect(mousePos, size);
            window.ShowPopup();
        }
    }
    //get the path of currently selected object in project view
    private static string GetCurrentSelectionPath()
    {
        var selection = Selection.activeObject;
        var result = "";
        if (selection)
        {
            var selectionPath = AssetDatabase.GetAssetPath(selection);
            //if selection is not an asset then gtfo
            if (selectionPath.Length > 0)
            {
                //if selection is a folder just use its path
                if (Directory.Exists(selectionPath))
                    result = selectionPath + "/";
                //selection is an asset so use its folder
                else
                    result = Regex.Match(selectionPath, ".+/").Value;
                result += InitialAssetName + ".asset";
            }
        }
        return result;
    }
    private void OnEnable()
    {
        //get all child types of parent type
        var parent = ParentType;
        _types = (from type in Assembly.GetExecutingAssembly().GetTypes()
                  where type.IsClass && type != parent && parent.IsAssignableFrom(type)
                  select type).ToArray();
        Array.Sort(_types, (t1, t2) => GetTypeDisplayName(t1).CompareTo(GetTypeDisplayName(t2)));
        //set to false so the GUI can set focus on text when it opens
        _hasDrawn = false;
    }
    private void OnGUI()
    {
        //draw the search field
        GUI.SetNextControlName(TextContolName);
        _name = EditorGUILayout.TextField(_name);
        //display types that match input name
        var name = _name?.Trim();
        var isEmpty = string.IsNullOrWhiteSpace(name);
        var visibleTypes = from type in _types where isEmpty || Matches(name, GetTypeDisplayName(type)) select type;
        foreach (var type in visibleTypes)
            EditorGUILayout.LabelField(GetTypeDisplayName(type));
        //process keys. Create asset from first match on Enter, close on Escape
        if (Event.current.isKey)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                    var type = visibleTypes.FirstOrDefault();
                    if (type == null)
                        Close();
                    else
                    {
                        var obj = CreateInstance(type);
                        ProjectWindowUtil.CreateAsset(obj, _path);
                    }
                    break;
                case KeyCode.Escape:
                    Close();
                    break;
            }
        }
        //focus the text field if window had just been opened
        if (!_hasDrawn)
        {
            _hasDrawn = true;
            EditorGUI.FocusTextInControl(TextContolName);
        }
    }
    private void OnLostFocus()
    {
        Close();
    }
    #endregion
}