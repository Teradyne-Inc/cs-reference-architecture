Attribute VB_Name = "VBT_Search_Parametric"
Option Explicit

Public Function Search_Parametric_LinearFull(aForcePins As String, aMeasurePin As String, aFrom As Double, aTo As Double, aCount As Integer, aThreshold As Double, aClampCurrent As Double, aWaitTime As Double) As Long
    
    Dim lMeasurements() As New SiteDouble
    Dim lResults As New SiteDouble
    Dim lForceValue As Double
    Dim lIncrement As Double
    Dim lMeas As New SiteDouble
    Dim lTripIndex As New SiteLong
    Dim i As Integer
    Dim Site As Variant
    
    ReDim lMeasurements(aCount - 1)
    lTripIndex(-1) = -1
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = chInitHi
    TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = chInitLo
    Call TheHdw.SettleWait(1)
    Call TheHdw.Digital.Pins(aForcePins & ", " & aMeasurePin).Disconnect
    Call TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Connect
    
    With TheHdw.PPMU.Pins(aMeasurePin)
        Call .ForceI(0)
        .Gate = tlOff
    End With
    
    With TheHdw.PPMU.Pins(aForcePins)
        Call .ForceV(aFrom, aClampCurrent)
        .Gate = tlOn
        lForceValue = aFrom
        lIncrement = (aTo - aFrom) / (aCount - 1)
        For i = 0 To aCount - 1
            Call .ForceV(lForceValue)
            Call TheHdw.SetSettlingTimer(aWaitTime)
            lMeasurements(i) = TheHdw.PPMU.Pins(aMeasurePin).Read()
            lForceValue = lForceValue + lIncrement
        Next i
    End With
    
    For Each Site In TheExec.Sites.Active
        For i = 0 To aCount - 1
            If lTripIndex = -1 And lMeasurements(i) > aThreshold Then lTripIndex = i
        Next i
        If lTripIndex = -1 Then lResults = -999 Else lResults = lTripIndex * lIncrement + aFrom
    Next Site
    
    TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Gate = tlOff
    Call TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Disconnect
    Call TheHdw.Digital.Pins(aForcePins & ", " & aMeasurePin).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceUnit:=unitCustom, ForceResults:=tlForceFlow)
    
End Function

Public Function Search_Parametric_LinearStop(aForcePins As String, aMeasurePin As String, aFrom As Double, aTo As Double, aCount As Integer, aThreshold As Double, aClampCurrent As Double, aWaitTime As Double) As Long
    
    Dim lResults As New SiteDouble
    Dim lForceValue As Double
    Dim lIncrement As Double
    Dim lMeas As New SiteDouble
    Dim lIndex As Integer
    Dim Site As Variant
    Dim lNotDone As Boolean
    Dim lSiteNotDone As Boolean
    
    lResults(-1) = -999
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = chInitHi
    TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = chInitLo
    Call TheHdw.SettleWait(1)
    Call TheHdw.Digital.Pins(aForcePins & ", " & aMeasurePin).Disconnect
    Call TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Connect
    
    With TheHdw.PPMU.Pins(aMeasurePin)
        Call .ForceI(0)
        .Gate = tlOff
    End With
    
    With TheHdw.PPMU.Pins(aForcePins)
        Call .ForceV(aFrom, aClampCurrent)
        .Gate = tlOn
        lForceValue = aFrom
        lIncrement = (aTo - aFrom) / (aCount - 1)
        lIndex = 0
        lNotDone = True
        Do While lForceValue <= aTo And lNotDone
            Call .ForceV(lForceValue)
            Call TheHdw.SetSettlingTimer(aWaitTime)
            lMeas = TheHdw.PPMU.Pins(aMeasurePin).Read()
            lNotDone = False
            For Each Site In TheExec.Sites.Active
                If lResults = -999 Then
                    If aThreshold < lMeas Then
                        lResults = lIndex * lIncrement + aFrom
                        lSiteNotDone = False
                    Else
                        lSiteNotDone = True
                    End If
                Else
                    lSiteNotDone = False
                End If
                lNotDone = lNotDone Or lSiteNotDone
            Next Site
            lForceValue = lForceValue + lIncrement
            lIndex = lIndex + 1
        Loop
    End With
    
    TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Gate = tlOff
    Call TheHdw.PPMU.Pins(aForcePins & ", " & aMeasurePin).Disconnect
    Call TheHdw.Digital.Pins(aForcePins & ", " & aMeasurePin).Connect
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceUnit:=unitCustom, ForceResults:=tlForceFlow)
    
End Function
