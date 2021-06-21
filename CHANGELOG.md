# Custom Project Browser

## [0.7.0] - 2021-06-21
- `SharedModule` v1.10.0 or later

### Added
- Added `focusedInspectorsButton`
- Added `notifyPrefabParent`

### Changed
- Removed extension color settings
You can set the color of the extension with `SharedModule`
- `customToolbar` has been deprecated and will be removed in the next version

## [0.6.0] - 2021-06-09
- `SharedModule` v1.9.0 or later

### Added
- Added `MonoBehaviour` Script View
- Added Project Path Open Button
- Added External Link Test
- Added `ScriptableObjectManager` extensions

### Changed
- Changed the Boolean value in `SettingsEditor` to a flag
- Removed Addressable function

### Fixed
- Fixed folders being created in unintended hierarchies
- Temporarily modified to restore `VisualElement` when the window is attached or detached 

## [0.5.17] - 2021-03-01
- SharedModule v1.7.7 or later

### Added
- GUIDs can now be copied from the context menu.
- Add language file

## [0.5.16] - 2021-02-07
- SharedModule v1.7.5 or later

### Added
- Added Upgrade function to URP, HDRP, and materials.
- Font, TMPro's Font Asset Creator can now be called

### Changed
- Titlebar Override is now available in 2019.1 and later

## [0.5.15] - 2020-12-20
- SharedModule v1.7.4 or later

### Added
- Added the button position adjustment function

### Changed
- The function to be terminated is changed to Obsolete

## [0.5.14] - 2020-12-13
- SharedModule v1.7.3 or later

### Changed
- Changed namespace

## [0.5.13] - 2020-12-02
- SharedModule v1.7.0 or later

## [0.5.12] - 2020-11-15
- SharedModule v1.6.0 or later

### Added
- Added "ForceReserializeAssets" to context menu

### Changed
- Made the operation of New Folder the same as the menu

## [0.5.11] - 2020-09-14

### Fixed
- Fixed Fixed an error when the argument of UnityEditorProjectBrowser.IsTwoColumns was null

## [0.5.10] - 2020-09-13

### Added
- Addition of a folder creation function with frequently used names
- Added a C # Script creation button

### Fixed
- Fixed heavy processing in IconClickContext

## [0.5.9] - 2020-08-09

### Required
- SharedModule v1.5.5 or later

### Added
- Added support for preset texture importers

## [0.5.8] - 2020-08-08

### Required
- SharedModule v1.5.4 or later

### Added
- Function addition test in tab field
  - Asset creation
  - GUID display

## [0.5.7] - 2020-08-02

### Added
- Added texture import settings to the folder

### Changed
- Changed dependencies in package.json

## [0.5.6] - 2020-07-16
- SharedModule v1.5.0 is supported

### Changed
- Support for the new setting
- The Localize folder has been moved

## [0.5.5] - 2020-05-31
SharedModule v1.3.0 is required

### Changed
- Set "Auto Referenced" of asmdef to false

### Fixed
- Fixed poor performance with a few hundred megabytes of assets

## [0.5.4] - 2020-04-18
- SharedModule v1.2.0 is supported

## [0.5.3] - 2020-04-04
- SharedModule v1.1.0 is supported

## [0.5.2] - 2020-03-28

### Added
- Added Addressable support

### Changed
- The extension click is now optionally selectable
- The extension click function has been changed to Windows only

### Fixed
- Fix compile error 「CustomProjectBrowser.PACKAGE_NAME」 -> 「Package.name」

## [0.5.0] - 2020-03-21
- First release

### Changed
- Restructured repository
- Refactored file names and namespaces