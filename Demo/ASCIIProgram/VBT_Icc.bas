Attribute VB_Name = "VBT_Icc"
Option Explicit

Public Function Icc_static(power_pin As PinList, Vcc_Value As Double, _
                            meter_current_range As Double, Init_LoPins As PinList, _
                            Init_HiPins As PinList, CurrentLimit As Double, _
                            Optional TNames_ As String = "Icc_static") As Long

    On Error GoTo errHandler

    Dim Site As Variant
    Dim IccMeasure As New PinListData


'''''Apply HSD levels, Init States, and PowerSupply pin values''''
'''''Connect all pins,load levels,do not load timing (not needed), do not hot-switch'''
'''''Set up initial state of pins as hi,low, and hi-z'''''''''
    TheHdw.Digital.ApplyLevelsTiming True, True, True, tlPowered, Init_HiPins.Value, Init_LoPins.Value

'''''''''Set up VCC pin to measure current'''''
    With TheHdw.DCVS.Pins(power_pin)
        .Meter.Mode = tlDCVSMeterCurrent
    End With

'''Program Current Range and Source Fold Limit
    TheHdw.DCVS.Pins(power_pin).CurrentRange = meter_current_range
    TheHdw.DCVS.Pins(power_pin).CurrentLimit.Source.FoldLimit.Level = CurrentLimit

'''Wait 5ms'''
    TheHdw.Wait 0.005

'''Strobe the meter on the VCC pin and store it in an pinlistdata variable defined''''
    IccMeasure = TheHdw.DCVS.Pins(power_pin).Meter.Read(tlStrobe, 10, 1000)

''''Set up OFFLINE Simulation by stuffing the pinlistdata variable with simulation data'''''''
    If TheExec.TesterMode = testModeOffline Then
    For Each Site In TheExec.Sites
        IccMeasure.Pins(0).Value(Site) = 0.055 + Rnd / 99
    Next Site
    End If

''''''''''DATALOG RESULTS''''''''''''''''''''''''''''''''''
    TheExec.Flow.TestLimit ResultVal:=IccMeasure, Unit:=unitAmp, TName:="Icc_static", _
            PinName:=power_pin, ForceVal:=Vcc_Value, ForceUnit:=unitVolt, _
            ForceResults:=tlForceFlow
            
    With TheHdw.DCVS.Pins(power_pin)
        .Gate = False
        .Disconnect (tlDCVSConnectDefault)
    End With



    Exit Function
errHandler:
    If AbortTest Then Exit Function Else Resume Next
End Function

Public Function icc_dynamic_vbt(PatternFile As Pattern, power_pin As PinList, Vcc_Value As Double, _
                            meter_current_range As Double, Init_LoPins As PinList, _
            Optional TNames_ As String = "Icc_dyanmic") As Long

    On Error GoTo errHandler

    Dim Site As Variant
    Dim IccMeasure As New PinListData


'''''Apply HSD levels, Init States, and PowerSupply pin values''''
'''''Connect all pins,load levels,do not load timing (not needed), do not hot-switch'''
'''''Set up initial state of pins as hi,low, and hi-z'''''''''
    TheHdw.Digital.ApplyLevelsTiming True, True, True, tlUnpowered, , Init_LoPins.Value


'''''''''Set up VCC pin to measure Current at 1A range'''''
    With TheHdw.DCVS.Pins(power_pin)
        .Meter.Mode = tlDCVSMeterCurrent
        .Meter.CurrentRange.Value = meter_current_range
    End With

    TheHdw.Patterns(PatternFile).Load
    TheHdw.Patterns(PatternFile).Start

'''Wait 1ms'''
    TheHdw.Wait 0.001

'''Strobe the meter on the VCC pin and store it in an pinlistdata variable defined''''
    IccMeasure = TheHdw.DCVS.Pins(power_pin).Meter.Read(tlStrobe)

    TheHdw.Digital.Patgen.Continue None, cpuA
    TheHdw.Digital.Patgen.HaltWait

''''Set up OFFLINE Simulation by stuffing the pinlistdata variable with simulation data'''''''
    If TheExec.TesterMode = testModeOffline Then
        For Each Site In TheExec.Sites
            IccMeasure.Pins(0).Value(Site) = 0.055 + Rnd / 99
        Next Site
    End If

''''''''''DATALOG RESULTS''''''''''''''''''''''''''''''''''
    TheExec.Flow.TestLimit ResultVal:=IccMeasure, Unit:=unitAmp, TName:="Icc_dynamic", _
            PinName:=power_pin, ForceVal:=Vcc_Value, ForceUnit:=unitVolt, _
            ForceResults:=tlForceFlow

    Exit Function
errHandler:
    If AbortTest Then Exit Function Else Resume Next
End Function



