Attribute VB_Name = "VBT_Trim_Digital"
Option Explicit

Public Function Trim_Digital_LinearStopTrip(aPattern As Pattern, aFrom As Long, aTo As Long, aCount As Long) As Long
    Dim lResults As New SiteLong
    Dim lValue As New SiteBoolean
    Dim lTimeDomain As String
    Dim lInValue As Long
    Dim lIncrement As Long
    Dim i As Long
    Dim Site As Variant
    Dim lAllValid As Boolean
    
    lResults(-1) = -999
    
    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)

        
    lInValue = aFrom
    lIncrement = (aTo - aFrom) / (aCount - 1)
    For i = 0 To aCount - 1
        Call TheHdw.Patterns(aPattern.Value + ":mod" + CStr(lInValue)).Start
        Call TheHdw.Digital.TimeDomains(lTimeDomain).Patgen.HaltWait
        lValue = TheHdw.Digital.Patgen.PatternBurstPassedPerSite
        For Each Site In TheExec.Sites.Active
            If lResults = -999 And lValue Then lResults = lInValue
        Next Site
        lInValue = lInValue + lIncrement
'            breaking if everything tripped is missing
        lAllValid = True
        For Each Site In TheExec.Sites
            If lResults(Site) = -999 Then
                lAllValid = False
                Exit For
            End If
        Next Site
        If lAllValid Then Exit For
    Next i
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function

Public Function Trim_Digital_LinearStopTarget(aPattern As Pattern, aCapPins As PinList, aFrom As Long, aTo As Long, aCount As Long, aTarget As Long) As Long
    Dim lResults As New SiteLong
    Dim lTimeDomain As String
    Dim lInValue As Long
    Dim lIncrement As Long
    Dim i As Long
    Dim Site As Variant
    Dim lOutValue1 As New SiteLong
    Dim lDelta1 As New SiteLong
    Dim lOutValue2 As New SiteLong
    Dim lDelta2 As New SiteLong
    Dim lAllValid As Boolean
    
    lResults(-1) = -999
    
    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)

    With TheHdw.Digital.HRAM
        .SetTrigger trigFirst, False, 0, True
        .CaptureType = captSTV
        .Size = 8
    End With
        
    lInValue = aFrom
    lIncrement = (aTo - aFrom) / (aCount - 1)
    
    lOutValue1 = OneMeasurement(aPattern, aPattern.Value + ":hram_mod" + CStr(lInValue), aCapPins, lInValue)
    lDelta1 = lOutValue1.Subtract(aTarget)
    lInValue = lInValue + lIncrement
    
    For i = 1 To aCount - 1
        lOutValue2 = OneMeasurement(aPattern, aPattern.Value + ":hram_mod" + CStr(lInValue), aCapPins, lInValue)
        lDelta2 = lOutValue2.Subtract(aTarget)
        
        For Each Site In TheExec.Sites.Active
            If lResults(Site) = -999 Then
                If Sgn(lDelta1(Site)) <> Sgn(lDelta2(Site)) Then
                    lResults(Site) = lInValue
                End If
            End If
        Next Site
        lInValue = lInValue + lIncrement
'            breaking if everything tripped is missing
        lAllValid = True
        For Each Site In TheExec.Sites
            If lResults(Site) = -999 Then
                lAllValid = False
                Exit For
            End If
        Next Site
        If lAllValid Then Exit For
    Next i
    
    With TheHdw.Digital.HRAM
        .SetTrigger trigNever, False, 0, True
        .CaptureType = captNone
        .Size = 0
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function

Public Function Trim_Digital_LinearFullTrip(aPattern As Pattern, aFrom As Long, aTo As Long, aCount As Long) As Long
    
    Dim lMeasurements() As New SiteBoolean
    Dim lResults As New SiteLong
    Dim lTripIndex As New SiteLong
    Dim lTimeDomain As String
    Dim Site As Variant
    Dim lInValue As Long
    Dim lIncrement As Long
    Dim i As Long
    
    ReDim lMeasurements(aCount - 1)
    lTripIndex(-1) = -1 'initialize with -1 to indicate no trip found

    lTimeDomain = TheHdw.Patterns(aPattern).TimeDomains
        
    lInValue = aFrom
    lIncrement = (aTo - aFrom) / (aCount - 1)
    For i = 0 To aCount - 1
        Call TheHdw.Patterns(aPattern.Value + ":mod" + CStr(lInValue)).Start
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
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)

End Function

