Attribute VB_Name = "DSP_Module"
Option Explicit

' This module should be used only for DSP Procedure code.  Functions in this
' module will be available to be called to perform DSP in all DSP modes.
' Additional modules may be added as needed (all starting with "DSP_").
'
' The required signature for a DSP Procedure is:
'
' Public Function FuncName(<arglist>) as Long
'   where <arglist> is any list of arguments supported by DSP code.
'
' See online help for supported types and other restrictions.


Public Function SineProcess(ByVal Sine As DSPWave, ByRef Amp As Double, ByRef Freq As Double, ByRef THD As Double, ByRef SNR As Double) As Long

    Dim Spectrum As New DSPWave

    Spectrum = Sine.Spectrum
    Amp = Spectrum.CalcAmplitudeFromSpectrum
    Freq = Spectrum.CalcFrequencyFromSpectrum
    THD = Spectrum.CalcTHD
    SNR = Spectrum.CalcSNR

End Function

