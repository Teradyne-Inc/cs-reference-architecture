Attribute VB_Name = "Interpose"
Option Explicit


Public Function WriteInfo(argc As Long, argv() As String)
    Dim info As String

    With TheHdw.Computer
        info = .OperatingSystem
        info = info & "/" & .ProcessorType
        info = info & "/" & .ProcessorSpeed
    End With

   TheExec.Datalog.WriteComment info
End Function

Public Function StartTimer(argc As Long, argv() As String) As Long
   TheHdw.StartStopwatch
End Function


Public Function EndTimer(argc As Long, argv() As String) As Long
    Dim testLength As String
    
    testLength = "Time for test" & TheExec.DataManager.instanceName
    testLength = testLength & " is: " & TheHdw.ReadStopwatch
    
    TheExec.Datalog.WriteComment vbCrLf & testLength
End Function

Public Function CloseDibRelay(argc As Long, argv() As String) As Long
    Dim utilityPin As String
    
    utilityPin = "dib_relay"   'argv(0)
    
    TheHdw.DIB.Power("12V").State = tlOn
    TheHdw.Utility(utilityPin).State = tlUtilBitOn
End Function

Public Function OpenDibRelay(argc As Long, argv() As String) As Long
    Dim utilityPin As String
    
    utilityPin = "dib_relay"
    
    TheHdw.Utility(utilityPin).State = tlUtilBitOff
    TheHdw.DIB.Power("12V").State = tlOff
    
End Function

Public Function setupPatgen(argc As Long, argv() As String) As Long
    TheHdw.Digital.Patgen.HaltMode = 1
    'Call TheHdw.Patterns("ti245functionalnew.pat").Start("", "")
    'Debug.Print TheHdw.Digital.Patgen.IsRunning
    'TheHdw.Digital.Patgen.Halt
End Function
