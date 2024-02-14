
from ..Script import Script

import re

class ModifiedProfile(Script):

    def __init__(self):
        super().__init__()

    def getSettingDataString(self):
        return """{
            "name": "Modified Profile",
            "key": "ModifiedProfile",
            "metadata": {},
            "version": 2,
            "settings": {}
            }"""

    def execute(self, data):
        timeDefLine = 0
        timeDefLayer = 0
        for layer in data:
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                #if (line.startswith("T")):
                #    toolChangeCount += 1
                if (line.startswith(";TIME:")):
                    timeDefLine = lines.index(line)
                    timeDefLayer = layer_index
                #if (line.startswith("G4")):
                #    dwellTime += int(line.split("P")[1]) / 1000
            data[layer_index] = "\n".join(lines)
        lines = data[timeDefLayer].split("\n")
        if (timeDefLine > 0):
            lines.pop(timeDefLine)
            lines.insert(timeDefLine, ";ModifiedProfile")
        data[timeDefLayer] = "\n".join(lines)
        
        return data