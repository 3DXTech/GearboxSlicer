# Copyright (c) 2020 BigRep GmbH
# DataCollector plugin is released under the terms of the LGPLv3.

from datetime import datetime
from typing import cast, List, Set, Dict, Any # pylint: disable=unused-import
import json
import locale
import platform
import psutil

import numpy
import trimesh

from cura.CuraApplication import CuraApplication
from UM.Application import Application
from UM.Scene.Iterator.DepthFirstIterator import DepthFirstIterator
from UM.Qt.Duration import DurationFormat

if platform.system() == "Windows":
    import wmi

application = cast(CuraApplication, Application.getInstance())

def readGCodeData():
    def getGCodeChecksum(gcode: str) -> str:
        gcodeLines = gcode.splitlines()
        checksum = str()
        for line in gcodeLines:
            if line.startswith(";Checksum:"):
                checksum = line.split(" ")[1]
        return checksum

    def getGCodeExtruders(gcode: str) -> list:
        removeExtruderKeys = ["material_cost", "material_diameter", "material"]
        gcodeLines = gcode.splitlines()
        extruderList = list()
        for line in gcodeLines:
            if line.startswith(";Extruder settings:"):
                extruderLine = line.split("settings:")[1]
                extruderSettings = json.loads(extruderLine)
                extruderSettings["material_name"] = extruderSettings["material"]["type"]
                extruderSettings["material_brand"] = extruderSettings["material"]["brand"]
                for key in removeExtruderKeys:
                    extruderSettings.pop(key)
                extruderList.append(extruderSettings)
        return extruderList

    def getGCodeMachine(gcode: str) -> dict:
        gcodeLines = gcode.splitlines()
        for line in gcodeLines:
            if line.startswith(";Machine settings:"):
                machineLine = line.split("settings:")[1]
                machineSettings = json.loads(machineLine)
                machineSettings.pop("bed_temperature")
        return machineSettings

    def getGCodeFeatureFilament(gcode: str) -> dict:
        printFilaments = dict()
        gcodeLines = gcode.splitlines()
        for line in gcodeLines:
            if line.startswith(";featureFilament_"):
                tempString = str()
                tempString = line.split("_")[1]
                printFilaments[tempString.split(": ")[0]] = float(tempString.split(": ")[1])

        return printFilaments

    def getGCodePrintBoundingBox(gcode: str) -> dict:
        printBoundingBox = dict()
        gcodeLines = gcode.splitlines()
        for line in gcodeLines:
            if line.startswith(";MIN"):
                axis = line.split("MIN")[1].split(":")[0]
                printBoundingBox["min{}".format(axis)] = float(line.split(":")[1])

            elif line.startswith(";MAX"):
                axis = line.split("MAX")[1].split(":")[0]
                printBoundingBox["max{}".format(axis)] = float(line.split(":")[1])

        return printBoundingBox

    scene = Application.getInstance().getController().getScene()
    gcode_dict = getattr(scene, "gcode_dict", {})
    gcodeData = gcode_dict[0]

    checksum = getGCodeChecksum(gcodeData[-1])
    extruderList = getGCodeExtruders(gcodeData[0])
    machineSettings = getGCodeMachine(gcodeData[0])
    printFilaments = getGCodeFeatureFilament(gcodeData[0])
    printBoundingBox = getGCodePrintBoundingBox(gcodeData[0])

    return checksum, extruderList, machineSettings, printFilaments, printBoundingBox
    
