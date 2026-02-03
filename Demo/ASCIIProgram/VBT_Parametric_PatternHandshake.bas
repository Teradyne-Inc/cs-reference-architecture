Attribute VB_Name = "VBT_Parametric_PatternHandshake"
Option Explicit

Public Function Parametric_PatternHandshake_Baseline(aPattern As Pattern, aNumberOfStops As Long, aTestFunctional As Boolean) As Long

' Pre Body
Dim i As Long, pin As Long
Dim lPatResult As New SiteBoolean
Dim lMeas As New PinListData
Dim lStop As Long
Dim lSite As Variant, lSiteMeas As Variant
Dim lSd As New SiteDouble
Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)

' Body
TheHdw.DCVS.Pins("vcc").Meter.Mode = tlDCVSMeterCurrent
Call TheHdw.DCVS.Pins("vcc").SetCurrentRanges(0.2 * A, 0.2 * A)
Call TheHdw.Patterns(aPattern).Start
For lStop = 0 To aNumberOfStops - 1
    Call TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPattern).TimeDomains).Patgen.FlagWait(CpuFlag.cpuA, 0)
    Call TheHdw.DCVS.Pins("vcc").Meter.Strobe
    Call TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPattern).TimeDomains).Patgen.Continue(0, CpuFlag.cpuA)
Next lStop
lMeas = TheHdw.DCVS.Pins("vcc").Meter.Read(tlNoStrobe, aNumberOfStops, , tlDCVSMeterReadingFormatArray)
If (aTestFunctional) Then
    lPatResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
End If

' Post Body
If (aTestFunctional) Then
    Call TheExec.Flow.FunctionalTestLimit(lPatResult, aPattern)
End If

Call TheExec.Flow.TestLimit(ResultVal:=lMeas, Unit:=unitCustom, CustomForceUnit:="A", CompareMode:=CompareEachSample, _
                ForceResults:=tlForceFlow)

End Function
