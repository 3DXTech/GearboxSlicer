
from ..Script import Script

import re

class ToolChangeCount(Script):

    def __init__(self):
        super().__init__()

    def getSettingDataString(self):
        return """{
            "name": "Tool Change Count",
            "key": "ToolChangeCount",
            "metadata": {},
            "version": 2,
            "settings": {}
            }"""

    # Get the time value from a line as a float.
    # Example line ;TIME_ELAPSED:1234.6789 or ;TIME:1337
    def getTimeValue(self, line):
        list_split = re.split(":", line)  # Split at ":" so we can get the numerical value
        return float(list_split[1])  # Convert the numerical portion to a float

    def execute(self, data):
        line_set = {}
        toolChangeCount = 0
        baseTimeRemaining = -1
        timeDefLine = 0
        for layer in data:
            line_set = {}
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                if line in line_set:
                    continue
                line_set[line] = True
                if (line.startswith("T")):
                    toolChangeCount += 1
                    lineIndex = lines.index(line)
                    lines.insert(lineIndex, ";ToolChangeCount {}".format(toolChangeCount))
                if (line.startswith(";TIME:") and baseTimeRemaining == -1):
                    timeDefLine = lines.index(line)
                    baseTimeRemaining = self.getTimeValue(line)
            data[layer_index] = "\n".join(lines)
        lines = data[0].split("\n")
        lines.pop(timeDefLine)
        lines.insert(timeDefLine, ";TIME:{}".format(baseTimeRemaining + (toolChangeCount * 30)))
        lines.insert(timeDefLine, ";Total number of tool changes: {}".format(toolChangeCount))
        data[0] = "\n".join(lines)
        
        return data    