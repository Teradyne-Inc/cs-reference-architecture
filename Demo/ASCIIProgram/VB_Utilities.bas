Attribute VB_Name = "VB_Utilities"
Option Explicit

Public Function SimulateCaptureSine(Ampl As Double, Freq As Double, Size As Integer, Fund_bin As Integer) As DSPWave
    
    Dim Sim As New DSPWave
    Dim Harmonic_2 As New DSPWave
    Dim Harmonic_3 As New DSPWave
    Dim Noise As New DSPWave
    Dim Ampl_Sd As New SiteDouble
    
    ' Create normalized sine (amplitude = 1.0)
    Call Sim.CreateSin(2 * Pi * Fund_bin / Size, 0#, Size)
    Sim.SampleRate = Freq * Size / Fund_bin
    
    ' Add 90dB of distortion
    Call Harmonic_2.CreateSin(2 * Pi * Fund_bin * 2 / Size, 0#, Size)
    Call Harmonic_3.CreateSin(2 * Pi * Fund_bin * 3 / Size, 0#, Size)
    Sim = Sim.Add(Harmonic_2.Multiply(Sqr(0.000000001 / 4# * 2.9))).Add(Harmonic_3.Multiply(Sqr(0.000000001 / 4#)))
    
    Dim Site As Variant
    
    For Each Site In TheExec.Sites
        'Add site-specific noise
        Call Noise.CreateRandom(-0.00001, 0.00001, Size)
        Sim(Site) = Sim.Add(Noise)
        ' site-specific gain
        Ampl_Sd(Site) = Ampl + Site * 0.1
        Sim(Site) = Sim.Multiply(Ampl_Sd)
    Next
    
    Set SimulateCaptureSine = Sim
End Function

Public Function CreateCaptureSine(PinList As PinList, Ampl As Double, Freq As Double, Size As Integer, Fund_bin As Integer) As PinListData
    
    Dim pld As New PinListData
    Dim PinData As PinData
    
    Dim PinName As Variant
    
    For Each PinName In PinList
        PinData.Name = PinName
        PinData = SimulateCaptureSine(Ampl, Freq, Size, Fund_bin)
        pld.AddPin (PinData)
        Ampl = Ampl + 0.1
    Next

    Set CreateCaptureSine = pld

End Function


