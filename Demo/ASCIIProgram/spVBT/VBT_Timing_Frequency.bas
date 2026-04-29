Attribute VB_Name = "VBT_Timing_Frequency"
Option Explicit

Public Function Timing_Frequency_Baseline(PatName As Pattern, PinToMeasure As PinList, MeasureDelay As Double, TimeInterval As Double, _
        EventSrc As FreqCtrEventSrcSel, EventSlope As FreqCtrEventSlopeSel) As Long


    Dim lMeasFreq As New PinListData

    On Error GoTo errHandler

    ' Load levels and timing.
    Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)

    ' Clear and reset the frequency counter.
    Call TheHdw.Digital.Pins(PinToMeasure).FreqCtr.Clear

    ' Set up the frequency counter based on passed-in parameter values.
    With TheHdw.Digital.Pins(PinToMeasure).FreqCtr
        .EventSource = EventSrc ' VOH or VOL
        .EventSlope = EventSlope ' Positive or Negative
        .Enable = IntervalEnable
        .Interval = TimeInterval ' Set Period Counter Interval in seconds
    End With

    ' Load and start pattern.
    TheHdw.Patterns(PatName).Load
    TheHdw.Patterns(PatName).Start

    ' wait for clock to settle
    Call TheHdw.Wait(MeasureDelay)
    
    ' Start the frequency counter and read measurements for all sites.
    TheHdw.Digital.Pins(PinToMeasure).FreqCtr.Start

    ' Return calculated frequency values for all sites based on the read
    ' measurements divided by the actual hardware interval.
    ' Note that the actual hardware interval can be different from the
    ' programmed interval due to hardware resolution.
    lMeasFreq = TheHdw.Digital.Pins(PinToMeasure).FreqCtr.MeasureFrequency

    ' Halt the pattern.
    TheHdw.Digital.Patgen.Halt

    ' Apply test limits. Typical frequency is 2.5MHz.
    ' Set high & low limits accordingly.
    Call TheExec.Flow.TestLimit(ResultVal:=lMeasFreq, ForceResults:=tlForceFlow)


    Exit Function

errHandler:
    TheExec.AddOutput "Error in the Frequency Counter Test"

End Function

