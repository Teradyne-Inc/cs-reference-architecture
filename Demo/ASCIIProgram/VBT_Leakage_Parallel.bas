Attribute VB_Name = "VBT_Leakage_Parallel"
Option Explicit

Public Function Leakage_Parallel_Baseline(aPinList As PinList, aVoltage As Double, aCurrentRange As Double, aWaitTime As Double) As Long

    Dim lMeas As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    ' This does the same like the InitLeakageTest-Setup in CSRA
    TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = chInitHi
    TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = chInitLo
    TheHdw.Digital.Pins("porta").InitState = chInitoff
    Call TheHdw.SettleWait(1)
    
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceV(aVoltage, aCurrentRange)
        .Gate = tlOn
        Call TheHdw.SetSettlingTimer(aWaitTime)
        lMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aPinList).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aVoltage, ForceUnit:=unitCustom, CustomForceUnit:="", ForceResults:=tlForceFlow)
    
End Function

Public Function Leakage_Parallel_Preconditioning(aPattern As Pattern, aPinList As PinList, aVoltage As Double, aCurrentRange As Double, aWaitTime As Double) As Long

    Dim lMeas As New PinListData
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    Call TheHdw.Patterns(aPattern).Load
    Call TheHdw.Patterns(aPattern).Start
    Call TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPattern).TimeDomains).Patgen.HaltWait
        
    Call TheHdw.Digital.Pins(aPinList).Disconnect
    
    With TheHdw.PPMU.Pins(aPinList)
        Call .Connect
        Call .ForceV(aVoltage, aCurrentRange)
        .Gate = tlOn
        Call TheHdw.SetSettlingTimer(aWaitTime)
        lMeas = .Read(tlPPMUReadMeasurements, 1, tlPPMUReadingFormatAverage)
        .Gate = tlOff
        Call .Disconnect
    End With
    
    Call TheHdw.Digital.Pins(aPinList).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lMeas, ForceVal:=aVoltage, ForceUnit:=unitCustom, CustomForceUnit:="", ForceResults:=tlForceFlow)
    
End Function
