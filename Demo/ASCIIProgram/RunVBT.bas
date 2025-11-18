Attribute VB_Name = "RunVBT"
' This ALWAYS GENERATED file contains wrappers for VBT tests.
' Do not edit.

Public Sub COD_Check()

End Sub

Private Sub HandleUntrappedError()
    ' Sanity clause
    If TheExec Is Nothing Then
        MsgBox "IG-XL is not running!  VBT tests cannot execute unless IG-XL is running."
        Exit Sub
    End If
    ' If the last site has failed out, let's ignore the error
    If TheExec.Sites.Active.Count = 0 Then Exit Sub  ' don't log the error
    ' If in a legacy site loop, make sure to complete it. (For-Each site syntax in IG-XL 6.10 aborts gracefully.)
    Do While TheExec.Sites.InSiteLoop
        Call TheExec.Sites.SelectNext(loopTop) '  Legacy syntax (hidden)
    Loop
    ' Select all active sites in case a subset of sites was selected when error occurred.
    TheExec.Sites.Selected = TheExec.Sites.Active
    ' Log the error to the IG-XL Error logging mechanism (tells Flow to fail the test)
    AbortTest
End Sub

Public Function Empty_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New InterposeName
    p1.Value = v(0)
    Dim p2 As New InterposeName
    p2.Value = v(1)
    Dim p3 As New InterposeName
    p3.Value = v(2)
    Dim p4 As New InterposeName
    p4.Value = v(3)
    Dim p5 As New InterposeName
    p5.Value = v(4)
    Dim p6 As New InterposeName
    p6.Value = v(5)
    Dim p7 As New PinList
    p7.Value = v(12)
    Dim p8 As New PinList
    p8.Value = v(13)
    Dim p9 As New PinList
    p9.Value = v(14)
    Dim p10 As New PinList
    p10.Value = v(15)
    Dim p11 As New PinList
    p11.Value = v(16)
    Dim p12 As New PinList
    p12.Value = v(17)
    Dim p13 As New PinList
    p13.Value = v(18)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Empty_T__ = Template.VBT_Empty_T.Empty_T(p1, p2, p3, p4, p5, p6, CStr(v(6)), CStr(v(7)), CStr(v(8)), CStr(v(9)), CStr(v(10)), CStr(v(11)), p7, p8, p9, p10, p11, p12, p13, pStep, CBool(v(19)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function Functional_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    Dim p2 As New InterposeName
    p2.Value = v(1)
    Dim p3 As New InterposeName
    p3.Value = v(2)
    Dim p4 As New InterposeName
    p4.Value = v(3)
    Dim p5 As New InterposeName
    p5.Value = v(4)
    Dim p6 As New InterposeName
    p6.Value = v(5)
    Dim p7 As New InterposeName
    p7.Value = v(6)
    Dim p8 As PFType
    p8 = v(7)
    Dim p9 As tlResultMode
    p9 = v(8)
    Dim p10 As New PinList
    p10.Value = v(9)
    Dim p11 As New PinList
    p11.Value = v(10)
    Dim p12 As New PinList
    p12.Value = v(11)
    Dim p13 As New PinList
    p13.Value = v(12)
    Dim p14 As New PinList
    p14.Value = v(13)
    Dim p15 As New PinList
    p15.Value = v(20)
    Dim p16 As New PinList
    p16.Value = v(21)
    Dim p17 As New InterposeName
    p17.Value = v(22)
    Dim p18 As tlRelayMode
    p18 = v(24)
    Dim p19 As tlWaitVal
    p19 = v(27)
    Dim p20 As tlWaitVal
    p20 = v(28)
    Dim p21 As tlWaitVal
    p21 = v(29)
    Dim p22 As tlWaitVal
    p22 = v(30)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Dim p23 As tlPatConcurrentMode
    p23 = v(34)
    Dim p24 As tlTemplateScanFailDataLogging
    p24 = v(35)
    Dim p25 As tlDigitalCMEMCaptureLimitMode
    p25 = v(36)
    Dim p26 As tlTemplateScanPinListSource
    p26 = v(38)
    Dim p27 As New PinList
    p27.Value = v(39)
    Dim p28 As tlTemplateScanCaptureFormat
    p28 = v(40)
    Dim p29 As tlTemplateScanCaptureDataType
    p29 = v(41)
    Dim p30 As tlTemplateScanUserCommentSource
    p30 = v(42)
    Dim p31 As tlTemplateATPGPinMapSource
    p31 = v(44)
    Functional_T__ = Template.VBT_Functional_T.Functional_T(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, CStr(v(14)), CStr(v(15)), CStr(v(16)), CStr(v(17)), CStr(v(18)), CStr(v(19)), p15, p16, p17, CStr(v(23)), p18, CBool(v(25)), CBool(v(26)), p19, p20, p21, p22, CBool(v(UBound(v))), CStr(v(32)), pStep, CStr(v(33)), p23, p24, p25, CLng(v(37)), p26, p27, p28, p29, p30, CStr(v(43)), p31, CStr(v(45)), CStr(v(46)), CBool(v(47)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function PinPMU_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New InterposeName
    p1.Value = v(1)
    Dim p2 As New InterposeName
    p2.Value = v(2)
    Dim p3 As New InterposeName
    p3.Value = v(3)
    Dim p4 As New InterposeName
    p4.Value = v(4)
    Dim p5 As New InterposeName
    p5.Value = v(5)
    Dim p6 As New InterposeName
    p6.Value = v(6)
    Dim p7 As New Pattern
    p7.Value = v(7)
    Dim p8 As New Pattern
    p8.Value = v(8)
    Dim p9 As New PinList
    p9.Value = v(10)
    Dim p10 As New PinList
    p10.Value = v(11)
    Dim p11 As New PinList
    p11.Value = v(12)
    Dim p12 As New PinList
    p12.Value = v(13)
    Dim p13 As New PinList
    p13.Value = v(14)
    Dim p14 As New PinList
    p14.Value = v(15)
    Dim p15 As tlPPMUMode
    p15 = v(16)
    Dim p16 As New FormulaArg
    p16.Value = v(18)
    Dim p17 As New FormulaArg
    p17.Value = v(19)
    Dim p18 As tlPPMURelayMode
    p18 = v(20)
    Dim p19 As New PinList
    p19.Value = v(36)
    Dim p20 As New PinList
    p20.Value = v(37)
    Dim p21 As tlWaitVal
    p21 = v(38)
    Dim p22 As tlWaitVal
    p22 = v(39)
    Dim p23 As tlWaitVal
    p23 = v(40)
    Dim p24 As tlWaitVal
    p24 = v(41)
    Dim p25 As tlPPMUMode
    p25 = v(49)
    Dim p26 As New FormulaArg
    p26.Value = v(52)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Dim p27 As New PinList
    p27.Value = v(53)
    Dim p28 As tlPPMUMode
    p28 = v(54)
    Dim p29 As New FormulaArg
    p29.Value = v(55)
    PinPMU_T__ = Template.VBT_PinPmu_T.PinPMU_T(CStr(v(0)), p1, p2, p3, p4, p5, p6, p7, p8, CStr(v(9)), p9, p10, p11, p12, p13, p14, p15, CDbl(v(17)), p16, p17, p18, CStr(v(21)), CStr(v(22)), CStr(v(23)), CStr(v(24)), CStr(v(25)), CStr(v(26)), CStr(v(27)), CStr(v(28)), CStr(v(29)), CStr(v(30)), CDbl(v(31)), CLng(v(32)), CBool(v(33)), CStr(v(34)), CStr(v(35)), p19, p20, p21, p22, p23, p24, CBool(v(UBound(v))), CStr(v(43)), CStr(v(44)), , CStr(v(45)), CBool(v(46)), CBool(v(47)), CBool(v(48)), p25, CStr(v(50)), CStr(v(51)), p26, pStep, p27, p28, p29, CStr(v(56)), CStr(v(57)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function DCVSPowerSupply_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    Dim p2 As New InterposeName
    p2.Value = v(1)
    Dim p3 As New InterposeName
    p3.Value = v(2)
    Dim p4 As New InterposeName
    p4.Value = v(3)
    Dim p5 As New InterposeName
    p5.Value = v(4)
    Dim p6 As New InterposeName
    p6.Value = v(5)
    Dim p7 As New InterposeName
    p7.Value = v(6)
    Dim p8 As New Pattern
    p8.Value = v(7)
    Dim p9 As New PinList
    p9.Value = v(8)
    Dim p10 As New PinList
    p10.Value = v(9)
    Dim p11 As New PinList
    p11.Value = v(10)
    Dim p12 As New PinList
    p12.Value = v(11)
    Dim p13 As New PinList
    p13.Value = v(12)
    Dim p14 As New PinList
    p14.Value = v(16)
    Dim p15 As tlPSSource
    p15 = v(17)
    Dim p16 As tlCommonRelayMode
    p16 = v(31)
    Dim p17 As New PinList
    p17.Value = v(32)
    Dim p18 As New PinList
    p18.Value = v(33)
    Dim p19 As tlPSTestControl
    p19 = v(34)
    Dim p20 As tlWaitVal
    p20 = v(35)
    Dim p21 As tlWaitVal
    p21 = v(36)
    Dim p22 As tlWaitVal
    p22 = v(37)
    Dim p23 As tlWaitVal
    p23 = v(38)
    Dim p24 As New FormulaArg
    p24.Value = v(40)
    Dim p25 As New FormulaArg
    p25.Value = v(41)
    Dim p26 As New FormulaArg
    p26.Value = v(42)
    Dim p27 As New FormulaArg
    p27.Value = v(43)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Dim p28 As New FormulaArg
    p28.Value = v(47)
    DCVSPowerSupply_T__ = Template.VBT_DCVSPowerSupply_T.DCVSPowerSupply_T(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, CDbl(v(13)), CLng(v(14)), CStr(v(15)), p14, p15, CStr(v(18)), CStr(v(19)), CStr(v(20)), CStr(v(21)), CStr(v(22)), CStr(v(23)), CStr(v(24)), CStr(v(25)), CStr(v(26)), CStr(v(27)), CStr(v(28)), CStr(v(29)), CBool(v(30)), p16, p17, p18, p19, p20, p21, p22, p23, CBool(v(UBound(v))), p24, p25, p26, p27, , CStr(v(44)), CBool(v(45)), CBool(v(46)), pStep, p28, CBool(v(48)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function CTO_ADC_AC_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(1)
    Dim p3 As New PinList
    p3.Value = v(2)
    Dim p4 As New PinList
    p4.Value = v(3)
    Dim p5 As tlACEncoding
    p5 = v(7)
    Dim p6 As tlACVoltageRange
    p6 = v(8)
    Dim p7 As tlRelayMode
    p7 = v(9)
    Dim p8 As tlCTOSourceBandwidth
    p8 = v(12)
    Dim p9 As New Pattern
    p9.Value = v(20)
    Dim p10 As New Pattern
    p10.Value = v(42)
    Dim p11 As New PinList
    p11.Value = v(47)
    Dim p12 As New PinList
    p12.Value = v(48)
    Dim p13 As New PinList
    p13.Value = v(49)
    Dim p14 As New PinList
    p14.Value = v(50)
    Dim p15 As New PinList
    p15.Value = v(51)
    Dim p16 As New PinList
    p16.Value = v(52)
    Dim p17 As New PinList
    p17.Value = v(53)
    Dim p18 As New InterposeName
    p18.Value = v(54)
    Dim ExtraArgs(0 To 10) As Variant
    Dim i As Integer
    For i = 0 To 10
        ExtraArgs(i) = v(56 + i)
    Next i
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    CTO_ADC_AC_T__ = Template.VBT_CTO_ADC_AC_T.CTO_ADC_AC_T(p1, p2, p3, p4, CDbl(v(4)), CDbl(v(5)), CLng(v(6)), p5, p6, p7, CBool(v(10)), CDbl(v(11)), p8, CBool(v(13)), CDbl(v(14)), CLng(v(15)), CDbl(v(16)), CDbl(v(17)), CDbl(v(18)), CBool(v(19)), p9, CBool(v(21)), CStr(v(22)), CBool(v(23)), CStr(v(24)), CBool(v(25)), CStr(v(26)), CBool(v(27)), CStr(v(28)), CBool(v(29)), CStr(v(30)), CBool(v(31)), CStr(v(32)), CBool(v(33)), CStr(v(34)), CBool(v(35)), CStr(v(36)), CBool(v(37)), CStr(v(38)), CBool(v(39)), CStr(v(40)), CLng(v(41)), p10, CStr(v(43)), CStr(v(44)), CBool(v(45)), CBool(v(46)), p11, p12, p13, p14, p15, p16, p17, p18, ExtraArgs, CBool(v(UBound(v))), pStep, CStr(v(56)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function CTO_ADC_DC_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(1)
    Dim p3 As New PinList
    p3.Value = v(2)
    Dim p4 As New PinList
    p4.Value = v(3)
    Dim p5 As tlDCEncoding
    p5 = v(7)
    Dim p6 As tlDCTransitionPoint
    p6 = v(8)
    Dim p7 As tlDCVoltageRange
    p7 = v(9)
    Dim p8 As tlRelayMode
    p8 = v(10)
    Dim p9 As tlCTOSourceBandwidth
    p9 = v(13)
    Dim p10 As tlDCInputSignal
    p10 = v(15)
    Dim p11 As tlDCAnalysisAlgorithm
    p11 = v(17)
    Dim p12 As tlNormalizationMethod
    p12 = v(21)
    Dim p13 As New Pattern
    p13.Value = v(23)
    Dim p14 As tlLimitUnit
    p14 = v(28)
    Dim p15 As tlLimitUnit
    p15 = v(33)
    Dim p16 As tlLimitUnit
    p16 = v(38)
    Dim p17 As tlLimitUnit
    p17 = v(43)
    Dim p18 As New Pattern
    p18.Value = v(48)
    Dim ExtraArgs(0 To 21) As Variant
    Dim i As Integer
    For i = 0 To 21
        ExtraArgs(i) = v(51 + i)
    Next i
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    CTO_ADC_DC_T__ = Template.VBT_CTO_ADC_DC_T.CTO_ADC_DC_T(p1, p2, p3, p4, CDbl(v(4)), CDbl(v(5)), CLng(v(6)), p5, p6, p7, p8, CBool(v(11)), CDbl(v(12)), p9, CBool(v(14)), p10, CStr(v(16)), p11, CDbl(v(18)), CLng(v(19)), CLng(v(20)), p12, CBool(v(22)), p13, CBool(v(24)), CStr(v(25)), CBool(v(26)), CStr(v(27)), p14, CBool(v(29)), CStr(v(30)), CBool(v(31)), CStr(v(32)), p15, CBool(v(34)), CStr(v(35)), CBool(v(36)), CStr(v(37)), p16, CBool(v(39)), CStr(v(40)), CBool(v(41)), CStr(v(42)), p17, CBool(v(44)), CStr(v(45)), CBool(v(46)), CStr(v(47)), p18, CStr(v(49)), ExtraArgs, CBool(v(UBound(v))), pStep, CStr(v(51)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function DCVIPowerSupply_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    Dim p2 As New InterposeName
    p2.Value = v(1)
    Dim p3 As New InterposeName
    p3.Value = v(2)
    Dim p4 As New InterposeName
    p4.Value = v(3)
    Dim p5 As New InterposeName
    p5.Value = v(4)
    Dim p6 As New InterposeName
    p6.Value = v(5)
    Dim p7 As New InterposeName
    p7.Value = v(6)
    Dim p8 As New Pattern
    p8.Value = v(7)
    Dim p9 As New PinList
    p9.Value = v(8)
    Dim p10 As New PinList
    p10.Value = v(9)
    Dim p11 As New PinList
    p11.Value = v(10)
    Dim p12 As New PinList
    p12.Value = v(11)
    Dim p13 As New PinList
    p13.Value = v(17)
    Dim p14 As New PinList
    p14.Value = v(18)
    Dim p15 As tlPSSource
    p15 = v(19)
    Dim p16 As tlRelayMode
    p16 = v(34)
    Dim p17 As New PinList
    p17.Value = v(35)
    Dim p18 As New PinList
    p18.Value = v(36)
    Dim p19 As tlPSTestControl
    p19 = v(37)
    Dim p20 As New InterposeName
    p20.Value = v(39)
    Dim p21 As tlWaitVal
    p21 = v(41)
    Dim p22 As tlWaitVal
    p22 = v(42)
    Dim p23 As tlWaitVal
    p23 = v(43)
    Dim p24 As tlWaitVal
    p24 = v(44)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    DCVIPowerSupply_T__ = Template.VBT_DCVIPowerSupply_T.DCVIPowerSupply_T(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, CDbl(v(12)), CLng(v(13)), CStr(v(14)), CDbl(v(15)), CDbl(v(16)), p13, p14, p15, CStr(v(20)), CStr(v(21)), CStr(v(22)), CStr(v(23)), CStr(v(24)), CStr(v(25)), CStr(v(26)), CStr(v(27)), CStr(v(28)), CStr(v(29)), CStr(v(30)), CDbl(v(31)), CStr(v(32)), CBool(v(33)), p16, p17, p18, p19, CBool(v(38)), p20, CStr(v(40)), p21, p22, p23, p24, CBool(v(UBound(v))), CStr(v(46)), , CStr(v(47)), CBool(v(48)), CBool(v(49)), pStep, CStr(v(50)), CBool(v(51)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function Continuity__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(1)
    Continuity__ = VBAProject.VBT_Continuity.Continuity(p1, p2, CDbl(v(2)), CDbl(v(3)), CDbl(v(4)), CDbl(v(5)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function Start_Current_profile__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Start_Current_profile__ = VBAProject.VBT_DCVS_Profile.Start_Current_profile(p1, CDbl(v(1)), CLng(v(2)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function Start_Voltage_profile__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Start_Voltage_profile__ = VBAProject.VBT_DCVS_Profile.Start_Voltage_profile(p1, CDbl(v(1)), CLng(v(2)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function start_profile__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    start_profile__ = VBAProject.VBT_DCVS_Profile.start_profile(p1, CStr(v(1)), CDbl(v(2)), CLng(v(3)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function Plot_profile__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Plot_profile__ = VBAProject.VBT_DCVS_Profile.Plot_profile(p1)
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function I_Meter_setup_strobe_readbk__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    I_Meter_setup_strobe_readbk__ = VBAProject.VBT_DCVS_Profile.I_Meter_setup_strobe_readbk()
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function FunctionalVB__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    FunctionalVB__ = VBAProject.VBT_Functional.FunctionalVB(p1)
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function Icc_static__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(3)
    Dim p3 As New PinList
    p3.Value = v(4)
    Icc_static__ = VBAProject.VBT_Icc.Icc_static(p1, CDbl(v(1)), CDbl(v(2)), p2, p3, CDbl(v(5)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function icc_dynamic_vbt__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(1)
    Dim p3 As New PinList
    p3.Value = v(4)
    icc_dynamic_vbt__ = VBAProject.VBT_Icc.icc_dynamic_vbt(p1, p2, CDbl(v(2)), CDbl(v(3)), p3)
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function dcvs_pop__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New Pattern
    p2.Value = v(1)
    dcvs_pop__ = VBAProject.vbt_pop.dcvs_pop(p1, p2, CDbl(v(2)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function CreatePset__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    CreatePset__ = VBAProject.vbt_pop.CreatePset()
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function vbt_pop__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    vbt_pop__ = VBAProject.vbt_pop.vbt_pop()
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function SeqLeakage__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New PinList
    p1.Value = v(0)
    Dim p2 As New PinList
    p2.Value = v(4)
    Dim p3 As New PinList
    p3.Value = v(5)
    SeqLeakage__ = VBAProject.VBT_Seq_Leakage.SeqLeakage(p1, CDbl(v(1)), CDbl(v(2)), CDbl(v(3)), p2, p3, CDbl(v(6)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function ValidateSystemSetup_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    ValidateSystemSetup_T__ = OasisXLA.VBT_ConfigCheck.ValidateSystemSetup_T(CStr(v(0)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function IGSim_Functional_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New Pattern
    p1.Value = v(0)
    Dim p2 As New InterposeName
    p2.Value = v(1)
    Dim p3 As New InterposeName
    p3.Value = v(2)
    Dim p4 As New InterposeName
    p4.Value = v(3)
    Dim p5 As New InterposeName
    p5.Value = v(4)
    Dim p6 As New InterposeName
    p6.Value = v(5)
    Dim p7 As New InterposeName
    p7.Value = v(6)
    Dim p8 As PFType
    p8 = v(7)
    Dim p9 As tlResultMode
    p9 = v(8)
    Dim p10 As New PinList
    p10.Value = v(9)
    Dim p11 As New PinList
    p11.Value = v(10)
    Dim p12 As New PinList
    p12.Value = v(11)
    Dim p13 As New PinList
    p13.Value = v(12)
    Dim p14 As New PinList
    p14.Value = v(13)
    Dim p15 As New PinList
    p15.Value = v(20)
    Dim p16 As New PinList
    p16.Value = v(21)
    Dim p17 As New InterposeName
    p17.Value = v(22)
    Dim p18 As tlRelayMode
    p18 = v(24)
    Dim p19 As tlWaitVal
    p19 = v(27)
    Dim p20 As tlWaitVal
    p20 = v(28)
    Dim p21 As tlWaitVal
    p21 = v(29)
    Dim p22 As tlWaitVal
    p22 = v(30)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Dim p23 As tlPatConcurrentMode
    p23 = v(34)
    Dim p24 As tlTemplateScanFailDataLogging
    p24 = v(35)
    Dim p25 As tlDigitalCMEMCaptureLimitMode
    p25 = v(36)
    Dim p26 As tlTemplateScanPinListSource
    p26 = v(38)
    Dim p27 As New PinList
    p27.Value = v(39)
    Dim p28 As tlTemplateScanCaptureFormat
    p28 = v(40)
    Dim p29 As tlTemplateScanCaptureDataType
    p29 = v(41)
    Dim p30 As tlTemplateScanUserCommentSource
    p30 = v(42)
    Dim p31 As tlTemplateATPGPinMapSource
    p31 = v(44)
    IGSim_Functional_T__ = OasisXLA.VBT_IGSIM_FUNCTIONAL_T.IGSim_Functional_T(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, CStr(v(14)), CStr(v(15)), CStr(v(16)), CStr(v(17)), CStr(v(18)), CStr(v(19)), p15, p16, p17, CStr(v(23)), p18, CBool(v(25)), CBool(v(26)), p19, p20, p21, p22, CBool(v(UBound(v))), CStr(v(32)), pStep, CStr(v(33)), p23, p24, p25, CLng(v(37)), p26, p27, p28, p29, p30, CStr(v(43)), p31, CStr(v(45)), CStr(v(46)), CBool(v(47)), CStr(v(48)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function DatalogType__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    DatalogType__ = OasisXLA.VBT_IGSIM_FUNCTIONAL_T.DatalogType()
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function PostTest__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    tl_dt_ErrorMsg "Unsupported type at function PostTest"

    ' Call OasisXLA.VBT_IGSIM_FUNCTIONAL_T.PostTest(*One or more unsupported types in argument list or non Long/Integer return type*)
    PostTest__ = TL_SUCCESS
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function

Public Function getdefaults__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    tl_dt_ErrorMsg "Unsupported type at function getdefaults"

    ' getdefaults__ = OasisXLA.VBT_IGSIM_FUNCTIONAL_T.getdefaults(*One or more unsupported types in argument list or non Long/Integer return type*)
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































Public Function IGSIM_PinPMU_T__(v As Variant) As Long
    m_STDSvcClient.ProfileService.OverrideEnabled = True
    If TheExec.RunMode = runModeProduction Or tl_IsRunningSynchronus Or errDestLogfile = TheExec.ErrorOutputMode Then On Error GoTo errpt
    m_STDSvcClient.ProfileService.OverrideEnabled = False
    Dim p1 As New InterposeName
    p1.Value = v(1)
    Dim p2 As New InterposeName
    p2.Value = v(2)
    Dim p3 As New InterposeName
    p3.Value = v(3)
    Dim p4 As New InterposeName
    p4.Value = v(4)
    Dim p5 As New InterposeName
    p5.Value = v(5)
    Dim p6 As New InterposeName
    p6.Value = v(6)
    Dim p7 As New Pattern
    p7.Value = v(7)
    Dim p8 As New Pattern
    p8.Value = v(8)
    Dim p9 As New PinList
    p9.Value = v(10)
    Dim p10 As New PinList
    p10.Value = v(11)
    Dim p11 As New PinList
    p11.Value = v(12)
    Dim p12 As New PinList
    p12.Value = v(13)
    Dim p13 As New PinList
    p13.Value = v(14)
    Dim p14 As New PinList
    p14.Value = v(15)
    Dim p15 As tlPPMUMode
    p15 = v(16)
    Dim p16 As New FormulaArg
    p16.Value = v(18)
    Dim p17 As New FormulaArg
    p17.Value = v(19)
    Dim p18 As tlPPMURelayMode
    p18 = v(20)
    Dim p19 As New PinList
    p19.Value = v(36)
    Dim p20 As New PinList
    p20.Value = v(37)
    Dim p21 As tlWaitVal
    p21 = v(38)
    Dim p22 As tlWaitVal
    p22 = v(39)
    Dim p23 As tlWaitVal
    p23 = v(40)
    Dim p24 As tlWaitVal
    p24 = v(41)
    Dim p25 As tlPPMUMode
    p25 = v(49)
    Dim p26 As New FormulaArg
    p26.Value = v(52)
    Dim pStep As SubType
    pStep = TheExec.Flow.StepType
    Dim p27 As New PinList
    p27.Value = v(53)
    Dim p28 As tlPPMUMode
    p28 = v(54)
    Dim p29 As New FormulaArg
    p29.Value = v(55)
    IGSIM_PinPMU_T__ = OasisXLA.VBT_IGSIM_PinPMU_T.IGSIM_PinPMU_T(CStr(v(0)), p1, p2, p3, p4, p5, p6, p7, p8, CStr(v(9)), p9, p10, p11, p12, p13, p14, p15, CDbl(v(17)), p16, p17, p18, CStr(v(21)), CStr(v(22)), CStr(v(23)), CStr(v(24)), CStr(v(25)), CStr(v(26)), CStr(v(27)), CStr(v(28)), CStr(v(29)), CStr(v(30)), CDbl(v(31)), CLng(v(32)), CBool(v(33)), CStr(v(34)), CStr(v(35)), p19, p20, p21, p22, p23, p24, CBool(v(UBound(v))), CStr(v(43)), CStr(v(44)), , CStr(v(45)), CBool(v(46)), CBool(v(47)), CBool(v(48)), p25, CStr(v(50)), CStr(v(51)), p26, pStep, p27, p28, p29, CStr(v(56)), CStr(v(57)))
    Exit Function
errpt:     ' Untrapped VB error in production.  Fail the test.
    HandleUntrappedError
End Function









































