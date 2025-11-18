Attribute VB_Name = "VBT_Continuity_Supply"
Option Explicit

Public Function Continuity_Supply_Baseline(aPinList As PinList, aforceVoltage As Double, aCurrentRange As Double, aWaitTime As Double) As Long

    Dim lMeas As New PinListData
    Dim lSinkFoldLimit As Double

    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    With TheHdw.DCVS.Pins(aPinList)
        Call .Connect
        .Mode = tlDCVSModeVoltage
        .Voltage.Value = aforceVoltage
        .CurrentRange = aCurrentRange
        lSinkFoldLimit = .CurrentLimit.Sink.FoldLimit.Level.Max
        .CurrentLimit.Source.FoldLimit.Level.Value = aCurrentRange
        If (aforceVoltage > lSinkFoldLimit) Then
            .CurrentLimit.Sink.FoldLimit.Level.Value = lSinkFoldLimit
        Else
            .CurrentLimit.Sink.FoldLimit.Level.Value = aCurrentRange
        End If
        .Gate = True
        Call TheHdw.SetSettlingTimer(aWaitTime)
        lMeas = .Meter.Read(tlStrobe, 1, , tlDCVSMeterReadingFormatAverage, tlDCVSMeterReadDataMeteredMode)
        .Gate = False
        Call .Disconnect
    End With

    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aforceVoltage, Unit:=unitCustom, CustomForceUnit:="V", ForceResults:=tlForceFlow)

End Function
