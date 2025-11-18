Attribute VB_Name = "VBT_DCVS_Profile"
Option Explicit

Public Function Start_Current_profile(PinName As PinList, _
                                    SampleRate As Double, _
                                    SampleSize As Long) As Long
                                    
    TheExec.EnableWord("Profile_Voltage") = False
    Call start_profile(PinName, "I", SampleRate, SampleSize)
    
End Function

Public Function Start_Voltage_profile(PinName As PinList, _
                                    SampleRate As Double, _
                                    SampleSize As Long) As Long
                                    
    TheExec.EnableWord("Profile_Current") = False
    Call start_profile(PinName, "V", SampleRate, SampleSize)
    
End Function

Public Function start_profile(PinName As PinList, WhatToCapture As String, _
                            SampleRate As Double, SampleSize As Long) As Long

' Wait if another capture is running
Do While TheHdw.DCVS.Pins(PinName).Capture.IsRunning = True
Loop

'Create a SIGNAL to set up instrument
TheHdw.DCVS.Pins(PinName).Capture.Signals.Add "Capture_signal"

'Set this as the default signal
TheHdw.DCVS.Pins(PinName).Capture.Signals.DefaultSignal = "Capture_signal"

' Define the signal used for the capture
With TheHdw.DCVS.Pins(PinName).Capture.Signals.Item("Capture_signal")
    .Reinitialize
    If (WhatToCapture = "I") Then
        .Mode = tlDCVSMeterCurrent
        .Range = 0.1
    Else
        .Mode = tlDCVSMeterVoltage
        .Range = 10
    End If
    .SampleRate = SampleRate
    .SampleSize = SampleSize

End With

' Setup the hardware by loading the signal
TheHdw.DCVS.Pins(PinName).Capture.Signals.Item("Capture_signal").LoadSettings

' Start the capture
TheHdw.DCVS.Pins(PinName).Capture.Signals.Item("Capture_signal").Trigger

End Function

Public Function Plot_profile(PinName As PinList)

Dim IPLD As New PinListData
Dim DSPW As New DSPWave
Dim Label As String

' Wait for capture to complete
Do While TheHdw.DCVS.Pins(PinName).Capture.IsRunning = True
Loop

' Get the captured samples from the instrument
IPLD = TheHdw.DCVS.Pins(PinName).Capture.Signals.Item("Capture_signal").DSPWave
If TheExec.Sites.Site(0).Active = True Then DSPW = IPLD.Pins(PinName).Value(0)
If TheExec.Sites.Site(1).Active = True Then DSPW = IPLD.Pins(PinName).Value(1)


If TheHdw.DCVS.Pins(PinName).Meter.Mode = tlDCVSMeterCurrent Then Label = "Current profile"
If TheHdw.DCVS.Pins(PinName).Meter.Mode = tlDCVSMeterVoltage Then Label = "Voltage profile"

DSPW.Plot Label

DSPW = Nothing

End Function

Public Function I_Meter_setup_strobe_readbk()

Dim IPLD As New PinListData
Dim pin As Variant
Dim Site As Variant
Dim PinList As String
Dim Result As Double

PinList = "vcc"

With TheHdw.DCVS.Pins(PinList)
    .CurrentRange.Value = 15
    .Meter.Mode = tlDCVSMeterCurrent
.Meter.CurrentRange.Value = 1
End With

' Strobe and readback 10 samples
IPLD = TheHdw.DCVS.Pins(PinList).Meter.Read(tlStrobe, 10)
For Each pin In IPLD.Pins
    For Each Site In IPLD.Sites
        Result = pin.Value(Site)
        Debug.Print "The measured value on Site(" + CStr(Site) + ") of Pin(" + CStr(pin.Name) + ") Is " + CStr(Result)
    Next Site
Next pin

End Function



