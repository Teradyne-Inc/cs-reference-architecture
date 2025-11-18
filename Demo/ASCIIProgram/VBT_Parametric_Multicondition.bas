Attribute VB_Name = "VBT_Parametric_Multicondition"
Option Explicit

Public Function Parametric_MultiCondition_Baseline(Optional aWaitTime As Double = 0) As Long

    Dim lMeas As New PinListData
    
    ' PreBody
    Call TheHdw.Digital.ApplyLevelsTiming(True, False, False, tlPowered)
    Call TheHdw.Digital.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Disconnect
    Call TheHdw.PPMU.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Connect
    
    ' Body
    Call TheHdw.PPMU.Pins("nLEBA, nOEBA, porta").ForceV(5 * V, 2 * mA)
    Call TheHdw.PPMU.Pins("nLEAB, nOEAB").ForceV(0 * V, 2 * mA)
    Call TheHdw.PPMU.Pins("portb").ForceI(-500 * uA)
    TheHdw.PPMU.Pins("portb").ClampVHi = 6.5 * V
    TheHdw.PPMU.Pins("portb").ClampVLo = 0 * V
    TheHdw.PPMU.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Gate = tlOn
    Call TheHdw.SetSettlingTimer(aWaitTime)
    lMeas = TheHdw.PPMU.Pins("portb").Read
    
    ' PostBody
    TheHdw.PPMU.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Gate = tlOff
    Call TheHdw.PPMU.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Disconnect
    Call TheHdw.Digital.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Connect
    Call TheExec.Flow.TestLimit(lMeas, ForceResults:=tlForceFlow)
End Function

Public Function Parametric_MultiCondition_PreconditionPattern(aPreconditionPat As Pattern, Optional aWaitTime As Double = 0) As Long

    Dim lMeas As New PinListData
    
    ' PreBody
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    With TheHdw.Patterns(aPreconditionPat)
        Call .Load
        Call .Start
        Call TheHdw.Digital.TimeDomains(.TimeDomains).Patgen.HaltWait
    End With
    Call TheHdw.Digital.Pins("porta, portb").Disconnect
    Call TheHdw.PPMU.Pins("porta, portb").Connect
    
    ' Body
    Call TheHdw.PPMU.Pins("porta").ForceV(5 * V, 2 * mA)
    Call TheHdw.PPMU.Pins("portb").ForceI(-500 * uA)
    TheHdw.PPMU.Pins("portb").ClampVHi = 6.5 * V
    TheHdw.PPMU.Pins("portb").ClampVLo = 0 * V
    TheHdw.PPMU.Pins("porta, portb").Gate = tlOn
    Call TheHdw.SetSettlingTimer(aWaitTime)
    lMeas = TheHdw.PPMU.Pins("portb").Read
    
    ' PostBody
    TheHdw.PPMU.Pins("porta, portb").Gate = tlOff
    Call TheHdw.PPMU.Pins("porta, portb").Disconnect
    Call TheHdw.Digital.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb").Connect
    Call TheExec.Flow.TestLimit(lMeas, ForceResults:=tlForceFlow)
End Function


