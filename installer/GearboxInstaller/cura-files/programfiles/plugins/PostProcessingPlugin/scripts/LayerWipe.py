from ..Script import Script

import re
import math

def is_compatible_material(line: str) -> bool:
        if ("b5787a9f-3bc2-4110-b863-912bb74bea06" in line):
            return True
        return False

class LayerWipe(Script):

    def __init__(self):
        super().__init__()

    def getSettingDataString(self):
        return """{
            "name": "Layer Wipe (Beta)",
            "key": "LayerWipePlugin",
            "metadata": {},
            "version": 2,
            "settings": {}
            }"""

    def execute(self, data):
        compatible_material = False
        currentTool = 0

        line_set = {}
        for layer in data:
            line_set = {}
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                # maintain a line collection so that we don't loop through the same lines over and over if insert shifts it down
                if line in line_set:
                    continue
                line_set[line] = True
                if (line.startswith(";material_guid0") and is_compatible_material(line)):
                    compatible_material = True
                if (line.startswith("T0")):
                    currentTool = 0
                if (line.startswith("T1")):
                    currentTool = 1
                if (compatible_material == True and line.startswith(";LAYER:")):
                    lineIndex = lines.index(line)
                    lines.insert(lineIndex, "G12")
            data[layer_index] = "\n".join(lines)
        return data