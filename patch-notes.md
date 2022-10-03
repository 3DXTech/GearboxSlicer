### Fixed
- Tool change count script should no longer cause the percentage to stop super early #26
- Got a new GUID for custom support so it doesn't overlap with GF-PEI
- Default Raft Middle Thickness was above the maximum value warning
- Edited AppData Config files to reflect the changes for the default settings

### Changed
- How Acceleration and Jerk is applied to profiles in order to remove short pauses in the G-Code
- LTS1 to 300C Print Temp

### Added
- Download percentage display while download the Cura installer
- End Extruder G-Code to move the nozzle off of the part before initiating the Tool Change

### New Materials
- ASA
- FR PC/ABS
- ESD ABS
- PEKK-A
- PC/ABS
- MTS1

### New Profiles
- GBX20
    - ASA
    - FR PC/ABS
    - PEKK-A
    - PC/ABS
    - ESD ABS
    - PC
- GBX16
    - ASA
    - PC/ABS
    - FR PC/ABS
    - ESD ABS
    - PC
    - MTS1
- GBX12
    - Custom Model
    - Custom Support
- GBX10
    - Custom Model
    - Custom Support