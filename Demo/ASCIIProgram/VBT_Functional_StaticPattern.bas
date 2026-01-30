Attribute VB_Name = "VBT_Functional_StaticPattern"
Option Explicit

Public Function Functional_StaticPattern_Baseline(aPattern As Pattern) As Long
    
    Dim lResult As New SiteBoolean
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    Call TheHdw.Patterns(aPattern).Start
    Call TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPattern).TimeDomains).Patgen.HaltWait
    lResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
    Call TheExec.Flow.FunctionalTestLimit(lResult, aPattern)

End Function
