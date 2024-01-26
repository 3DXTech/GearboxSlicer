# Copyright (c) 2023 3DXTECH LLC
# Created by Jon Grimshaw, jrg@3dxtech.com

# GB3D_AddLinesNCura script - Add Nxxx Line Numbers to G-code Files
# Lines starting with ";", or blank lines will be ignored

# history / changelog:
# V1.0.0:   Initial working release

from ..Script import Script
import re
#from UM.Logger import Logger

class GB3D_AddLinesNCura(Script):
    version = "1.0.0"
      
    def getSettingDataString(self):
        return """{
            "name": "GB3D Add Nxxx Line Numbers to G-code",
            "key": "GB3D_AddLinesNCura",
            "metadata": {},
            "version": 2,
            "settings":
            {
                "addlines_enabled":
                {
                    "label": "Enabled",
                    "description": "Allows enabling and disabling Nxxx Lines",
                    "type": "bool",
                    "default_value": true
                }
            }
        }"""

    def __init__(self):
        super().__init__()

    def execute(self, data):
        lineNumber = 1
        for layer in data:
            # will hold our updated gcode
            modified_gcode = ""
            
            # hold layer number
            layer_index = data.index(layer)
            
            # break apart the layer into commands
            lines = layer.split("\n")
            
            for line in lines:
                # hold line number
                line_index = lines.index(line)
                
                # trim or command
                #line = line.strip()
                                
                if (line.startswith(";")) or line.startswith(";POSTPROCESSED"):
                    #Logger.log("d", "Line with ; found")
                    #lines[line_index] = ";" + line
                    lines[line_index] = line
                    #Logger.log("d", line)
                else:
                    #lines[line_index] = "N" + str(lineNumber) + line
                    #Logger.log("d", "Adding linenumber N:" + str(lineNumber))
                    lines[line_index] = "N" + str(lineNumber) + " " + line
                    #Logger.log("d", line)
                    lineNumber += 1
                    
            final_lines = "\n".join(lines)
            data[layer_index] = final_lines
                    
        return data