def getEnviomentData() -> dict:
    environmentData = dict() # type: Dict[Any, Any]

    environmentData["uuid"] = str(Application.getInstance().getPreferences().getValue("info/uuid"))
    environmentData["system"] = platform.system()
    environmentData["systemVersion"] = platform.version()
    environmentData["localization"] = str(locale.getdefaultlocale()[0])
    environmentData["cores"] = psutil.cpu_count(logical=False)
    environmentData["vcores"] = psutil.cpu_count()
    environmentData["memory"] = round((psutil.virtual_memory().total/1024 ** 3), 2)

    if platform.system() == "Windows":
        computer = wmi.WMI()
        cpuInfo = computer.Win32_Processor()[0]
        gpuInfo = computer.Win32_VideoController()

        environmentData["cpu"] = cpuInfo.Name
        environmentData["cpu"] = environmentData["cpu"].rstrip()
        environmentData["gpu"] = list() # type: ignore
        for gpu in gpuInfo:
            environmentData["gpu"].append(gpu.Name)

    now = datetime.now()
    intToDay = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    environmentData["date"] = now.strftime("%Y-%m-%d")
    environmentData["time"] = now.strftime("%H:%M:%S")
    environmentData["day"] = intToDay[int(now.strftime("%w"))]
    environmentData["week"] = now.strftime("%W")

    environmentData["bladeVersion"] = application.getVersion().split("-")[0]
    environmentData["bladeBuildVersion"] = application.getVersion().split("-")[1]
    environmentData["bladeLanguage"] = Application.getInstance().getPreferences().getValue("general/language")

    return environmentData

def getPrintTimes() -> dict:
    printTimes = dict()

    printTimesData = application.getPrintInformation().printTimes()
    for feature in printTimesData:
        printTimes[feature] = int(printTimesData[feature].getDisplayString(DurationFormat.Format.Seconds))
    printTimes["total"] = int(application.getPrintInformation().currentPrintTime.getDisplayString(DurationFormat.Format.Seconds))
    return printTimes

def convertToTriMesh(meshData):
    indices = meshData.getIndices()
    if indices is None:
        indices = numpy.arange(meshData.getVertexCount()).reshape(-1, 3)

    return trimesh.base.Trimesh(vertices=meshData.getVertices(), faces=indices, vertex_normals=meshData.getNormals())

def getModelInformation() -> list:
    requestedSettings = ["support_enable", "adhesion_type", "infill_mesh", "cutting_mesh", "support_mesh",
                         "anti_overhang_mesh", "wall_line_count", "infill_sparse_density", "infill_pattern",
                         "support_tree_enable", "support_enable"]
    modelInformation = list()
    modelHashes = dict() # type: Dict[str, int]

    for node in DepthFirstIterator(application.getController().getScene().getRoot()):
        if node.callDecoration("isSliceable"):
            if node.getMeshData().getHash() in modelHashes:
                modelHashes[node.getMeshData().getHash()] += 1
                continue

            model = dict()
            model["hash"] = node.getMeshData().getHash()
            model["vertexCount"] = node.getMeshData().getVertexCount()
            modelHashes[model["hash"]] = 1
            boundingBox = node.getBoundingBox()
            model["boundingBox"] = {"minimum": {"x": boundingBox.minimum.x,
                                                "y": boundingBox.minimum.y,
                                                "z": boundingBox.minimum.z},
                                    "maximum": {"x": boundingBox.maximum.x,
                                                "y": boundingBox.maximum.y,
                                                "z": boundingBox.maximum.z}}
            model["transformation"] = {"data": str(node.getWorldTransformation().getData()).replace("\n", "")}

            triMeshMesh = convertToTriMesh(node.getMeshData())
            model["volume"] = triMeshMesh.volume
            model["surface"] = triMeshMesh.area

            extruder_position = node.callDecoration("getActiveExtruderPosition")
            model["extruder"] = 0 if extruder_position is None else int(extruder_position)

            modelSettings = dict()
            modelStack = node.callDecoration("getStack")
            if modelStack:
                for setting in requestedSettings:
                    modelSettings[setting] = modelStack.getProperty(setting, "value")
                modelSettings["support_extruder_nr"] = int(modelStack.getExtruderPositionValueWithDefault("support_extruder_nr"))
                modelSettings["support_interface_extruder_nr"] = int(modelStack.getExtruderPositionValueWithDefault("support_interface_extruder_nr"))

            model["modelSettings"] = modelSettings

            modelInformation.append(model)

    for model in modelInformation:
        model["meshCount"] = modelHashes[model["hash"]]

    return modelInformation

