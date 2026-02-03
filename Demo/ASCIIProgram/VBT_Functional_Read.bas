Attribute VB_Name = "VBT_Functional_Read"
Option Explicit

Public Function Functional_Read_Baseline(aPattern As Pattern, aPinList As PinList)
    
    Dim lResult As New SiteBoolean
    Dim lWords() As SiteLong
    Dim i As Long
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = chInitLo
    TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = chInitHi
    TheHdw.Digital.Pins("porta").InitState = chInitoff
    Call TheHdw.SettleWait(1#)
    
    Call TheHdw.Digital.HRAM.SetTrigger(trigFirst, False, 0, True)
    TheHdw.Digital.HRAM.CaptureType = captAll
    TheHdw.Digital.HRAM.Size = 128
    
    Call TheHdw.Patterns(aPattern).Start
    Call TheHdw.Patterns(aPattern).HaltWait
    
    lResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
    lWords = TheHdw.Digital.Pins(aPinList).HRAM.ReadDataWord(startIndex:=8, Count:=8, WordSize:=8, msbOrLsb:=tlBitOrderLsbFirst)
    
    Call TheExec.Flow.FunctionalTestLimit(lResult, aPattern)
    For i = LBound(lWords) To UBound(lWords)
        Call TheExec.Flow.TestLimit(ResultVal:=lWords(i), ForceResults:=tlForceFlow)
    Next i
    
End Function
