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
                    "label": "Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "time":
                {
                    "label": "Time (Hours)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "steps":
                {
                    "label": "Steps",
                    "description": "Amount of times to anneal model",
                    "type": "int",
                    "default_value": 1,
                    "minimum_value": 1,
                    "maximum_value": 10
                },
                "step_temp_delta":
                {
                    "label": "Step Temperature Delta",
                    "description": "Amount to decrease temp each step to minimum of 50 C",
                    "type": "int",
                    "default_value": 30,
                    "minimum_value": 1,
                    "maximum_value": 200
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
                    i = 0
                    while i < self.getSettingValueByKey("steps"):
                        lines.insert(lineIndex, "G4 P{}".format(self.getSettingValueByKey("time") * 60 * 60 * 1000))
                        lines.insert(lineIndex, "M141 S{}".format(self.getSettingValueByKey("temperature") if i == 0 else self.getSettingValueByKey("temperature") - (self.getSettingValueByKey("step_temp_delta") * (i + 1))))
                        lineIndex += 2
                        i += 1
                        if self.getSettingValueByKey("temperature") - (self.getSettingValueByKey("step_temp_delta") * (i + 1)) <= 50:
                            break
                    break
            data[layer_index] = "\n".join(lines)
        lines = data[timeDefLayer].split("\n")
        if (timeDefLine > 0):
            lines.insert(timeDefLine, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        else:
            lines.insert(0, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        data[timeDefLayer] = "\n".join(lines)
        
        return data    
        