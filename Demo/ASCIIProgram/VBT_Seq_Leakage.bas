Attribute VB_Name = "VBT_Seq_Leakage"
Option Explicit


Public Function SeqLeakage(SeqLeakPins As PinList, ForceV_IiH As Double, _
        ForceV_IiL As Double, waitTime As Double, Init_HiPins As PinList, _
        Init_LoPins As PinList, Optional I_Meas_Range As Double) As Long

    Dim Site As Variant
    Dim PinArr() As String, PinCount As Long, i As Long
    Dim measVal As New PinListData

    ' Connect all signal pins (digital_pins) to the pin electronics and apply levels
    'TheHdw.Digital.ApplyLevelsTiming True, True, False, tlPowered, Init_HiPins.Value, Init_LoPins.Value
    TheHdw.PPMU.Pins(SeqLeakPins).Gate = tlOff 'insure all ppmu's are gated off

    ' separate pingroup into individual pins
    TheExec.DataManager.DecomposePinList SeqLeakPins, PinArr(), PinCount

    ' Loop for Leakage High (ForceV_IiH)
        For i = 0 To PinCount - 1

            With TheHdw.PPMU(PinArr(i))
                TheHdw.Digital.DisconnectPins (PinArr(i)) ' disconnect DUT pin from PE
                .Connect                ' connect the ppmu to DUT pin
                .Gate = tlOn            ' turn on ppmu
                .ForceV ForceV_IiH, I_Meas_Range ' force voltage, set measure and range
                TheHdw.Wait waitTime ' let force value stabilize prior to measurement
                measVal = .Read(tlPPMUReadMeasurements) 'make the measurement

        ' Setup OFFLINE Simulation by stuffing the pinlistdata variable with simulation data
                If TheExec.TesterMode = testModeOffline Then
                    For Each Site In TheExec.Sites
                        measVal.Pins(PinArr(i)).Value(Site) = 0.00000019 + (Rnd() / 25000000#)
                    Next Site
                End If

        ' test the "measVal" against the limits
                TheExec.Flow.TestLimit ResultVal:=measVal, Unit:=unitAmp, _
                        ForceVal:=ForceV_IiH, ForceUnit:=unitVolt, ForceResults:=tlForceFlow
                .Gate = tlOff 'gate the ppmu off on the tested pin
                .Disconnect  'disconnect the ppmu from the tested pin
                TheHdw.Digital.ConnectPins (PinArr(i)) 'connect the tested pin back to the PE
            End With
        Next i

    ' Loop for Leakage Low (ForceV_IiL)
        For i = 0 To PinCount - 1

            With TheHdw.PPMU(PinArr(i))
                TheHdw.Digital.DisconnectPins (PinArr(i)) ' disconnect DUT pin from PE
                .Connect                    ' connect the ppmu to DUT pin
                .Gate = tlOn                ' turn on ppmu
                .ForceV ForceV_IiL, I_Meas_Range ' force voltage, set measure and range
                TheHdw.Wait waitTime        ' let force value stabilize prior to measurement
                measVal = .Read(tlPPMUReadMeasurements) 'make the measurement

        ' Setup OFFLINE Simulation by stuffing the pinlistdata variable with simulation data
                If TheExec.TesterMode = testModeOffline Then
                    For Each Site In TheExec.Sites
                        measVal.Pins(PinArr(i)).Value(Site) = -0.000014 - (Rnd() / 110000#)
                    Next Site
                End If

        ' test the "measVal" against the limits
             TheExec.Flow.TestLimit ResultVal:=measVal, Unit:=unitAmp, _
                    ForceVal:=ForceV_IiL, ForceUnit:=unitVolt, ForceResults:=tlForceFlow
            .Gate = tlOff 'gate the ppmu off on the tested pin
            .Disconnect  'disconnect the ppmu from the tested pin
            TheHdw.Digital.ConnectPins (PinArr(i)) 'connect the tested pin back to the PE
        End With
    Next i
    TheHdw.PPMU.Pins(SeqLeakPins).Reset tlResetConnections + tlResetSettings 'reset ppmu connections and settings
    TheHdw.PPMU.Pins(SeqLeakPins).Gate = tlOff
End Function



