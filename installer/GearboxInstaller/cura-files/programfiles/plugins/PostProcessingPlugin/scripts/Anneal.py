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
                "step1_enabled": {
                    "label": "Step 1 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step1_temperature":
                {
                    "label": "Step 1 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step1_time":
                {
                    "label": "Step 1 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step2_enabled": {
                    "label": "Step 2 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step2_temperature":
                {
                    "label": "Step 2 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step2_time":
                {
                    "label": "Step 2 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step3_enabled": {
                    "label": "Step 3 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step3_temperature":
                {
                    "label": "Step 3 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step3_time":
                {
                    "label": "Step 3 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step4_enabled": {
                    "label": "Step 4 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step4_temperature":
                {
                    "label": "Step 4 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step4_time":
                {
                    "label": "Step 4 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step5_enabled": {
                    "label": "Step 5 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step5_temperature":
                {
                    "label": "Step 5 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step5_time":
                {
                    "label": "Step 5 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step6_enabled": {
                    "label": "Step 6 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step6_temperature":
                {
                    "label": "Step 6 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step6_time":
                {
                    "label": "Step 6 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step7_enabled": {
                    "label": "Step 7 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step7_temperature":
                {
                    "label": "Step 7 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step7_time":
                {
                    "label": "Step 7 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step8_enabled": {
                    "label": "Step 8 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step8_temperature":
                {
                    "label": "Step 8 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step8_time":
                {
                    "label": "Step 8 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step9_enabled": {
                    "label": "Step 9 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step9_temperature":
                {
                    "label": "Step 9 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step9_time":
                {
                    "label": "Step 9 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
                    "type": "int",
                    "default_value": 2
                },
                "step10_enabled": {
                    "label": "Step 10 Enabled",
                    "description": "Enables this annealing step",
                    "type": "bool",
                    "default_value": false
                },     
                "step10_temperature":
                {
                    "label": "Step 10 Temperature (C)",
                    "description": "The temperature to set the machine for the dwell duration",
                    "type": "int",
                    "maximum_value_warning": "230",
                    "default_value": 90
                },
                "step10_time":
                {
                    "label": "Step 10 Time (Minutes)",
                    "description": "How long to dwell at the given temperature",
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
                    i = 1
                    while i < 10:
                        if (self.getSettingValueByKey("step{}_enabled".format(i)) == True): 
                            lines.insert(lineIndex, "G4 P{}".format(self.getSettingValueByKey("step{}_time".format(i)) * 60 * 1000))
                            lines.insert(lineIndex, "M141 S{}".format(self.getSettingValueByKey("step{}_temperature".format(i))))
                            lineIndex += 2
                        i += 1
                    break
            data[layer_index] = "\n".join(lines)
        lines = data[timeDefLayer].split("\n")
        #if (timeDefLine > 0):
        #    lines.insert(timeDefLine, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        #else:
        #    lines.insert(0, ";ANNEAL {}, {}".format(self.getSettingValueByKey("temperature"), (self.getSettingValueByKey("time") * 60 * 60 * 1000)))
        data[timeDefLayer] = "\n".join(lines)
        
        return data    
        