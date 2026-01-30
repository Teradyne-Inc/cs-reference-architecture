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

Public Function Resistance_RdsOn_ThreePinsOneForceTwoMeasure(aForcePin As PinList, aForceCurrentPin As Double, aClampValueOfForcePin As Double, aMeasureFirstPin As PinList, _
            aMeasureSecondPin As PinList, aWaitTime As Double) As Long

    Dim lForce As New PinListData
    Dim lMeas As New PinListData
    Dim lMeasFirst As New PinListData
    Dim lMeasSecond As New PinListData
    Dim lResistance As New PinListData
    Dim measurePins As New PinList
    Dim digPins As New PinList
    
    measurePins = aMeasureFirstPin & "," & aMeasureSecondPin
    digPins = aForcePin & "," & measurePins
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("TDI,TMS").InitState = ChInitState.chInitoff
    Call TheHdw.Digital.Pins(digPins).Disconnect
    Call TheHdw.PPMU.Pins(digPins).Connect
    
    With TheHdw.PPMU.Pins(measurePins)
        Call .ForceI(0)
        .Gate = tlOff
    End With
    With TheHdw.PPMU.Pins(aForcePin)
        Call .ForceI(aForceCurrentPin, aForceCurrentPin)
        .ClampVHi.Value = aClampValueOfForcePin
        .Gate = tlOn
    End With
    Call TheHdw.Wait(aWaitTime)
    
    lMeasFirst = TheHdw.PPMU.Pins(aMeasureFirstPin).Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    lMeasSecond = TheHdw.PPMU.Pins(aMeasureSecondPin).Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    
    With lForce
        .AddPin (aForcePin)
        .Pins(aForcePin) = aForceCurrentPin
    End With
        
    lMeas = lMeasFirst.Math.Subtract(lMeasSecond)
    lResistance = lMeas.Math.Divide(lForce)
               
    With TheHdw.PPMU.Pins(digPins)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(digPins).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function

Public Function Resistance_RdsOn_FourPinsTwoForceTwoMeasure(aForceFirstPin As PinList, aForceValueFirstPin As Double, aClampValueOfForceFirstPin As Double, _
    aForceSecondPin As PinList, aForceValueSecondPin As Double, aClampValueOfForceSecondPin As Double, _
    aMeasureFirstPin As PinList, aMeasureSecondPin As PinList, aWaitTime As Double) As Long

    Dim lForce As New PinListData
    Dim lMeas As New PinListData
    Dim lMeasFirst As New PinListData
    Dim lMeasSecond As New PinListData
    Dim lResistance As New PinListData
    Dim lSinkFoldLimit As Double
    Dim measurePins As New PinList
    Dim digPins As New PinList
    
    measurePins = aMeasureFirstPin & "," & aMeasureSecondPin
    digPins = aForceFirstPin & "," & measurePins
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("TDI,TMS").InitState = ChInitState.chInitoff
    Call TheHdw.Digital.Pins(digPins).Disconnect
    Call TheHdw.PPMU.Pins(digPins).Connect
        
    With TheHdw.PPMU.Pins(measurePins)
        Call .ForceI(0)
        .Gate = tlOff
    End With
    With TheHdw.PPMU.Pins(aForceFirstPin)
        Call .ForceI(aForceValueFirstPin, aForceValueFirstPin)
        .ClampVHi.Value = aClampValueOfForceFirstPin
        .Gate = tlOn
    End With
    With TheHdw.DCVS.Pins(aForceSecondPin)
        .Mode = tlDCVSModeVoltage
        .CurrentRange.Value = aClampValueOfForceSecondPin
        .VoltageRange.Value = aForceValueSecondPin
        .Voltage.Value = aForceValueSecondPin
        lSinkFoldLimit = .CurrentLimit.Sink.FoldLimit.Level.Max
        .CurrentLimit.Sink.FoldLimit.Level.Value = IIf(aClampValueOfForceSecondPin > lSinkFoldLimit, lSinkFoldLimit, aClampValueOfForceSecondPin)
        .CurrentLimit.Source.FoldLimit.Level.Value = aClampValueOfForceSecondPin
        .Gate = True
    End With
        
    Call TheHdw.Wait(aWaitTime)
    
    lMeasFirst = TheHdw.PPMU.Pins(aMeasureFirstPin).Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    lMeasSecond = TheHdw.PPMU.Pins(aMeasureSecondPin).Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
    
    With lForce
        .AddPin (aForceFirstPin)
        .Pins(aForceFirstPin) = aForceValueFirstPin
    End With
        
    lMeas = lMeasFirst.Math.Subtract(lMeasSecond)
    lResistance = lMeas.Math.Divide(lForce)
               
    With TheHdw.PPMU.Pins(digPins)
        .Gate = tlOff
        Call .Disconnect
    End With
    Call TheHdw.Digital.Pins(digPins).Connect
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResistance, ForceResults:=tlForceFlow)
End Function
