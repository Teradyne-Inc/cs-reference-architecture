Attribute VB_Name = "VBT_SupplyCurrent_Dynamic"
Option Explicit

Function SupplyCurrent_Dynamic_Baseline(aPinList As PinList, aForceValue As Double, aMeasureRange As Double, aClampValue As Double, aWaitTime As Double, aPattern As Pattern, stops As Integer) As Long

    Dim i As Integer, pin As Integer
    Dim lSinkFoldLimit As Double
    Dim lMeas As New PinListData
    Dim lSite As Variant, lSiteMeas As Variant
    Dim lTimeDomain As String
    Dim lSd As New SiteDouble
    
    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    With TheHdw.DCVS.Pins(aPinList)
        Call .Connect
        .Mode = tlDCVSModeVoltage
        .Voltage.Value = aForceValue
        .SetCurrentRanges aClampValue, aMeasureRange
        lSinkFoldLimit = .CurrentLimit.Sink.FoldLimit.Level.Max
        .CurrentLimit.Source.FoldLimit.Level.Value = aMeasureRange
        If (aMeasureRange > lSinkFoldLimit) Then
            .CurrentLimit.Sink.FoldLimit.Level.Value = lSinkFoldLimit
        Else
            .CurrentLimit.Sink.FoldLimit.Level.Value = aMeasureRange
        End If
        .Gate = True
        Call TheHdw.Patterns(aPattern).Start
        For i = 0 To stops - 1
            Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.FlagWait(CpuFlag.cpuA, 0)
            Call TheHdw.SetSettlingTimer(aWaitTime)
            .Meter.Strobe
            Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.Continue(0, CpuFlag.cpuA)
        Next i
        Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.HaltWait
        lMeas = .Meter.Read(tlNoStrobe, stops, 1, tlDCVSMeterReadingFormatArray)
        .Gate = False
        Call .Disconnect
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, Unit:=unitCustom, CustomForceUnit:="A", CompareMode:=CompareEachSample, _
                ForceResults:=tlForceFlow)
    
End Function
