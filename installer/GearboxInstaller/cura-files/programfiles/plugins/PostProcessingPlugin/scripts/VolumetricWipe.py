from ..Script import Script

import re
import math

def is_extrusion_line(line: str) -> bool:
        """Check if current line is a standard printing segment.

        Args:
            line (str): Gcode line

        Returns:
            bool: True if the line is a standard printing segment
        """
        return "G1" in line and "X" in line and "Y" in line and "E" in line

def is_compatible_material(line: str) -> bool:
        if ("b5787a9f-3bc2-4110-b863-912bb74bea06" in line):
            return True
        return False

class VolumetricWipe(Script):

    def __init__(self):
        super().__init__()

    def getSettingDataString(self):
        return """{
            "name": "Volumetric Wipe",
            "key": "VolumetricWipePlugin",
            "metadata": {},
            "version": 2,
            "settings": {
                "extrusion":
                {
                    "label": "Extrusion Amount",
                    "description": "The amount of extrusion needed to wipe. Ex: 100 will wipe every 100mm of extrusion",
                    "type": "int",
                    "maximum_value_warning": "5000",
                    "default_value": 400
                }
            }
            }"""

    def execute(self, data):
        compatible_material = False
        currentTool = 0

        line_set = {}
        last_rounded_e_value = 0
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
                if (is_extrusion_line(line) and compatible_material == True and currentTool == 0):
                    searchE = re.search(r"E([-+]?\d*\.?\d*)", line)
                    if searchE:
                        e_value=float(searchE.group(1))
                        rounded_e = int(math.floor(e_value / self.getSettingValueByKey("extrusion"))) * self.getSettingValueByKey("extrusion")
                        if (rounded_e != last_rounded_e_value and rounded_e > last_rounded_e_value and rounded_e >= 0 and last_rounded_e_value >= 0):
                            lineIndex = lines.index(line)
                            lines.insert(lineIndex, "G12")
                        last_rounded_e_value = rounded_e
            data[layer_index] = "\n".join(lines)
        return data