Attribute VB_Name = "VBT_SineTest"
Option Explicit

Public Function TestSine() As Long

    Dim Sine As New DSPWave
    Dim Amp As New SiteDouble
    Dim Freq As New SiteDouble
    Dim THD As New SiteDouble
    Dim SNR As New SiteDouble

    Sine = SimulateCaptureSine(2#, 1000#, 1024, 5)
    
    ' Perform the following steps to process the "capture" data:
    ' 1) Add a new DSP Project to this solution, called "TI245_DSP" (or any name)
    ' * Click-right on Solution, select Add->New Project...
    ' * Choose an "IGXL DSP Project" in Visual Basic from the many templates available
    ' (You can filter the list to "Visual Basic" and "IG-XL" for a much shorter list.)
    ' * Give the project a useful name.  The name should probably include "DSP", but that is not required.
    ' 2) Inside your new DSP Project:
    ' * Rename the DSP Class to DspInDotNet and the empty DSP Method to SineProcess
    ' (Do not change the "<DspClass>" and "<DspMethod>" attributes. Those are keywords.  Rename the items they refer to.)
    ' If you rename the Class, you may also rename the file holding it -- that is best practice.
    ' 3) Refactor your empty DSP Method signature to accept 1 DSPWave input (your sine wave) and return 4 ByRef results
    ' ... SineProcess(ByVal sine As DSPWave, ByRef amp As Double, ByRef freq As Double, ByRef thd As Double, ByRef snr As Double)
    ' 4) Process the sine wave using DSP
    ' Dim spectrum As IDspWave
    ' spectrum = sine.Spectrum()
    ' amp = spectrum.CalcAmplitudeFromSpectrum()
    ' freq = spectrum.CalcFrequencyFromSpectrum()
    ' thd = spectrum.CalcTHD()
    ' snr = spectrum.CalcSNR()
    ' 5) Add a reference from _this_ project to your new DSP Project
    ' * In Solution Explorer, [Project Name]->Add->Reference...
    ' * Expand "Projects" on the left and add a checkmark next to your new DSP Project
    ' 6) Finally, call your new DSP Method below (replace the error statment with a RunDsp call),
    ' passing in the local "sine" DSPWave and the 4 result variables
    ' If your Method doesn't show up, perform a Build->Build solution.
            
    Call RunDsp.SineProcess(Sine, Amp, Freq, THD, SNR)

    Call TheExec.Flow.TestLimit(Amp, TName:="Amp", Unit:=unitVolt)
    Call TheExec.Flow.TestLimit(Freq, TName:="Freq", Unit:=unitHz)
    Call TheExec.Flow.TestLimit(THD, TName:="THD", Unit:=unitDb)
    Call TheExec.Flow.TestLimit(SNR, TName:="SNR", Unit:=unitDb)
    

End Function

Public Function TestSinePLD(PinList As PinList) As Long
            
    Dim Sine As New PinListData
    Dim Amp As New PinListData
    Dim Freq As New PinListData
    Dim THD As New PinListData
    Dim SNR As New PinListData

    Sine = CreateCaptureSine(PinList, 2#, 1000#, 1024, 5)

    Call RunDsp.SineProcess(Sine, Amp, Freq, THD, SNR)

    Call TheExec.Flow.TestLimit(Amp, TName:="Amp", Unit:=unitVolt)
    Call TheExec.Flow.TestLimit(Freq, TName:="Freq", Unit:=unitHz)
    Call TheExec.Flow.TestLimit(THD, TName:="THD", Unit:=unitDb)
    Call TheExec.Flow.TestLimit(SNR, TName:="SNR", Unit:=unitDb)
    
End Function