Public Function Trim_Digital_LinearFullTarget(aPattern As Pattern, aCapPins As PinList, aFrom As Long, aTo As Long, aCount As Long, aTarget As Long) As Long
    Dim lMeasurements() As New SiteLong
    Dim lResults As New SiteLong
    Dim lValue As New SiteBoolean
    Dim lInValue As Long
    Dim lIncrement As Long
    Dim i As Long
    Dim Site As Variant
    Dim lOutValue As New SiteLong
    Dim lDeltas() As Long
    Dim lAllValid As Boolean
    Dim lClosestDeltaSoFar As Long
    Dim lClosestIndexSoFar As Long
    
    lResults(-1) = -999
    ReDim lMeasurements(aCount - 1)
    ReDim lDeltas(aCount - 1)
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)

    With TheHdw.Digital.HRAM
        .SetTrigger trigFirst, False, 0, True
        .CaptureType = captSTV
        .Size = 8
    End With
        
    lInValue = aFrom
    lIncrement = (aTo - aFrom) / (aCount - 1)
    
    For i = 0 To aCount - 1
        lMeasurements(i) = OneMeasurement(aPattern, aPattern.Value + ":hram_mod" + CStr(lInValue), aCapPins, lInValue)
        lInValue = lInValue + lIncrement
    Next i
    
    For Each Site In TheExec.Sites
        For i = 0 To aCount - 1
            lDeltas(i) = lMeasurements(i).Subtract(aTarget).Abs
        Next i
        lClosestDeltaSoFar = lDeltas(0)
        lClosestIndexSoFar = 0
        For i = 0 To aCount - 1
            If Abs(lDeltas(i)) < Abs(lClosestDeltaSoFar) Then
                lClosestDeltaSoFar = lDeltas(i)
                lClosestIndexSoFar = i
            End If
        Next i
        lResults(Site) = lMeasurements(lClosestIndexSoFar)(Site)
    Next Site
    
    With TheHdw.Digital.HRAM
        .SetTrigger trigNever, False, 0, True
        .CaptureType = captNone
        .Size = 0
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function

Public Function Trim_Digital_BinaryTrip(aPattern As Pattern, aFrom As Long, aTo As Long, aMinDelta As Long, aInvertedOutput As Boolean) As Long
    Dim lResults As New SiteLong
    Dim lInValue As New SiteLong
    Dim lValue As New SiteBoolean
    Dim lAlwaysTripped As New SiteBoolean
    Dim lModuleCount As Long
    Dim lFromPerSite As New SiteLong
    Dim lToPerSite As New SiteLong
    Dim lStepSize As New SiteLong
    Dim Site As Variant
    Dim lPatSpec As New SiteVariant
    Dim lName As String
    Dim i As Long
    Dim lNotDone As New SiteBoolean
    
    lModuleCount = 256
    lAlwaysTripped(-1) = True
    lResults(-1) = -999
    lFromPerSite(-1) = aFrom
    lToPerSite(-1) = aTo
    lNotDone(-1) = True
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    For i = 0 To lModuleCount - 1
        lName = aPattern.Value + ":mod" + CStr(i)
        TheHdw.Patterns(lName).Threading.Enable = True
        TheHdw.Patterns(lName).ValidateThreading
    Next i
        
    Do
        For Each Site In TheExec.Sites
            lInValue(Site) = (lFromPerSite(Site) + lToPerSite(Site)) / 2
            lPatSpec(Site) = aPattern.Value + ":mod" + CStr(lInValue(Site))
        Next Site
        Call TheHdw.PatternsPerSite(lPatSpec).Start
        Call TheHdw.PatternsPerSite(lPatSpec).HaltWait
        lValue = TheHdw.Digital.Patgen.PatternBurstPassedPerSite

        For Each Site In TheExec.Sites.Active
            If lValue Then
                lResults = lInValue
                lStepSize(Site) = lToPerSite(Site) - lInValue(Site)
                lToPerSite(Site) = lInValue(Site)
            Else
                lAlwaysTripped = False
                lStepSize(Site) = lInValue(Site) - lFromPerSite(Site)
                lFromPerSite(Site) = lInValue(Site)
            End If
            lNotDone(Site) = lStepSize(Site) >= aMinDelta
        Next Site
    Loop While lNotDone.Any(True)
    
    For Each Site In TheExec.Sites.Active
        If lAlwaysTripped Then lResults = -999
    Next Site

    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function

