
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
        dwellTime = 0
        timeDefLine = 0
        timeDefLayer = 0
        for layer in data:
            layer_index = data.index(layer)
            lines = layer.split("\n")
            for line in lines:
                if (line.startswith("T")):
                    toolChangeCount += 1
                if (line.startswith(";TIME:") and baseTimeRemaining == -1):
                    timeDefLine = lines.index(line)
                    timeDefLayer = layer_index
                    baseTimeRemaining = self.getTimeValue(line)
                if (line.startswith("G4")):
                    dwellTime += int(line.split("P")[1]) / 1000
            data[layer_index] = "\n".join(lines)
        lines = data[timeDefLayer].split("\n")
        if (timeDefLine > 0):
            lines.pop(timeDefLine)
            lines.insert(timeDefLine, ";Base:{}".format(baseTimeRemaining))
            lines.insert(timeDefLine, ";Dwell:{}".format(dwellTime))
            addedToolTime = int(toolChangeCount * 30)
            allTheTime = int(baseTimeRemaining + addedToolTime + dwellTime)
            lines.insert(timeDefLine, ";TIME:{}".format(allTheTime))
            lines.insert(timeDefLine, ";Total number of tool changes: {}".format(toolChangeCount))
        data[timeDefLayer] = "\n".join(lines)
        
        return data