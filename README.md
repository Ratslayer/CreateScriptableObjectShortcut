# CreateScriptableObjectShortcut
This is a script file that adds a shortcut for easy scriptable object creation, without having to use CreateAssetMenuAttribute.
Just add this script to your project (preferably in an Editor folder) and it'll add a shortcut to Unity (Ctrl+Shift+C as default).

Here's how it works:
- Select any asset or folder in your project view.
- Invoke the shortcut.
- This opens a popup with the list of all types that extend ScriptableObject class. 
- You can search through those types with either caseless or capitalcase comparison.
- Pressing Enter will create a new asset same folder as your current project view selection.
- Asset type is the first type in the displayed list.
- Rename your asset and you're done.

The popup won't show if nothing is selected in the project view.
The asset won't be created if displayed type list is empty;

Checkout the customize region for simple script customization.
I recommend changing ParentType property from ScriptableObject to your own type that extends ScriptableObject.
Many types extend ScriptableObject (including windows) and they'll clutter the search list.
