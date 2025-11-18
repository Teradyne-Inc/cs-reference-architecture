Attribute VB_Name = "VBT_Search_Functional"
Option Explicit

Public Function Search_Functional_LinearFull(aPattern As Pattern, aForcePins As String, aFrom As Double, aTo As Double, aCount As Integer, aWaitTime As Double) As Long
    
    Dim lMeasurements() As New SiteBoolean
    Dim lResults As New SiteDouble
    Dim lTripIndex As New SiteLong
    Dim lTimeDomain As String
    Dim Site As Variant
    Dim lInValue As Double
    Dim lIncrement As Double
    Dim i As Integer
    
    ReDim lMeasurements(aCount)
    lTripIndex(-1) = -1

    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlUnpowered)
    With TheHdw.DCVS.Pins(aForcePins)
        Call .Connect
        .Voltage.Value = aFrom
        .Gate = True
'       first step may be bigger than the subsequent ones, use 2x settling
        Call TheHdw.Wait(aWaitTime)
        
        lInValue = aFrom
        lIncrement = (aTo - aFrom) / (aCount - 1)
        For i = 0 To aCount - 1
            .Voltage.Main = lInValue
            Call TheHdw.Wait(aWaitTime)
            Call TheHdw.Patterns(aPattern).Start
            Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.HaltWait
            lMeasurements(i) = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
            lInValue = lInValue + lIncrement
        Next i
        For Each Site In TheExec.Sites.Active
            For i = 0 To aCount - 1
                If lTripIndex = -1 And lMeasurements(i) Then lTripIndex = i
            Next i
        Next Site
        lResults = lTripIndex.Multiply(lIncrement).Add(aFrom)
        For Each Site In TheExec.Sites.Active
            If lTripIndex = -1 Then lResults = -999
        Next Site
        
        .Gate = False
        Call .Disconnect
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)

End Function
    

Public Function Search_Functional_LinearStop(aPattern As Pattern, aForcePins As String, aFrom As Double, aTo As Double, aCount As Integer, aWaitTime As Double) As Long

    Dim lResults As New SiteDouble
    Dim lValue As New SiteBoolean
    Dim lTimeDomain As String
    Dim lInValue As Double
    Dim lIncrement As Double
    Dim i As Integer
    Dim Site As Variant
    
    lResults(-1) = -999
    
    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlUnpowered)
    With TheHdw.DCVS.Pins(aForcePins)
        Call .Connect
        .Voltage.Value = aFrom
        .Gate = True
'       first step may be bigger than the subsequent ones, use 2x settling
        Call TheHdw.Wait(aWaitTime)
        
        lInValue = aFrom
        lIncrement = (aTo - aFrom) / (aCount - 1)
        For i = 0 To aCount - 1
            .Voltage.Value = lInValue
            Call TheHdw.Wait(aWaitTime)
            Call TheHdw.Patterns(aPattern).Start
            Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.HaltWait
            lValue = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
            For Each Site In TheExec.Sites.Active
                If lResults = -999 And lValue Then lResults = lInValue
            Next Site
            lInValue = lInValue + lIncrement
'            breaking if everything tripped is missing
        Next i
        .Gate = False
        Call .Disconnect
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)

End Function

Public Function Search_Functional_Binary(aPattern As Pattern, aForcePins As String, aFrom As Double, aTo As Double, aMinDelta As Double, aWaitTime As Double) As Long

    Dim lResults As New SiteDouble
    Dim lInValue As New SiteDouble
    Dim lValue As New SiteBoolean
    Dim lAlwaysTripped As New SiteBoolean
    Dim lTimeDomain As String
    Dim lDelta As Double
    Dim lDone As Boolean
    Dim Site As Variant
    
    lAlwaysTripped(-1) = True
    lResults(-1) = -999
    lDone = False
    lInValue(-1) = (aFrom + aTo) / 2
    lDelta = (aTo - aFrom) / 2
    
    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlUnpowered)
    With TheHdw.DCVS.Pins(aForcePins)
        Call .Connect
        .Voltage.Value = (aFrom + aTo) / 2
        .Gate = True
'       first step may be bigger than the subsequent ones, use 2x settling
        Call TheHdw.Wait(aWaitTime)
        
        Do
            .Voltage.ValuePerSite = lInValue
            Call TheHdw.Wait(aWaitTime)
            Call TheHdw.Patterns(aPattern).Start
            Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.HaltWait
            lValue = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
            lDone = lDelta <= aMinDelta
            lDelta = lDelta / 2
            For Each Site In TheExec.Sites.Active
                If lValue Then
                    lResults = lInValue
                    lInValue = lInValue - lDelta
                Else
                    lAlwaysTripped = False
                    lInValue = lInValue + lDelta
                End If
            Next Site
        Loop Until lDone
        
        For Each Site In TheExec.Sites.Active
            If lAlwaysTripped Then lResults = -999
        Next Site
        
        .Gate = False
        Call .Disconnect
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function
