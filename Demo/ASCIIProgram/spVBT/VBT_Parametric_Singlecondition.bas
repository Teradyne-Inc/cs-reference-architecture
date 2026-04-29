Attribute VB_Name = "VBT_Parametric_Singlecondition"
Option Explicit

Public Function Parametric_Singlecondition_Baseline(aForcePinList As PinList, aForceMode As String, aForceValue As Double, _
                                                        aClampValue As Double, aMeasureWhat As String, aMeasureRange As Double, _
                                                        aSampleSize As Integer, Optional aMeasPinList As PinList = Null, _
                                                        Optional aWaitTime As Double = 0) As Long
Dim lMeas As New PinListData

' Pre-Body
Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
Call TheHdw.Digital.Pins(aForcePinList).Disconnect
Call TheHdw.PPMU.Pins(aForcePinList).Connect

' Body
Call TheHdw.PPMU.Pins(aForcePinList).ForceI(aForceValue, aForceValue)
TheHdw.PPMU.Pins(aForcePinList).ClampVHi = aClampValue
TheHdw.PPMU.Pins(aForcePinList).Gate = tlOn
Call TheHdw.SetSettlingTimer(aWaitTime)
lMeas = TheHdw.PPMU.Pins(aForcePinList).Read(tlPPMUReadMeasurements, aSampleSize, tlPPMUReadingFormatAverage)
TheHdw.PPMU.Pins(aForcePinList).Gate = tlOff

' Post-Body
Call TheHdw.PPMU.Pins(aForcePinList).Disconnect
Call TheHdw.Digital.Pins(aForcePinList).Disconnect
Call TheExec.Flow.TestLimit(lMeas, ForceResults:=tlForceFlow)

End Function

Public Function Parametric_Singlecondition_PreCondition_Pattern(aForcePinList As PinList, aForceMode As String, aForceValue As Double, _
                                                        aClampValue As Double, aPreconditionPattern As Pattern, aMeasureWhat As String, _
                                                        aMeasureRange As Double, aSampleSize As Integer, Optional MeasPinList As PinList = Null, _
                                                        Optional aWaitTime As Double = 0) As Long
Dim lMeas As New PinListData
                                                        
' Pre-Body
Call TheHdw.Digital.ApplyLevelsTiming(True, True, True, tlPowered)
Call TheHdw.Patterns(aPreconditionPattern).Load
Call TheHdw.Patterns(aPreconditionPattern).Start
Call TheHdw.Digital.TimeDomains(TheHdw.Patterns(aPreconditionPattern).TimeDomains).Patgen.HaltWait
Call TheHdw.Digital.Pins(aForcePinList).Disconnect
Call TheHdw.PPMU.Pins(aForcePinList).Connect

' Body
Call TheHdw.PPMU.Pins(aForcePinList).ForceI(aForceValue, aForceValue)
TheHdw.PPMU.Pins(aForcePinList).ClampVHi = aClampValue
TheHdw.PPMU.Pins(aForcePinList).Gate = tlOn
Call TheHdw.SetSettlingTimer(aWaitTime)
lMeas = TheHdw.PPMU.Pins(aForcePinList).Read(tlPPMUReadMeasurements, aSampleSize, tlPPMUReadingFormatAverage)
TheHdw.PPMU.Pins(aForcePinList).Gate = tlOff

' Post-Body
Call TheHdw.PPMU.Pins(aForcePinList).Disconnect
Call TheHdw.Digital.Pins(aForcePinList).Disconnect
Call TheExec.Flow.TestLimit(lMeas, ForceResults:=tlForceFlow)

End Function
