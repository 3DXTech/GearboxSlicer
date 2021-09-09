# GearSlice
### Current Version: 4.10.0

# Install Instructions
If Cura 4.10.0 is not installed, download the installer [here](https://github.com/Ultimaker/Cura/releases/download/4.10.0/Ultimaker_Cura-4.10.0-amd64.exe) (release notes [here](https://github.com/Ultimaker/Cura/releases/tag/4.10.0))

## Adding Printer, Materials, and Settings
> Make sure Cura is closed before copying any of these files!

Copy the contents of the `GearSlice` Folder (plugins/resources) into `Program Files/Ultimaker Cura 4.10.0/` if it asks you to merge folders or overwrite files, say yes.

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
    - Definition file for the HT2
  - extruders
    - Definition files for the 2 HT2 extruders.
  - images
    - Splash screen and logo images for Cura skin
  - materials
    - Definitions for all the 3DXTech materials
  - quality
    - Settings for how the materials print on the HT2, broken down by nozzle
  - setting_visibility
    - These allow different levels of viewable settings in Cura. To see the settings that we recommend changing, use the Gearbox3d level. You may change settings using advanced, however be sure to follow the recommended limits and warnings inside Cura to ensure proper operation of the HT2.
  - themes
    - An optional dark theme for Cura
  - variants
    - Cura supports multiple variants for each extruder, which is differentiated by nozzle size with these definitions. These are required for the different material profiles to work with each different sized nozzle.
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

---


**General Responsibilities:**

1. Cura File System Overview (files, areas of changes, reasons for changes, future requirements, etc)
  - Changes made to 4.8
  - 8 changes needed, if this path holds true
  - 8 Changes required for 4.10, if this path is needed

**Cura File System Overview** **(Eddie and Collin)**

![](RackMultipart20210817-4-bjis2r_html_5d961a27ddf6a839.png)

All files/folders that were left untouched are: bundled\_packages, firmware, i18n, intent, setting\_visibility, shaders, texts, and public\_key. NOTE: Setting\_visibility folder may be handy for making a Gearbox3D specific file that limits what Users can see in the CUSTOM settings view in Cura. All other files were modified in some fashion and these changes can be found on Github as well as here where I will give general explanations for the folder contents&#39; functions and modifications.

**Definitions** – This folder houses the files that tell Cura specific data about the MACHINE/PRINTER. There is a master file called fdm\_printer that all definitions use as a baseline or default. Modifications or &quot;overrides&quot; can then be placed in the specific machine definition to modify the default values. I removed all other machines in this folder and inputted a custom file for the gearbox3d\_ht2. The reason that I removed the files (similar for all removals) is so that they do not populate the menu in the program for Users to select.

**Extruders** – This folder is very similar to the definitions folder expect it houses the config files for the individual machine\_extruders or heads. The default file, fdm\_extruder, for this is pulled from the definitions folder.

**Images** – This folder houses most of the image files that are used in the program. Notable items missing from this folder are the icons which are located in Themes. Here you will find splash screens, backplates (used for text on buildplate), and some random items that I have not determined a use for.

**Materials** – All material files live in this folder. The file type is .xml. Each of these files has information about the specific materials as well as print settings which can be machine- and variant-specific.

**Meshes** – Contains the 3d file (.stl) of the build plate to be used in the program.

**QML** – This folder and the related changes are much more intensive than the other folders. I will not be able to describe each of the changes fully here but there is a decent change log in [Github.](https://github.com/GEARBOX3D/GearSlice)

**Quality** – Holds settings for different variant and material combinations. Notable items are the global quality settings and the material + variant specific settings. The global ones hold defaults specific to that variant. The material + variant ones hold the &quot;meat&quot; of the profile work that Aaron has been conducting. NOTE: The support materials do not have a GBX20 at the moment due to the decision to use a GBX16 support with both GBX16 and GBX20 model.

**Themes** – This folder holds the information surrounding the GUI colors, fonts, icons, etc. The main folders are for each of the GUI themes with Cura Light being the &quot;parent&quot; or default that the others work off of.

**Variants** – The variants are the different nozzle sizes that are available for a certain machine&#39;s extruder. For us, they are the GBX10 thru GBX40 nozzles.

**Helpful**** Resources**:

- [Cura Process Flow for Settings](https://github.com/Ultimaker/Cura/wiki/Profiles-&amp;-Settings) – In the link is a webpage of the Cura wiki that helps show the workflow for how Cura determines print settings.
- [Print Settings Plugin](https://github.com/Ghostkeeper/SettingsGuide) – A plugin made by GhostKeeper to help explain more about what the Cura print settings actually do. I haven&#39;t used it but it always looked helpful.
- [God Mode Plugin](https://github.com/sedwards2009/cura-god-mode-plugin) – Exports a json file of your current &quot;Cura Print Settings Stack&quot;

- [Cura Wiki](https://github.com/Ultimaker/Cura/wiki/Profiles-&amp;-Settings) – Great resource for understanding how the Cura program works from a process standpoint.

**4.8 Changes Still Needed:**

- QML needs Legal statement added into welcome pages
  - [Gary&#39;s notes on this](https://netorg4911482.sharepoint.com/:w:/s/Gearbox3D/EeRVwEdvuixPpZJdKo2pT5IBsrvdAsXsHQZIqGp8dF546A?e=2BHGSx)
- Welcome Pages needs Gearbox3D website links updated
- Dark theme needs some color fixes
  - Some of the left hand menus for model manipulation have dark text on a dark background
  - Material window in the preferences menu has dark text on a dark highlight
- If anything changes with the machine specific settings, say on Motionworks side, then the machine definition should be changed to match it
- Temperatures for material standby/idle states need to be locked down
- Add in Aaron&#39;s dual extrusion profile changes
  - This is a simple process of taking the Custom User Profile that he exports from cura and picking out the settings list. Then, one must manually compare this to the settings currently in the QUALITY DEFINITION for that material + variant combination. Whatever is new, input it into the file but KEEP IN MIND that something may look new but can be included in a 1) variant definition, 2) material definition, or 3) machine definition. Those settings that are in any of those areas are there because they are common amongst a group.

**Changes needed for 4.10:**

- Check that all &quot;overrides&quot; in machine definition match any changes to fdm\_printer definition
- Check for any QML folder changes and make sure that my 4.8 changes span the versions without issues
- Plugin cleanup

**Custom GCode Explanations (Collin and Eddie)**

- Most codes have explanations in them as to what sections of code are doing from a process standpoint. I would like to give a bit more context as to why I chose to add in certain sections.
- **T0 and T1** – The general process for these is as follows:
  1. Coord shift to global
  2. Move the head from current position to Purge Chute
  3. Purge and wipe current nozzle
  4. Switch heads position
  5. Purge and wipe new nozzle
    - Debate surrounding this due to battle between ooze compensation and extrusion readiness
  6. Return to last coords on build plate
  7. Coord shift to printing
  8. M30
- **Load** – Loading process is as follows:
  1. Machine will run when asked to load until it sees filament activate the limit switch in the y-blocks. At this point, it will activate Gcode which will begin the following:
  2. Coord shift to global
  3. Tool Check
  4. Move to last Z-position move to the purge chute
  5. M104 to extruding temp
  6. Home extruder axis to 0 and command motion
  7. M109 to extruding temp
  8. Command motion in intervals and wipe in between
    - Planned for this to be a point where the user can confirm that material is extruding and stop before wasting more filament
  9. Retract 2.9mm and set to M104 S100
  10. Return to last position and home axis
  11. Coord shift to printing
  12. M30
- **Unload** – Unloading process is as follows:
  1. Coord shift to global
  2. Tool Check
  3. M104 to extruding temp
  4. Move to last Z-position move to the purge chute
  5. Wipe
  6. M109 to extruding temp
  7. Purge 20mm then retract 70mm
  8. Wipe
  9. M104 to 0C
  10. Retract out of the printer by a distance
  11. Move to last known coords
  12. Coord shift to printing
  13. M30
- **Purge and Wipe** – Unloading process is as follows:
  1. Coord shift to global
  2. Move Z position down 2 mm from current position
  3. Move to purge chute
  4. Activate the inactive tool
  5. Switch heads position
  6. Wipe 2x
  7. Activate previous Tool
  8. Switch heads position
  9. Purge 10mm and wait
  10. Wipe
  11. Reset each tool extruder position to last known state
  12. Return to last known coords
  13. Coord shift to printing
  14. M30
- **Wipe** – Wiping process is as follows:
  1. Coord shift to global
  2. Move Z position down 2 mm from current position
  3. Move to purge chute
  4. Wipe 2x
  5. Move to last known coords
  6. Reset each tool extruder position to last known state
  7. Coord shift to printing
  8. M30

**Laptop Files and Programs + Physical Documentation**

- **C:\Program Files\Gearbox Cura** – I have not been able to push all of my changes to github at the time that I am writing this. I hope to complete this task but this file directory is where my most up to date files are stored for CURA 4.8
- **C:\Program Files\Ultimaker Cura 4.10** – I have made some slight modifications to Cura 4.10 due to requests from the Software team (Eddie and Collin specifically) but have not taken a deep dive into this due to the uncertainty around what path we want to take with Cura. The latest update I received on this was from Collin on August 6, 2021 saying that they wanted to make 4.10 look Gearbox-specific but Eddie had not yet figured out if making our own build was possible or not. We will wait until Eddie finds a conclusion.

`main_window_header_button_text_hovered`: Text of the tabs (Preview/Prepare/Monitor) If moused over when not selected
