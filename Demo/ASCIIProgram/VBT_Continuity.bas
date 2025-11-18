Attribute VB_Name = "VBT_Continuity"
Option Explicit

 
Public Function Continuity(digital_pins As PinList, power_pin As PinList, _
                            Power_pin_voltage As Double, _
                            Power_pin_current As Double, _
                            Power_pin_current_range As Double, _
                            PPMU_current_value As Double, _
                            Optional TNames_ As String = "Continuity_VBT") As Long

'''' Dimension object as PinListData to contain PPMU measured results
     Dim PPMUMeasure As New PinListData

'''' Offline simulation variables
     Dim Site As Variant
     Dim PinNameArray() As String
     Dim NumPins As Long

''''Disconnect All_Dig Pin Electronics from pins in order to connect PPMU's''''
    TheHdw.Digital.Pins(digital_pins).Disconnect
    
'''' Setup VCC to 0V
    With TheHdw.DCVS.Pins(power_pin)
        .Gate = False
        .Disconnect tlDCVSConnectDefault
        .Mode = tlDCVSModeVoltage
        .Voltage.Output = tlDCVSVoltageMain
        .Voltage.Value = Power_pin_voltage
        .CurrentRange.Value = Power_pin_current_range
        .Connect tlDCVSConnectDefault
        .Gate = True
    End With

''''Program All_Dig PPMU Pins to force CurrentValue. Connect the PPMU's and Gate on'''''
    With TheHdw.PPMU.Pins(digital_pins)
        .ForceI PPMU_current_value
        .Connect
        .Gate = tlOn
    End With

'''Make Measurements on PPMU pins and store in pinlistdata''''
    PPMUMeasure = TheHdw.PPMU.Pins(digital_pins).Read(tlPPMUReadMeasurements)

''''Setup OFFLINE Simulation  ''''''''''''''''''''''''''''''''''''''''''''''''''''''
    If TheExec.TesterMode = testModeOffline Then
        TheExec.DataManager.DecomposePinList digital_pins, PinNameArray, NumPins
        For NumPins = 0 To NumPins - 1
            For Each Site In TheExec.Sites
                PPMUMeasure.Pins(NumPins).Value(Site) = -0.5 - (Rnd() / 23)
            Next Site
        Next NumPins
    End If

 'Disconnect PPMU from digital channels
       With TheHdw.PPMU.Pins(digital_pins)
        .ForceI 0
        .Gate = tlOff
        .Disconnect
    End With
 
'''''''''DATALOG RESULTS''''''''''''''''''''''''''''
    TheExec.Flow.TestLimit ResultVal:=PPMUMeasure, Unit:=unitVolt, ForceVal:=PPMU_current_value, _
    ForceUnit:=unitAmp, ForceResults:=tlForceFlow
       
End Function


