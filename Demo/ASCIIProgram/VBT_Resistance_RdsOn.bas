Attribute VB_Name = "VBT_Resistance_RdsOn"
Option Explicit

Public Function Resistance_RdsOn_Baseline(aForcePin As PinList, aForceValue As Double, aWaitTime As Double) As Long

    Dim lForce As New PinListData
    Dim lMeas As New PinListData
    Dim lResistance As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    Call TheHdw.Digital.Pins(aForcePin).Disconnect
    
    With TheHdw.PPMU.Pins(aForcePin)
        Call .Connect
        Call .ForceI(aForceValue, aForceValue)
        .Gate = tlOn
        Call TheHdw.Wait(aWaitTime)
        lMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    End With
    
    With lForce
        .AddPin (aForcePin)
        .Pins(aForcePin) = aForceValue
    End With
    
    lResistance = lMeas.Math.Divide(lForce)
        
    With TheHdw.PPMU.Pins(aForcePin)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aForcePin).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function

Public Function Resistance_RdsOn_TwoPinsOneForceOneMeasure(aPinList As PinList, aForceValue As Double, aClampValue As Double, labelOfStoredVoltage As Double, aWaitTime As Double) As Long


    Dim lForce As New PinListData
    Dim lComputeValue As New PinListData
    Dim lMeas As New PinListData
    Dim lResistance As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("clk_src").InitState = chInitHi
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceI(aForceValue, aForceValue)
        .ClampVHi.Value = aClampValue
        .Gate = tlOn
        Call TheHdw.Wait(aWaitTime)
        lMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    End With
                
    With lForce
        .AddPin (aPinList)
        .Pins(aPinList) = aForceValue
    End With
    
    lComputeValue = lMeas.Math.Subtract(labelOfStoredVoltage)
    lResistance = lComputeValue.Math.Divide(lForce)
               
    With TheHdw.PPMU.Pins(aPinList)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aPinList).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function

Public Function Resistance_RdsOn_TwoPinsDeltaForceDeltaMeasure(aPinList As PinList, aForceFirst As Double, aForceSecond As Double, aClampValue As Double, aWaitTime As Double) As Long


    Dim lForce As New PinListData
    Dim lMeas As New PinListData
    Dim lMeasFirst As New PinListData
    Dim lMeasSecond As New PinListData
    Dim lResistance As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("clk_src").InitState = chInitHi
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceI(aForceFirst, aForceFirst)
        .ClampVHi.Value = aClampValue
        .Gate = tlOn
        Call TheHdw.Wait(aWaitTime)
        lMeasFirst = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
        Call .ForceI(aForceSecond, aForceSecond)
        Call TheHdw.Wait(aWaitTime)
        lMeasSecond = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    End With
                
    With lForce
        .AddPin (aPinList)
        .Pins(aPinList) = aForceFirst - aForceSecond
    End With
        
    lMeas = lMeasFirst.Math.Subtract(lMeasSecond)
    lResistance = lMeas.Math.Divide(lForce)
               
    With TheHdw.PPMU.Pins(aPinList)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aPinList).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function
