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

    // Area where the cloud contents can be put. Pictures, texts and such.
    Item
    {
        id: cloudContentsArea
        anchors
        {
            top: parent.top
            bottom: skipButton.top
            left: parent.left
            right: parent.right
        }

        // Pictures and texts are arranged using Columns with spacing. The whole picture and text area is centered in
        // the cloud contents area.
        Column
        {
            anchors.horizontalCenter: parent.horizontalCenter
            width: parent.width
            height: childrenRect.height

            spacing: UM.Theme.getSize("thick_margin").height

            Label
            {
                id: titleLabel
                anchors.horizontalCenter: parent.horizontalCenter
                horizontalAlignment: Text.AlignHCenter
                //text: catalog.i18nc("@label", "Sign in to the Ultimaker platform")
                text: "Check out our products!"
                color: UM.Theme.getColor("primary_button")
                font: UM.Theme.getFont("huge")
                renderType: Text.NativeRendering
            }

            // Filler item
            Item
            {
                height: UM.Theme.getSize("default_margin").height
                width: parent.width
            }

            // Cloud image
            Image
            {
                id: cloudImage
                anchors.horizontalCenter: parent.horizontalCenter
                source: UM.Theme.getImage("first_run_ultimaker_cloud")
                fillMode: Image.PreserveAspectFit
                width: UM.Theme.getSize("welcome_wizard_content_image_big").width
                sourceSize.width: width
                sourceSize.height: height
            }


            // Filler item
            Item
            {
                height: UM.Theme.getSize("default_margin").height
                width: parent.width
            }

            // Motivational icons
            Row
            {
                id: motivationRow
                width: parent.width

                Column
                {
                    id: marketplaceColumn
                    width: Math.round(parent.width / 3)
                    spacing: UM.Theme.getSize("default_margin").height

                    Image
                    {
                        id: marketplaceImage
                        anchors.horizontalCenter: parent.horizontalCenter
                        fillMode: Image.PreserveAspectFit
                        width: UM.Theme.getSize("welcome_wizard_cloud_content_image").width
                        source: UM.Theme.getIcon("package")
                        sourceSize.width: width
                        sourceSize.height: height
                    }
                    Label
                    {
                        id: marketplaceTextLabel
                        anchors.horizontalCenter: parent.horizontalCenter
                        width: parent.width
                        //text: catalog.i18nc("@text", "Add material settings and plugins from the Marketplace")
                        text: "Your place for all you need to 3D print with confidence!"
                        wrapMode: Text.Wrap
                        horizontalAlignment: Text.AlignHCenter
                        color: UM.Theme.getColor("text")
                        font: UM.Theme.getFont("default")
                        renderType: Text.NativeRendering
                    }
                }

                Column
                {
                    id: syncColumn
                    width: Math.round(parent.width / 3)
                    spacing: UM.Theme.getSize("default_margin").height

                    Image
                    {
                        id: syncImage
                        anchors.horizontalCenter: parent.horizontalCenter
                        fillMode: Image.PreserveAspectFit
                        width: UM.Theme.getSize("welcome_wizard_cloud_content_image").width
                        source: UM.Theme.getIcon("material_spool")
                        sourceSize.width: width
                        sourceSize.height: height
                    }
                    Label
                    {
                        id: syncTextLabel
                        anchors.horizontalCenter: parent.horizontalCenter
                        width: parent.width
                        //text: catalog.i18nc("@text", "Backup and sync your material settings and plugins")
                        text: "Browse and order new materials and tools for your Gearbox3D HT2."
                        wrapMode: Text.Wrap
                        horizontalAlignment: Text.AlignHCenter
                        color: UM.Theme.getColor("text")
                        font: UM.Theme.getFont("default")
                        renderType: Text.NativeRendering
                    }
                }

                Column
                {
                    id: communityColumn
                    width: Math.round(parent.width / 3)
                    spacing: UM.Theme.getSize("default_margin").height

                    Image
                    {
                        id: communityImage
                        anchors.horizontalCenter: communityColumn.horizontalCenter
                        fillMode: Image.PreserveAspectFit
                        width: UM.Theme.getSize("welcome_wizard_cloud_content_image").width
                        source: UM.Theme.getIcon("group")
                        sourceSize.width: width
                        sourceSize.height: height
                    }
                    Label
                    {
                        id: communityTextLabel
                        anchors.horizontalCenter: communityColumn.horizontalCenter
                        width: parent.width
                        //text: catalog.i18nc("@text", "Share ideas and get help from 48,000+ users in the Ultimaker Community")
                        text: "Stay up to date on new product offerings and happenings."
                        wrapMode: Text.Wrap
                        horizontalAlignment: Text.AlignHCenter
                        color: UM.Theme.getColor("text")
                        font: UM.Theme.getFont("default")
                        renderType: Text.NativeRendering
                    }
                }
            }

            // Sign in Button
            Cura.PrimaryButton
            {
                id: signInButton
                anchors.horizontalCenter: parent.horizontalCenter
                //text: catalog.i18nc("@button", "Sign in")
                //onClicked: Cura.API.account.login()
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

            // Create an account button
            Cura.TertiaryButton
            {
                id: createAccountButton
                anchors.horizontalCenter: parent.horizontalCenter
                //text: catalog.i18nc("@text", "Create a free Ultimaker Account")
                //onClicked:  Qt.openUrlExternally(CuraApplication.ultimakerCloudAccountRootUrl + "/app/create")
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
        //text: catalog.i18nc("@button", "Skip")
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
