// Copyright (c) 2019 Ultimaker B.V.
// Cura is released under the terms of the LGPLv3 or higher.

import QtQuick 2.10
import QtQuick.Controls 2.3

import UM 1.3 as UM
import Cura 1.1 as Cura


//
// This component contains the content for the "Ultimaker Cloud" page of the welcome on-boarding process.
//
Item
{
    UM.I18nCatalog { id: catalog; name: "cura" }

    signal cloudPrintersDetected(bool newCloudPrintersDetected)

    Component.onCompleted: CuraApplication.getDiscoveredCloudPrintersModel().cloudPrintersDetectedChanged.connect(cloudPrintersDetected)

    onCloudPrintersDetected:
    {
        // When the user signs in successfully, it will be checked whether he/she has cloud printers connected to
        // the account. If he/she does, then the welcome wizard will show a summary of the Cloud printers linked to the
        // account. If there are no cloud printers, then proceed to the next page (if any)
        if(newCloudPrintersDetected)
        {
            base.goToPage("add_cloud_printers")
        }
        else
        {
            base.showNextPage()
        }
    }

    Label
    {
        id: titleLabel
        anchors.top: parent.top
        anchors.horizontalCenter: parent.horizontalCenter
        horizontalAlignment: Text.AlignHCenter
        text: "Check out our products!"
        color: UM.Theme.getColor("primary_button")
        font: UM.Theme.getFont("huge")
        renderType: Text.NativeRendering
    }

    // Area where the cloud contents can be put. Pictures, texts and such.
    Item
    {
        id: cloudContentsArea
        anchors
        {
            top: titleLabel.bottom
            bottom: skipButton.top
            left: parent.left
            right: parent.right
            topMargin: UM.Theme.getSize("default_margin").height
        }

        // Pictures and texts are arranged using Columns with spacing. The whole picture and text area is centered in
        // the cloud contents area.
        Column
        {
            anchors.centerIn: parent
            width: parent.width
            height: childrenRect.height

            spacing: 20 * screenScaleFactor

            Image  // Cloud image
            {
                id: cloudImage
                anchors.horizontalCenter: parent.horizontalCenter
                source: UM.Theme.getImage("first_run_ultimaker_cloud")
            }

            Label  // A title-ish text
            {
                id: highlightTextLabel
                anchors.horizontalCenter: parent.horizontalCenter
                horizontalAlignment: Text.AlignHCenter
                text: "Your place for all you need to 3D print with confidence!"
                textFormat: Text.RichText
                color: UM.Theme.getColor("primary")
                font: UM.Theme.getFont("medium")
                renderType: Text.NativeRendering
            }

            Label  // A number of text items
            {
                id: textLabel
                anchors.horizontalCenter: parent.horizontalCenter
                text:
                {
                    // There are 3 text items, each of which is translated separately as a single piece of text.
                    var full_text = ""
                    var t = ""

                    t = "- Browse and order new materials and tools for your Gearbox3D HT2."
                    full_text += "<p>" + t + "</p>"

                    t = "- Stay up to date on new product offerings and happenings."
                    full_text += "<p>" + t + "</p>"

//                    t = "- FILLER TEXT"
//                    full_text += "<p>" + t + "</p>"

                    return full_text
                }
                textFormat: Text.RichText
                font: UM.Theme.getFont("medium")
                color: UM.Theme.getColor("text")
                renderType: Text.NativeRendering
            }

            // "Sign in" and "Create an account" exist inside the column
            Cura.PrimaryButton
            {
                id: signInButton
                height: createAccountButton.height
                width: createAccountButton.width
                anchors.horizontalCenter: parent.horizontalCenter
                text: "3DXTech Website"
                onClicked: Qt.openUrlExternally("http://www.3dxtech.com")
                // Content Item is used in order to align the text inside the button. Without it, when resizing the
                // button, the text will be aligned on the left
                contentItem: Text {
                    text: signInButton.text
                    font: UM.Theme.getFont("medium")
                    color: UM.Theme.getColor("primary_text")
                    horizontalAlignment: Text.AlignHCenter
                    verticalAlignment: Text.AlignVCenter
                }
            }

            Cura.SecondaryButton
            {
                id: createAccountButton
                anchors.horizontalCenter: parent.horizontalCenter
                text: "Engineering Grade Filaments"
                onClicked: Qt.openUrlExternally("http://www.3dxtech.com/products/")
            }
        }


    }
    
    // The "Skip" button exists on the bottom right
    Label
    {
        id: skipButton
        anchors.right: parent.right
        anchors.bottom: parent.bottom
        anchors.leftMargin: UM.Theme.getSize("default_margin").width
        text: "Next"
        color: UM.Theme.getColor("secondary_button_text")
        font: UM.Theme.getFont("medium")
        renderType: Text.NativeRendering

        MouseArea
        {
            anchors.fill: parent
            hoverEnabled: true
            onClicked: base.showNextPage()
            onEntered: parent.font.underline = true
            onExited: parent.font.underline = false
        }
    }
}
