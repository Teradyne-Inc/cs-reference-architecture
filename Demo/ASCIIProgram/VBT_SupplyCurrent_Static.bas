Attribute VB_Name = "VBT_SupplyCurrent_Static"
Option Explicit

Function SupplyCurrent_Static(aPinList As PinList, aForceValue As Double, aMeasureRange As Double, aClampValue As Double, aWaitTime As Double) As Long

    Dim lSinkFoldLimit As Double
    Dim lMeas As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    With TheHdw.DCVS.Pins(aPinList)
        Call .Connect
        .Mode = tlDCVSModeVoltage
        .Voltage.Value = aForceValue
        .SetCurrentRanges aClampValue, aMeasureRange
        lSinkFoldLimit = .CurrentLimit.Sink.FoldLimit.Level.Max
        .CurrentLimit.Source.FoldLimit.Level.Value = aClampValue
        If aClampValue > lSinkFoldLimit Then
            .CurrentLimit.Sink.FoldLimit.Level.Value = lSinkFoldLimit
        Else
            .CurrentLimit.Sink.FoldLimit.Level.Value = aClampValue
        End If
        .Gate = True
        Call TheHdw.SetSettlingTimer(aWaitTime)
        lMeas = TheHdw.DCVS.Pins(aPinList).Meter.Read()
        .Gate = False
        Call .Disconnect
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aForceValue, Unit:=unitCustom, CustomForceUnit:="V", _
    ForceResults:=tlForceFlow)

End Function
