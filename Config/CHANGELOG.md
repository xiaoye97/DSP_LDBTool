### v3.0.0
- Updated for game version 0.10.28.20771
- Removed support for adding custom strings, because the game code changed in such way where it no longer makes sense for LDBTool to provide this functionality.

<details>
<summary>All changes</summary>

### v2.0.6
- Fixed that CustomGridIndex, CustomLocalization.ENUS, CustomLocalization.FRFR, CustomLocalization.ZNCH config files were wiped every time the game was launched.
### v2.0.5
- Fixed errors if UnityExporer was not installed.
### v2.0.4
- Fixed issues if one of mods had missing type references
- Fixed issues opening Proto View UI without UnityExplorer
### v2.0.3
- Removed caching of string protos and grid indexes. Now only if a player has changed value manually will it be overwriting the default. With this old config files were purged. A backup is saved in case players had some important settings there.
### v2.0.2
- Fixed some old mods causing missing method exception. Please note that using MethedEx.Copy in your mods is unrecomended. It will be removed soon.
### v2.0.1
- Fix README
- All Protos can now be seen in Proto view menu

### v2.0.0
- Types of protos that can be added is now computed at runtime
- Strings are bound in config file by their string key
- Strings ID's now are autoassigned and not bound to config file
- Now mods can override empty strings binding
- Added UnityExplorer support to Proto UI

### v1.8.0
- Added the function of custom translation, players can customize the translated text added by the Mod in the configuration file.

### v1.7.0
- Added the ability to customize the construction shortcut bar

### v1.6.0
- Optimized GUI, use RuntimeUnityEditor's skin when RuntimeUnityEditor is installed
- Added Proto search function, you can search for ID, Name, and translation
- Added a custom GridIndex configuration file, players can define the location of Mod items by themselves.

### v1.5.0
- Added the function of easily querying Proto data in the data display mode (point the mouse at the item, press I to view ItemProto, and press R to view RECEIVEPROTO)
- In the data display mode, the Tip of the item will display the ID later

### v1.4.0
- A profile with a custom ID has been added, and players can define the ID of the Mod item by themselves.

### v1.3.0
- Fixed item sorting issue
- Add object copy method

### v1.2.0
- Split the added data into pre-added and post-added in order to add translation Proto

### v1.1.0
- Support for modifying Proto data
- Add Proto data to view GUI

</details>