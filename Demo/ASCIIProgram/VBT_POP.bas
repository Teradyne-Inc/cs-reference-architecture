Attribute VB_Name = "VBT_POP"
Option Explicit


Public Function dcvs_pop(PowerPin As PinList, _
                          PatName As Pattern, _
                          NumMeasPoints As Double) As Long

    Dim DCVS_Meas As New PinListData
    Dim MeasValue As Double, TestNumber As Long, Site As Variant
    Dim Channel As String, i As Double

    ' apply HSD levels and PowerSupply pin values, timing and init states
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
     
    '=====  setup DCVS parameters that can not be controlled by the PSet
    With TheHdw.DCVS.Pins(PowerPin)
        .Gate = False
        .Voltage.Output = tlDCVSVoltageMain
        .Mode = tlDCVSModeVoltage
        ' .CurrentLimit.Source.FoldLimit.TimeOut.Value = 0.5
        .CurrentLimit.Source.FoldLimit.Level.Value = 0.625
        '.LocalCapacitor = tlDCVSOff
        .Gate = True
    End With

    '===== After Debug, move "Call CreatePset()" to
    '===== ExecInterpose function "OnProgramValidated()" in the Exec_IP_Module
    Call CreatePset

    '===== Load & Run Pattern, wait for PAT to finish
    TheHdw.Patterns(PatName).Load
    TheHdw.Patterns(PatName).Start
    TheHdw.Digital.Patgen.HaltWait
    TheHdw.Wait 0.03
    
    '===== by stuffing the pinlistdata variable with simulation data
    '===== Setup OFFLINE Simulation
    If TheExec.TesterMode = testModeOffline Then
        DCVS_Meas.AddPin (PowerPin)
        For Each Site In TheExec.Sites
            For i = 0 To NumMeasPoints - 1
                DCVS_Meas.Pins(PowerPin).Value(Site) = 0.028 + Rnd / 99
                MeasValue = DCVS_Meas.pin(PowerPin).Value(Site)
            Next i
        Next Site
    Else
        DCVS_Meas = TheHdw.DCVS.Pins(PowerPin).Meter.Read(tlNoStrobe, NumMeasPoints, , tlDCVSMeterReadingFormatArray)
    End If

    TheExec.Flow.TestLimit ResultVal:=DCVS_Meas, ForceResults:=tlForceFlow, CompareMode:=CompareAverage, ForceVal:=5#

End Function


Public Function CreatePset() As Long

    ' 1, Add new PSets call "DCVSPSet"
    TheHdw.DCVS.Pins("vcc").PSets.Add "DCVSPSet"

    ' 2, Set parameters for DCVSPSet
    With TheHdw.DCVS.Pins("vcc").PSets.Item("DCVSPSet")
        .Voltage.Main.Value = 5#
        .CurrentRange.Value = 0.1
        .Meter.Mode = tlDCVSMeterCurrent
        '.Meter.CurrentRange.Value = 0.2
        .CurrentLimit.Sink.FoldLimit.Level.Value = 0.05
        .CurrentLimit.Source.FoldLimit.Level.Value = 0.05
        .Capture.SampleSize = 4
        .Apply
    End With

End Function


Public Function vbt_pop() As Long
    On Error GoTo errHandler

    Exit Function
errHandler:
    If AbortTest Then Exit Function Else Resume Next
End Function

