using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Csra.TheLib;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib.Setup {
    public class Digital : ILib.ISetup.IDigital {
        public void Connect(Pins pins) {
            if (pins.ContainsFeature(InstrumentFeature.Digital, out string pinList)) {
                TheHdw.Digital.Pins(pinList).Connect();
            }
            else {
                Api.Services.Alert.Warning("None of the pins contain 'Digital' features - no action performed");
            }
        }

        public void Disconnect(Pins pins) {
            if (pins.ContainsFeature(InstrumentFeature.Digital, out string pinList)) {
                TheHdw.Digital.Pins(pinList).Disconnect();
            } else {
                Api.Services.Alert.Warning("None of the pins contain 'Digital' features - no action performed");
            }
        }

        public void FrequencyCounter(Pins pins, double measureWindow, FreqCtrEventSrcSel eventSource, FreqCtrEventSlopeSel eventSlope) {
            if (pins.ContainsFeature(InstrumentFeature.Digital, out string pinList)) {
                var freqCtr = TheHdw.Digital.Pins(pinList).FreqCtr;
                freqCtr.Clear();
                freqCtr.EventSource = eventSource;
                freqCtr.EventSlope = eventSlope;
                freqCtr.Interval = measureWindow;
            } else {
                Api.Services.Alert.Error("Digital pins are required.");
            }
        }
        
        public void ModifyPins(Pins pins, DigitalPinsParameters parameters) => ModifyPins(pins,
            parameters.AlarmType, parameters.AlarmBehavior, parameters.DisableCompare, parameters.DisableDrive, parameters.InitState,
            parameters.StartState, parameters.CalibrationExcluded, parameters.CalibrationHighAccuracy);

        public void ModifyPins(Pins pins, tlHSDMAlarm? alarmType = null, tlAlarmBehavior? alarmBehavior = null,
            bool? disableCompare = null, bool? disableDrive = null, ChInitState? initState = null, ChStartState? startState = null,
            bool? calibrationExcluded = null, bool? calibrationHighAccuracy = null) {

            if (pins.ContainsFeature(InstrumentFeature.Digital, out string digitalPins)) {
                var digital = TheHdw.Digital.Pins(digitalPins);
                if (alarmType.HasValue && alarmBehavior.HasValue) {
                    digital.Alarms[alarmType.Value] = alarmBehavior.Value;
                } else if (alarmType.HasValue && !alarmBehavior.HasValue) {
                    Api.Services.Alert.Error("Cannot configure alarm on Digital Pins without setting value for alarmBehavior.");
                } else if (!alarmType.HasValue && alarmBehavior.HasValue) {
                    Api.Services.Alert.Error("Cannot configure alarm on Digital Pins without setting value for alarmType.");
                }

                if (disableCompare.HasValue) digital.DisableCompare = disableCompare.Value;
                if (disableDrive.HasValue) digital.DisableDrive = disableDrive.Value;
                if (initState.HasValue) digital.InitState = initState.Value;
                if (startState.HasValue) digital.StartState = startState.Value;
                if (calibrationExcluded.HasValue) digital.Calibration.Excluded = calibrationExcluded.Value;
                if (calibrationHighAccuracy.HasValue) digital.Calibration.HighAccuracy = calibrationHighAccuracy.Value;
            }
        }
        
        public void ModifyPinsLevels(Pins pins, DigitalPinsLevelsParameters parameters) => ModifyPinsLevels(pins,
            parameters.DifferentialLevelsType, parameters.DifferentialLevelsValue, parameters.DifferentialLevelsValuesType, parameters.DifferentialLevelsValues,
            parameters.LevelsDriverMode, parameters.LevelsType, parameters.LevelsValue, parameters.LevelsValuePerSite, parameters.LevelsValues);

        public void ModifyPinsLevels(Pins pins, ChDiffPinLevel? differentialLevelsType = null, double? differentialLevelsValue = null,
            TLibDiffLvlValType[] differentialLevelsValuesType = null, double[] differentialLevelsValues = null, tlDriverMode? levelsDriverMode = null,
            ChPinLevel? levelsType = null, double? levelsValue = null, SiteDouble levelsValuePerSite = null, PinListData levelsValues = null) {

            if (pins.ContainsFeature(InstrumentFeature.Digital, out string digitalPins)) {
                var digital = TheHdw.Digital.Pins(digitalPins);
                if (differentialLevelsType.HasValue && differentialLevelsValue.HasValue) {
                    digital.DifferentialLevels.Value[differentialLevelsType.Value] = differentialLevelsValue.Value;
                } else if (differentialLevelsType.HasValue && !differentialLevelsValue.HasValue) {
                    Api.Services.Alert.Error("Cannot configure DifferentialLevelsValue on Digital Pins without setting value for differentialLevelsValue.");
                } else if (!differentialLevelsType.HasValue && differentialLevelsValue.HasValue) {
                    Api.Services.Alert.Error("Cannot configure DifferentialLevelsValue on Digital Pins without setting value for differentialLevelsType.");
                }

                if (differentialLevelsValuesType is not null && differentialLevelsValues is not null) {
                    if (differentialLevelsValuesType.Length != differentialLevelsValues.Length) {
                        Api.Services.Alert.Error("Cannot configure DifferentialLevelsValues on Digital Pins when types array length does not equal to value array " +
                            "length.");
                    }
                    var valuesDict = Enum.GetValues(typeof(TLibDiffLvlValType))
                        .Cast<TLibDiffLvlValType>()
                        .ToDictionary(p => p, p => -5.0);

                    for (int i = 0; i < differentialLevelsValuesType.Length; i++) {
                        valuesDict[differentialLevelsValuesType[i]] = differentialLevelsValues[i];
                    }

                    digital.DifferentialLevels.Values(VID: valuesDict[TLibDiffLvlValType.VID],
                        dVID0: valuesDict[TLibDiffLvlValType.dVID0],
                        dVID1: valuesDict[TLibDiffLvlValType.dVID1],
                        VICM: valuesDict[TLibDiffLvlValType.VICM],
                        dVICM0: valuesDict[TLibDiffLvlValType.dVICM0],
                        dVICM1: valuesDict[TLibDiffLvlValType.dVICM1],
                        VOD: valuesDict[TLibDiffLvlValType.VOD],
                        dVOD0: valuesDict[TLibDiffLvlValType.dVOD0],
                        dVOD1: valuesDict[TLibDiffLvlValType.dVOD1],
                        VCL: valuesDict[TLibDiffLvlValType.VCL],
                        VCH: valuesDict[TLibDiffLvlValType.VCH],
                        VT: valuesDict[TLibDiffLvlValType.VT],
                        IOL: valuesDict[TLibDiffLvlValType.IOL],
                        IOH: valuesDict[TLibDiffLvlValType.IOH],
                        VodTyp: valuesDict[TLibDiffLvlValType.VodTyp],
                        VocmTyp: valuesDict[TLibDiffLvlValType.VocmTyp]
                        );
                } else if (differentialLevelsValuesType is not null && differentialLevelsValues is null) {
                    Api.Services.Alert.Error("Cannot configure DifferentialLevelsValues on Digital Pins without setting value for differentialLevelsValues.");
                } else if (differentialLevelsValuesType is null && differentialLevelsValues is not null) {
                    Api.Services.Alert.Error("Cannot configure DifferentialLevelsValues on Digital Pins without setting value for differentialLevelsValuesType.");
                }

                if (levelsDriverMode.HasValue) digital.Levels.DriverMode = levelsDriverMode.Value;
                if (levelsType.HasValue && levelsValue.HasValue) {
                    digital.Levels.Value[levelsType.Value] = levelsValue.Value;
                } else if (!levelsType.HasValue && levelsValue.HasValue) {
                    Api.Services.Alert.Error("Cannot configure LevelsValue on Digital Pins without setting value for levelsType.");
                }

                if (levelsType.HasValue && levelsValuePerSite is not null) {
                    digital.Levels.ValuePerSite[levelsType.Value] = levelsValuePerSite;
                } else if (!levelsType.HasValue && levelsValuePerSite is not null) {
                    Api.Services.Alert.Error("Cannot configure LevelsValuePerSite on Digital Pins without setting value for levelsType.");
                }

                if (levelsType.HasValue && levelsValues is not null) {
                    digital.Levels.Values[levelsType.Value] = levelsValues;
                } else if (!levelsType.HasValue && levelsValues is not null) {
                    Api.Services.Alert.Error("Cannot configure LevelsValues on Digital Pins without setting value for levelsType.");
                }

                if (levelsType.HasValue && !levelsValue.HasValue && levelsValuePerSite is null && levelsValues is null) {
                    Api.Services.Alert.Error("Cannot configure LevelsValue/LevelsValuePerSite/LevelsValues on Digital Pins only with levelsType.");
                }
            }
        }
        
        public void ModifyPinsTiming(Pins pins, DigitalPinsTimingParameters parameters) => ModifyPinsTiming(pins,
            parameters.TimingClockOffset, parameters.TimingClockPeriod, parameters.TimingDisableAllEdges, parameters.TimingEdgeSet, parameters.TimingEdgeVal,
            parameters.TimingEdgeEnabled, parameters.TimingEdgeTime, parameters.TimingRefOffset, parameters.TimingSetup1xDiagnosticCapture,
            parameters.TimingSrcSyncDataDelay, parameters.TimingOffsetType, parameters.TimingOffsetValue, parameters.TimingOffsetEnabled,
            parameters.TimingOffsetSelectedPerSite, parameters.TimingOffsetValuePerSiteIndex, parameters.TimingOffsetValuePerSiteValue,
            parameters.AutoStrobeEnabled, parameters.AutoStrobeNumSteps, parameters.AutoStrobeSamplesPerStep, parameters.AutoStrobeStartTime,
            parameters.AutoStrobeStepTime, parameters.FreeRunningClockEnabled, parameters.FreeRunningClockFrequency, parameters.FreqCtrEnable,
            parameters.FreqCtrEventSlope, parameters.FreqCtrEventSource, parameters.FreqCtrInterval);

        public void ModifyPinsTiming(Pins pins, double? timingClockOffset = null, double? timingClockPeriod = null,
            bool? timingDisableAllEdges = null, string timingEdgeSet = null, chEdge? timingEdgeVal = null, bool? timingEdgeEnabled = null,
            double? timingEdgeTime = null, double? timingRefOffset = null, string timingSetup1xDiagnosticCapture = null,
            double? timingSrcSyncDataDelay = null, tlOffsetType? timingOffsetType = null, double? timingOffsetValue = null, bool? timingOffsetEnabled = null,
            SiteLong timingOffsetSelectedPerSite = null, int? timingOffsetValuePerSiteIndex = null, SiteDouble timingOffsetValuePerSiteValue = null,
            AutoStrobeEnableSel? autoStrobeEnabled = null, int? autoStrobeNumSteps = null, int? autoStrobeSamplesPerStep = null,
            double? autoStrobeStartTime = null, double? autoStrobeStepTime = null, bool? freeRunningClockEnabled = null,
            double? freeRunningClockFrequency = null, FreqCtrEnableSel? freqCtrEnable = null, FreqCtrEventSlopeSel? freqCtrEventSlope = null,
            FreqCtrEventSrcSel? freqCtrEventSource = null, double? freqCtrInterval = null) {

            if (pins.ContainsFeature(InstrumentFeature.Digital, out string digitalPins)) {
                var digital = TheHdw.Digital.Pins(digitalPins);
                if (timingClockOffset.HasValue) digital.Timing.ClockOffset = timingClockOffset.Value;
                if (timingClockPeriod.HasValue) digital.Timing.ClockPeriod.Value = timingClockPeriod.Value;
                if (timingDisableAllEdges.HasValue) digital.Timing.DisableAllEdges(timingDisableAllEdges.Value);

                if (timingEdgeSet is not null && timingEdgeVal.HasValue && timingEdgeEnabled.HasValue) {
                    digital.Timing.EdgeEnabled[timingEdgeSet, timingEdgeVal.Value] = timingEdgeEnabled.Value;
                } else if (timingEdgeSet is null && timingEdgeVal.HasValue && timingEdgeEnabled.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeEnabled on Digital Pins without setting value for timingEdgeSet.");
                } else if (timingEdgeSet is not null && !timingEdgeVal.HasValue && timingEdgeEnabled.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeEnabled on Digital Pins without setting value for timingEdgeVal.");
                } else if (timingEdgeSet is null && !timingEdgeVal.HasValue && timingEdgeEnabled.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeEnabled on Digital Pins without setting value for timingEdgeSet and timingEdgeVal.");
                }

                if (timingEdgeSet is not null && timingEdgeVal.HasValue && timingEdgeTime.HasValue) {
                    digital.Timing.EdgeTime(timingEdgeSet, timingEdgeVal.Value).Value = timingEdgeTime.Value;
                } else if (timingEdgeSet is null && timingEdgeVal.HasValue && timingEdgeTime.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeTime on Digital Pins without setting value for timingEdgeSet.");
                } else if (timingEdgeSet is not null && !timingEdgeVal.HasValue && timingEdgeTime.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeTime on Digital Pins without setting value for timingEdgeVal.");
                } else if (timingEdgeSet is null && !timingEdgeVal.HasValue && timingEdgeTime.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeTime on Digital Pins without setting value for timingEdgeSet and timingEdgeVal.");
                }

                if (!timingEdgeEnabled.HasValue && !timingEdgeTime.HasValue && (timingEdgeSet is not null || timingEdgeVal.HasValue)) {
                    Api.Services.Alert.Error("Cannot configure TimingEdgeEnabled or TimingEdgeTime on Digital Pins with ONLY setting value for timingEdgeSet and " +
                        "timingEdgeVal.");
                }

                if (timingRefOffset.HasValue) digital.Timing.RefOffset = timingRefOffset.Value;
                if (timingSetup1xDiagnosticCapture is not null) digital.Timing.Setup1xDiagnosticCapture(timingSetup1xDiagnosticCapture);
                if (timingSrcSyncDataDelay.HasValue) digital.Timing.SrcSyncDataDelay = timingSrcSyncDataDelay.Value;

                if (timingOffsetType.HasValue && timingOffsetValue.HasValue) {
                    digital.Timing.Offset(timingOffsetType.Value).Value = timingOffsetValue.Value;
                } else if (!timingOffsetType.HasValue && timingOffsetValue.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingOffset on Digital Pins without setting value for timingOffsetType.");
                }

                if (timingOffsetType.HasValue && timingOffsetEnabled.HasValue) {
                    digital.Timing.Offset(timingOffsetType.Value).Enabled = timingOffsetEnabled.Value;
                } else if (!timingOffsetType.HasValue && timingOffsetEnabled.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetEnabled on Digital Pins without setting value for timingOffsetType.");
                }

                if (timingOffsetType.HasValue && timingOffsetSelectedPerSite is not null) {
                    digital.Timing.Offset(timingOffsetType.Value).SelectedPerSite = timingOffsetSelectedPerSite;
                } else if (!timingOffsetType.HasValue && timingOffsetSelectedPerSite is not null) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetSelectedPerSite on Digital Pins without setting value for timingOffsetType.");
                }

                if (timingOffsetType.HasValue && timingOffsetValuePerSiteIndex.HasValue && timingOffsetValuePerSiteValue is not null) {
                    digital.Timing.Offset(timingOffsetType.Value).ValuePerSite[timingOffsetValuePerSiteIndex.Value] = timingOffsetValuePerSiteValue;
                } else if (!timingOffsetType.HasValue && timingOffsetValuePerSiteIndex.HasValue && timingOffsetValuePerSiteValue is not null) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetValuePerSite on Digital Pins without setting value for timingOffsetType.");
                } else if (timingOffsetType.HasValue && !timingOffsetValuePerSiteIndex.HasValue && timingOffsetValuePerSiteValue is not null) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetValuePerSite on Digital Pins without setting value for timingOffsetValuePerSiteIndex.");
                } else if (timingOffsetType.HasValue && timingOffsetValuePerSiteIndex.HasValue && timingOffsetValuePerSiteValue is null) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetValuePerSite on Digital Pins without setting value for timingOffsetValuePerSiteValue.");
                }

                if (!timingOffsetValue.HasValue && !timingOffsetEnabled.HasValue && timingOffsetSelectedPerSite is null &&
                    !timingOffsetValuePerSiteIndex.HasValue && timingOffsetValuePerSiteValue is null && timingOffsetType.HasValue) {
                    Api.Services.Alert.Error("Cannot configure TimingOffsetValue or TimingOffsetEnabled or TimingOffsetSelectedPerSite or " +
                        "TimingOffsetValuePerSiteValue on Digital Pins with ONLY setting value for timingOffsetType.");
                }
                // AutoStrobe
                if (autoStrobeEnabled.HasValue) digital.AutoStrobe.Enabled = autoStrobeEnabled.Value;
                if (autoStrobeNumSteps.HasValue) digital.AutoStrobe.NumSteps = autoStrobeNumSteps.Value;
                if (autoStrobeSamplesPerStep.HasValue) digital.AutoStrobe.SamplesPerStep = autoStrobeSamplesPerStep.Value;
                if (autoStrobeStartTime.HasValue) digital.AutoStrobe.StartTime = autoStrobeStartTime.Value;
                if (autoStrobeStepTime.HasValue) digital.AutoStrobe.StepTime = autoStrobeStepTime.Value;
                // FreeRunningClock
                if (freeRunningClockEnabled.HasValue) digital.FreeRunningClock.Enabled = freeRunningClockEnabled.Value;
                if (freeRunningClockFrequency.HasValue) digital.FreeRunningClock.Frequency = freeRunningClockFrequency.Value;
                // FreqCtr
                if (freqCtrEnable.HasValue) digital.FreqCtr.Enable = freqCtrEnable.Value;
                if (freqCtrEventSlope.HasValue) digital.FreqCtr.EventSlope = freqCtrEventSlope.Value;
                if (freqCtrEventSource.HasValue) digital.FreqCtr.EventSource = freqCtrEventSource.Value;
                if (freqCtrInterval.HasValue) digital.FreqCtr.Interval = freqCtrInterval.Value;
            }
        }

        public void ReadHram(int captureLimit, CaptType captureType, TrigType triggerType, bool waitForEvent, int preTriggerCycleCount) {
            TheHdw.Digital.HRAM.SetTrigger(triggerType, waitForEvent, preTriggerCycleCount, true);
            TheHdw.Digital.HRAM.CaptureType = captureType;
            TheHdw.Digital.HRAM.Size = captureLimit;
        }
        
        public void ReadAll() => ReadHram(TheHdw.Digital.HRAM.MaxDepth, CaptType.All, TrigType.First, false, 0);

        public void ReadFails() => ReadHram(TheHdw.Digital.HRAM.MaxDepth, CaptType.Fail, TrigType.First, false, 0);

        public void ReadStoredVectors() => ReadHram(TheHdw.Digital.HRAM.MaxDepth, CaptType.STV, TrigType.First, false, 0);
    }
}
