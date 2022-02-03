
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
        for layer in data:
            line_set = {}
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                if line in line_set:
                    continue
                line_set[line] = True
                if (line.startswith("M30")):
                    lineIndex = lines.index(line)
                    lines.insert(lineIndex, "G4 P{}".format(self.getSettingValueByKey("time") * 60 * 60 * 1000))
                    lines.insert(lineIndex, "M141 S{}".format(self.getSettingValueByKey("temperature")))
            data[layer_index] = "\n".join(lines)
        lines = data[0].split("\n")
        lines.insert(4, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        data[0] = "\n".join(lines)
        
        return data    