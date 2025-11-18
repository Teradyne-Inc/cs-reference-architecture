Attribute VB_Name = "PortBridgeSupport"
Option Explicit
    
Public PortBridge As Variant 'New PortBridgeLanguage - can't use type to allow compiling also in no-PB-config
Private Const PORTBRIDGE_ENABLED_FILE As String = "..\PortBridgeEnabled.txt" 'typically where the *.sln file is located

Public Sub PortBridgeInitIfEnabled()
    On Error GoTo Hell:
    If LenB(Dir(PORTBRIDGE_ENABLED_FILE)) > 0 Then
        Set PortBridge = CreateObject("Teradyne.PortBridge.PortBridgeLanguage")
        Call PortBridge.Initialize(TheHdw.Tester.Type)
        PortBridge.Utilities.Logger.ShowInformationEntries = True
        PortBridge.Utilities.ShowStatusMonitor
    End If
    Exit Sub
Hell:
    Call TheExec.AddOutput("Error in OnProgramLoaded > PortBridgeInitIfEnabled:" & Err.Description)
End Sub

 

