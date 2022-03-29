from ..Script import Script

import re

class Anneal(Script):

    def __init__(self):
        super().__init__()

    def getSettingDataString(self):
        return """{
            "name": "Anneal",
            "key": "AnnealPlugin",
            "metadata": {},
            "version": 2,
            "settings": {
                "temperature":
                {
                    "label": "Temperature",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "default_value": 200
                },
                "time":
                {
                    "label": "Time",
                    "description": "How long to dwell at the given temperature, in hours",
                    "type": "int",
                    "default_value": 2
                }
            }
            }"""

    def execute(self, data):
        line_set = {}
        timeDefLine = 0
        timeDefLayer = 0
        for layer in data:
            line_set = {}
            lineIndex = 0
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                # maintain a line collection so that we don't loop through the same lines over and over if insert shifts it down
                if line in line_set:
                    continue
                line_set[line] = True
                if (line.startswith(";TIME") and timeDefLine == 0):
                    timeDefLine = lines.index(line)
                if (line.startswith(";anneal") and layer_index > 1):
                    lineIndex = lines.index(line)
                    lines.insert(lineIndex, "G4 P{}".format(self.getSettingValueByKey("time") * 60 * 60 * 1000))
                    lines.insert(lineIndex, "M141 S{}".format(self.getSettingValueByKey("temperature")))
                    break
            data[layer_index] = "\n".join(lines)
        lines = data[timeDefLayer].split("\n")
        if (timeDefLine > 0):
            lines.insert(timeDefLine, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        else:
            lines.insert(0, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        data[timeDefLayer] = "\n".join(lines)
        
        return data    
        