# Copyright (c) 2020 BigRep GmbH
# DataCollector plugin is released under the terms of the LGPLv3.

import os
import json
import uuid
from typing import Optional, cast
import requests

from PyQt5.QtCore import pyqtSlot, QObject # pylint: disable=no-name-in-module

from UM.Extension import Extension
from UM.Application import Application
from UM.i18n import i18nCatalog
from UM.Logger import Logger
from UM.PluginRegistry import PluginRegistry
from cura.CuraApplication import CuraApplication

from . import collectorScript

catalog = i18nCatalog("DataCollector")

class DataCollector(QObject, Extension):
    
    def __init__(self, parent=None):
        QObject.__init__(self, parent)
        Extension.__init__(self)

        self._application = Application.getInstance()

        self._application.getOutputDeviceManager().writeFinished.connect(self.startCollector)
        
        self._application.getPreferences().addPreference("info/send_slice_info", True)
        self._application.getPreferences().addPreference("info/uuid", False)

        self.dataCollectorDialog = None
        self.dataCollectorExample = None
        self.data = None
        self.serverURL = "https://blade-statistics.bigrep.com/api/blade"

        self._application.initializationFinished.connect(self._onAppInitialized)

    def _onAppInitialized(self):
        if not self._application.getPreferences().getValue("info/uuid"):
            self._application.getPreferences().setValue("info/uuid", str(uuid.uuid4()))

    def showMoreInfoDialog(self):
        if self.dataCollectorDialog is None:
            self.dataCollectorDialog = self._createDialog("moreInfoDialog.qml")
        self.dataCollectorDialog.open()

    def _createDialog(self, qml_name):
        Logger.log("d", "Creating dialog [%s]", qml_name)
        filePath = os.path.join(PluginRegistry.getInstance().getPluginPath(self.getPluginId()), qml_name)
        dialog = Application.getInstance().createQmlComponent(filePath, {"manager": self})
        return dialog

    @pyqtSlot(result=str)
    def getExampleData(self) -> Optional[str]:
        if self.dataCollectorExample is None:
            pluginPath = PluginRegistry.getInstance().getPluginPath(self.getPluginId())
            if not pluginPath:
                Logger.log("e", "Could not get plugin path!", self.getPluginId())
                return None
            filePath = os.path.join(pluginPath, "exampleData.json")
            if filePath:
                with open(filePath, "r", encoding="utf-8") as exampleFile:
                    self.dataCollectorExample = exampleFile.read()
        return self.dataCollectorExample

    @pyqtSlot(bool)
    def setSendSliceInfo(self, enabled: bool):
        self._application.getPreferences().setValue("info/send_slice_info", enabled)

    def startCollector(self, outputDevice):
        try:
            scene = self._application.getController().getScene()
            # If the scene does not have a gcode, do nothing
            if not hasattr(scene, "gcode_dict"):
                return
            gcodeDict = getattr(scene, "gcode_dict")
            if not gcodeDict:
                return
            
            # get gcode list for the active build plate
            activeBuildPlateID = CuraApplication.getInstance().getMultiBuildPlateModel().activeBuildPlate
            gcodeList = gcodeDict[activeBuildPlateID]
            if not gcodeList:
                return

            if ";BigRep Blade" in gcodeList[0]:
                if Application.getInstance().getPreferences().getValue("info/send_slice_info"):
                    Logger.log("i", "User allowed to send data.")
                    with open(os.path.join(PluginRegistry.getInstance().getPluginPath(self.getPluginId()), "plugin.json")) as jsonFile:
                        pluginSettings = json.load(jsonFile)
                        DCVersion = pluginSettings["version"]
                    try:
                        self.data = collectorScript.collect()
                        self.data["environment"]["saveTo"] = type(outputDevice).__name__
                        self.data["DataCollector"] = DCVersion
                    except Exception as errorMessage: # pylint: disable=broad-except
                        application = cast(CuraApplication, Application.getInstance())
                        self.data = dict()
                        self.data["DataCollector"] = DCVersion
                        self.data["bladeVersion"] = application.getVersion().split("-")[0]
                        self.data["bladeBuildVersion"] = application.getVersion().split("-")[1]
                        self.data["uuid"] = str(Application.getInstance().getPreferences().getValue("info/uuid"))
                        self.data["status"] = "error"
                        self.data["message"] = str(errorMessage)
                        Logger.log("w", "Something went wrong when running CollectorScript. Error Message: %s", str(errorMessage))
                    self.sendData()
                else:
                    Logger.log("i", "User don't want to send data.")
            else:
                Logger.log("w", "Gcode is not postprocessed, but needs and should be processed by PPBlade.")
        except Exception as errorMessage: # pylint: disable=broad-except
            Logger.log("w", "An Error occurred during start collector: %s", errorMessage)

    def sendData(self):
        Logger.log("i", "Sending anonymous slice info to [%s]...", self.serverURL)

        jsonData = json.dumps(self.data, sort_keys=True).encode("utf-8")

        response = requests.post(self.serverURL, headers={'Content-Type': 'application/json'}, data=jsonData)
        if response.status_code == 200:
            Logger.log("i", "Sent anonymous slice info successfully.")
        else:
            Logger.log("w", "Failed to send anonymous slice info: %s", response)
