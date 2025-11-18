Attribute VB_Name = "VBT_Resistance_Contact"
Option Explicit

Public Function Resistance_Contact_Baseline(aForcePin As PinList, aForceFirst As Double, aForceSecond As Double, aClampValue As Double, aWaitTime As Double) As Long

    Dim lForce As New PinListData
    Dim lMeas As New PinListData
    Dim lMeasFirst As New PinListData
    Dim lMeasSecond As New PinListData
    Dim lResistance As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    Call TheHdw.Digital.Pins(aForcePin).Disconnect
    
    With TheHdw.PPMU.Pins(aForcePin)
        Call .Connect
        Call .ForceI(aForceFirst, aForceFirst)
        .ClampVHi.Value = .ClampVHi.Max
        .ClampVLo.Value = aClampValue
        .Gate = tlOn
        Call TheHdw.Wait(aWaitTime)
        lMeasFirst = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
        Call .ForceI(aForceSecond, aForceSecond)
        Call TheHdw.Wait(aWaitTime)
        lMeasSecond = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    End With
    
    With lForce
        .AddPin (aForcePin)
        .Pins(aForcePin) = (aForceFirst - aForceSecond)
    End With
    
    lMeas = lMeasFirst.Math.Subtract(lMeasSecond)
    lResistance = lMeas.Math.Divide(lForce)
        
    With TheHdw.PPMU.Pins(aForcePin)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aForcePin).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function