def getUserModifiedSettings() -> list:
    globalStack = application.getMachineManager().activeMachine

    globalKeys = globalStack.userChanges.getAllKeys() | globalStack.qualityChanges.getAllKeys()
    globalChangedSettings = dict()
    for setting in globalKeys:
        globalChangedSettings[setting] = globalStack.getProperty(setting, "value")

    extruderChangedSettings = list()
    for extruder in globalStack.extruderList:
        tempExtruderChanges = dict() # type: Dict[str, Any]
        extruderKeys = extruder.userChanges.getAllKeys() | extruder.qualityChanges.getAllKeys()
        for setting in extruderKeys:
            tempExtruderChanges[setting] = extruder.getProperty(setting, "value")
        extruderChangedSettings.append(tempExtruderChanges)

    userModifiedSettings = list() # type: List[Any]
    userModifiedSettings.append(globalChangedSettings)
    userModifiedSettings.append(extruderChangedSettings)

    return userModifiedSettings

def getGlobalSettings() -> dict:
    globalStack = application.getMachineManager().activeMachine
    requestedSettings = ["layer_height", "support_enable", "adhesion_type", "support_structure",
                         "infill_sparse_density", "infill_pattern", "print_sequence", "retraction_combing",
                         "z_seam_type", "z_seam_position", "z_seam_corner"]

    globalPrintSettings = dict()
    for setting in requestedSettings:
        globalPrintSettings[setting] = globalStack.getProperty(setting, "value")
    globalPrintSettings["support_extruder_nr"] = int(globalStack.getExtruderPositionValueWithDefault("support_extruder_nr"))
    globalPrintSettings["support_interface_extruder_nr"] = int(globalStack.getExtruderPositionValueWithDefault("support_interface_extruder_nr"))

    return globalPrintSettings

def getExtruderSettings() -> list:
    globalStack = application.getMachineManager().activeMachine
    requestedSettings = ["wall_line_count", "infill_sparse_density", "infill_pattern"]

    extruderSettings = list()

    extruders = globalStack.extruderList
    extruders = sorted(extruders, key=lambda extruder: extruder.getMetaDataEntry("position"))

    for extruder in extruders:
        tempExtruderSettings = dict()
        for setting in requestedSettings:
            tempExtruderSettings[setting] = extruder.getProperty(setting, "value")
        tempExtruderSettings["intent"] = extruder.intent.getMetaDataEntry("intent_category")

        extruderSettings.append(tempExtruderSettings)

    return extruderSettings

def getQualityProfile():
    globalStack = application.getMachineManager().activeMachine
    return globalStack.quality.getMetaData().get("quality_type")

def collect():
    collectedData = dict()

    collectedData["environment"] = getEnviomentData()

    checksum, extruderList, machineSettings, printFilaments, printBoundingBox = readGCodeData()
    collectedData["slicing"] = dict()
    collectedData["slicing"]["gcodeChecksum"] = checksum
    collectedData["slicing"]["extruders"] = extruderList
    collectedData["slicing"]["machine"] = machineSettings["machine"]
    collectedData["slicing"]["printFilaments"] = printFilaments
    collectedData["slicing"]["printBoundingBox"] = printBoundingBox
    collectedData["slicing"]["printTimes"] = getPrintTimes()
    collectedData["model"] = getModelInformation()
    collectedData["slicing"]["changedSettings"] = getUserModifiedSettings()
    collectedData["slicing"]["selectedPrintSettings"] = getGlobalSettings()
    extruderSliceSettings = getExtruderSettings()
    collectedData["slicing"]["extruders"][0]["selectedExtruderSettings"] = extruderSliceSettings[0]
    collectedData["slicing"]["extruders"][1]["selectedExtruderSettings"] = extruderSliceSettings[1]
    collectedData["slicing"]["qualityProfile"] = getQualityProfile()

    return collectedData
