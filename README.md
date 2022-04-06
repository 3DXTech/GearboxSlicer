# GearSlice
### Current Version: 4.10.0

# Auto Install Instructions
Download and run the latest version of the `GearboxInstaller.exe` [here](https://github.com/GEARBOX3D/GearboxSlicer/releases)

# Install Instructions
If Cura 4.10.0 is not installed, download the installer [here](https://github.com/Ultimaker/Cura/releases/download/4.10.0/Ultimaker_Cura-4.10.0-amd64.exe) (release notes [here](https://github.com/Ultimaker/Cura/releases/tag/4.10.0))

### Offline Install
Download the Cura installer referenced above (make sure it is the correct version) and rename it to `curainstaller.exe` and place it in the same location as the Gearbox Slicer installer. When you run the installer it should automatically see that file and use that rather than download.

## Adding Printer, Materials, and Settings
> Make sure Cura is closed before copying any of these files!

Copy the contents of the `cura-files/programfiles` Folder (plugins/resources) into `Program Files/GearboxSlicer/` if it asks you to merge folders or overwrite files, say yes.
> If you would like to only see Gearbox materials and printers, delete the folders `extruders`, `materials`, `definitions`, and `variants` inside resources **before** copying files from this repo.

## Preconfiguring Settings
If you would like the HT2 and Gearbox3d theme selected in Cura automatically, copy the contents of the Appdata folder (`cura`) to `C:\Users\your-user-name\AppData\Roaming\`
> If you have an existing Cura 4.10.0 install be aware this will change your current settings. Refer to the [Cura Documentation](https://support.ultimaker.com/hc/en-us/articles/360012033899-How-to-add-a-printer-to-Ultimaker-Cura) for instructions on adding the printer manually.

## Breakdown
<details>
<summary>Click to show details of changes</summary>

- plugins
  - This contains a script for the `PostProcessingPlugin.py` that helps calculate a more accurate time estimate for prints on the HT2.
- resources
  - definitions
    - Default FDM printer definition and FDM extruder definition files as well as the HT2 printer definition file that inherits from FDM printer.
  - extruders
    - HT2 Definition files for the 2 HT2 extruders.
  - images
    - Splash screen and logo images for Cura skin.
  - materials
    - Definitions for all the Gearbox materials.
  - quality
    - Settings for how the materials print on the HT2, broken down by quality type.
  - setting_visibility
    - These allow different levels of viewable settings in Cura. To see the settings that we recommend changing, use the Gearbox3d level. You may change settings using advanced, however be sure to follow the recommended limits and warnings inside Cura to ensure proper operation of the HT2.
  - themes
    - An optional dark theme for Cura.
  - variants
    - Place new nozzle sizes in this area for them to appear on the Nozzle Size drop down. Quality profiles reference the variant name to correlate them.
</details>

## Post Processing Configuration

To add post processing scripts, click `Extensions -> Post Processing -> Modify G-Code` and add:
 1. `Tool Change Count`
 2. `Display Progress On LCD` - Check both boxes to show percentage and time remaining. Choose M73 for Time reporting method.
 3. `Create Thumbnail` - Change to 300x300


The tool change must come before display progress, otherwise the order does not matter, any other enabled scripts are fine to be added anywhere as well.

## Creating
### Random Notes
1. The `quality` folder is where the nozzle combinations are defined. 1 file must exist for each dual nozzle combination supported. If you want to support GBX20 model/GBX16 support and GBX16 model /GBX16 support, you need 2 files.
2. The file `definitions/fdmprinter.def.json` contains all the base settings. Anything that is not specifically defined in `gearbox3d_ht2.def.json` will use the values from that file.
3. Material definition files (`/materials/`) contain a GUID field, if you copy and paste one of them [create a new GUID](https://www.guidgenerator.com/) and change that field.

## Helpful Cura Things
1. [Profiles And Settings](https://github.com/Ultimaker/Cura/wiki/Profiles-&-Settings)
2. [Getting a Setting Value](https://github.com/Ultimaker/Cura/blob/master/docs/profiles/getting_a_setting_value.md)
3. [Container Stacks](https://github.com/Ultimaker/Cura/blob/master/docs/profiles/container_stacks.md)
4. [Create New Definition File](https://github.com/Ultimaker/Cura/wiki/Definition-Files-Explained#create-definition-files-for-a-new-machine)
