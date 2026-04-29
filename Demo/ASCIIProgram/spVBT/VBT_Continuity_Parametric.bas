Attribute VB_Name = "VBT_Continuity_Parametric"
Option Explicit

Public Function Continuity_Parametric_Parallel(aPinList As PinList, aCurrent As Double, aClampVoltage As Double, aWaitTime As Double) As Long

    Dim lMeas As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceI(aCurrent, aCurrent)
        If aCurrent >= 0 Then
            If aClampVoltage > .ClampVHi.Max Then
                .ClampVHi = .ClampVHi.Max
            Else
                .ClampVHi = aClampVoltage
            End If
            .ClampVLo = .ClampVLo.Min
        Else
            .ClampVHi = .ClampVHi.Max
            If aClampVoltage < .ClampVLo.Min Then
                .ClampVLo = .ClampVLo.Min
            Else
                .ClampVLo = aClampVoltage
            End If
        End If
        .Gate = tlOn
    
        Call TheHdw.SetSettlingTimer(aWaitTime)
        lMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aPinList).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aCurrent, Unit:=unitCustom, CustomForceUnit:="A", _
    ForceResults:=tlForceFlow)
End Function

Public Function Continuity_Parametric_Serial(aPinList As PinList, aCurrent As Double, aClampVoltage As Double, aWaitTime As Double) As Long

    Dim lMeas As New PinListData
    Dim lSingleMeas As New PinListData
    Dim lPins() As String
    Dim lCount As Long
    Dim lPin As Variant
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceV(0 * V)
        .Gate = tlOn
    
        Call TheExec.DataManager.DecomposePinList(aPinList, lPins, lCount)
        
        For Each lPin In lPins
            With TheHdw.PPMU.Pins(lPin)
                Call .ForceI(aCurrent, aCurrent)
                If aCurrent >= 0 Then
                    If aClampVoltage > .ClampVHi.Max Then
                        .ClampVHi = .ClampVHi.Max
                    Else
                        .ClampVHi = aClampVoltage
                    End If
                    .ClampVLo = .ClampVLo.Min
                Else
                    .ClampVHi = .ClampVHi.Max
                    If aClampVoltage < .ClampVLo.Min Then
                        .ClampVLo = .ClampVLo.Min
                    Else
                        .ClampVLo = aClampVoltage
                    End If
                End If
                Call TheHdw.SetSettlingTimer(aWaitTime)
                
                lSingleMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
                With lMeas.AddPin(lSingleMeas.Pins(0).Name)
                    .Value = lSingleMeas.Pins(0)
                End With
                
                Call .ForceV(0 * V)
            End With
        Next lPin
    
        .Gate = tlOff
        Call .Disconnect
    End With
    Call TheHdw.Digital.Pins(aPinList).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aCurrent, Unit:=unitCustom, CustomForceUnit:="A", _
    ForceResults:=tlForceFlow)
  
End Function