Public Function Trim_Digital_BinaryTarget(aPattern As Pattern, aCapPins As PinList, aFrom As Long, aTo As Long, aMinDelta As Long, aInvertedOutput As Boolean, aTarget As Long) As Long
    Dim lResults As New SiteLong
    Dim lInValue As New SiteLong
    Dim lValue As New SiteLong
    Dim lModuleCount As Long
    Dim lFromPerSite As New SiteLong
    Dim lToPerSite As New SiteLong
    Dim lStepSize As New SiteLong
    Dim lNotDone As New SiteBoolean
    Dim Site As Variant
    Dim lPatSpec As New SiteVariant
    Dim lName As String
    Dim lDevAbsBest As New SiteLong
    Dim i As Long
    Dim lDevAbs As Long
    Dim lFirst As Boolean
    
    lModuleCount = 256
    lResults(-1) = -999
    lDevAbsBest(-1) = 0
    lFirst = True
    lFromPerSite(-1) = aFrom
    lToPerSite(-1) = aTo
    lNotDone(-1) = True
    
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
    
    For i = 0 To lModuleCount - 1
        lName = aPattern.Value + ":hram_mod" + CStr(i)
        TheHdw.Patterns(lName).Threading.Enable = True
        TheHdw.Patterns(lName).ValidateThreading
    Next i
    
    With TheHdw.Digital.HRAM
        .SetTrigger trigFirst, False, 0, True
        .CaptureType = captSTV
        .Size = 8
    End With
        
    Do
        For Each Site In TheExec.Sites
            lInValue(Site) = (lFromPerSite(Site) + lToPerSite(Site)) / 2
            lPatSpec(Site) = aPattern.Value + ":hram_mod" + CStr(lInValue(Site))
        Next Site
        Call TheHdw.PatternsPerSite(lPatSpec).Start
        Call TheHdw.PatternsPerSite(lPatSpec).HaltWait
        lValue = OneMeasurementPatternsPerSite(lPatSpec, aCapPins, lInValue)

        For Each Site In TheExec.Sites.Active
            lDevAbs = Abs(lValue(Site) - aTarget)
            If (lDevAbs < lDevAbsBest(Site)) Or lFirst Then
                lDevAbsBest(Site) = lDevAbs
                lResults(Site) = lInValue(Site)
            End If
            
            If (lValue(Site) > aTarget) Then
                lStepSize(Site) = lToPerSite(Site) - lInValue(Site)
                lToPerSite(Site) = lInValue(Site)
            Else
                lStepSize(Site) = lInValue(Site) - lFromPerSite(Site)
                lFromPerSite(Site) = lInValue(Site)
            End If
            lNotDone(Site) = lStepSize(Site) >= aMinDelta
        Next Site
        lFirst = False
    Loop While lNotDone.Any(True)

    With TheHdw.Digital.HRAM
        .SetTrigger trigNever, False, 0, True
        .CaptureType = captNone
        .Size = 0
    End With
    
    Call TheExec.Flow.TestLimit(ResultVal:=lResults, ForceResults:=tlForceFlow)
End Function

Private Function OneMeasurement(aPattern As Pattern, aModName As String, aCapPins As PinList, aInValue As Long) As SiteLong
    Set OneMeasurement = New SiteLong
    Dim lResult As New SiteLong
    Dim lHramWords() As New SiteLong
    Dim Site As Variant
    
    TheHdw.Patterns(aModName).Start
    TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPattern).TimeDomains).Patgen.HaltWait
    lHramWords = TheHdw.Digital.Pins(aCapPins).HRAM.ReadDataWord(0, 8, 8, tlBitOrderLsbFirst)
    For Each Site In TheExec.Sites
        OneMeasurement = IIf(True, aInValue, lHramWords(0)) 'Simulate data process
    Next Site
    
End Function

Private Function OneMeasurementPatternsPerSite(aPatSpec As SiteVariant, aCapPins As PinList, aModIndex As SiteLong) As SiteLong
    Set OneMeasurementPatternsPerSite = New SiteLong
    Dim lResult As New SiteLong
    Dim lHramWords() As New SiteLong
    Dim Site As Variant
    
    TheHdw.PatternsPerSite(aPatSpec).Start
    TheHdw.PatternsPerSite(aPatSpec).HaltWait
    lHramWords = TheHdw.Digital.Pins(aCapPins).HRAM.ReadDataWord(0, 8, 8, tlBitOrderLsbFirst)
    For Each Site In TheExec.Sites
        OneMeasurementPatternsPerSite = IIf(True, aModIndex(Site), lHramWords(0)) 'Simulate data process
    Next Site
    
End Function
