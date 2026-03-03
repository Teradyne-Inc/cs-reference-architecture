using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Functional {

    [TestClass(Creation.TestInstance), Serializable]
    public class Ramp : TestCodeBase {
        PatternInfo _patternInfo;
        Site<bool> _patResult;
        tlPPMUSourceMode _mode;
        tlPPMUPatternControlReadFormat _readFormat;
        CaptType _captureType;
        TrigType _trigType;
        Pins _rampPin;
        Pins _capturePin;

        #region Execute ramp
        /// <summary>
        /// Executes a ramp on the specific pin while executing a pattern in-sync with the ramp. Intended for UP5000.
        /// </summary>
        /// <param name="rampPin">Digital pin for ramp to be configured on.</param>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="signalName">The signal name for the ramp.</param>
        /// <param name="startingValue">The starting value for the ramp.</param>
        /// <param name="incrementValue">The value at which the ramp will increment.</param>
        /// <param name="incrementPeriod">The period at which the ramp will increment.</param>
        /// <param name="incrementCount">The number of times the ramp will increment.</param>
        /// <param name="startDelay">The delay to begin incrementing from the start of the ramp.</param>
        /// <param name="captureLimit">The capture limit for the HRAM.</param>
        /// <param name="captureType">The capture type for the HRAM.</param>
        /// <param name="triggerType">The Trigger type for the HRAM.</param>
        /// <param name="waitForEvent">Whether to wait for an event when capturing the HRAM.</param>
        /// <param name="preTriggerCycleCount">The pre-trigger cycle count for the HRAM.</param>
        /// <param name="timeout">The time limit for PPMU sourcing before timeout.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        [TestMethod, Steppable, CustomValidation]
        public void ExecuteRamp(PinList rampPin, Pattern pattern, string signalName, double startingValue, double incrementValue,
            double incrementPeriod, int incrementCount, double startDelay, int captureLimit, string captureType, string triggerType, bool waitForEvent,
            int preTriggerCycleCount, double timeout, string setup = "") {
            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(rampPin, "", out _rampPin);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _patternInfo);
                TheLib.Validate.Enum(captureType.ToLower(), "", out _captureType);
                TheLib.Validate.Enum(triggerType.ToLower(), "", out _trigType);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                TheLib.Setup.Digital.Disconnect(_rampPin);
                TheLib.Setup.Dc.Connect(_rampPin);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Digital.ConfigureRamp(_rampPin, signalName, startingValue, incrementValue, incrementPeriod, startDelay, incrementCount);
                TheLib.Setup.Digital.ReadHram(captureLimit, _captureType, _trigType, waitForEvent, preTriggerCycleCount);
                TheLib.Execute.Digital.RunPatternSyncRamp(_rampPin, _patternInfo, signalName, timeout);
                _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestFunctional(_patResult, pattern);
            }
        }
        #endregion

        #region Execute Ramp with synchronized pattern & control PPMU through pattern.
        /// <summary>
        /// Execute a ramp on the specific pin while executing a pattern in-sync with the ramp and use UP5000-specific functionality to control the PPMU on the capture pin through the pattern.
        /// </summary>
        /// <param name="rampPin">The digital pin to configure the ramp on.</param>
        /// <param name="capturePin">The digital pin to capture data on through pattern control using the PPMU.</param>
        /// <param name="pattern">The pattern to execute.</param>
        /// <param name="signalName">The name of the signal for the ramp.</param>
        /// <param name="startingValue">The starting value of the ramp.</param>
        /// <param name="incrementValue">The value by which the ramp increments.</param>
        /// <param name="incrementPeriod">The period between each increment of the ramp.</param>
        /// <param name="incrementCount">The amount of times the ramp increments during execution.</param>
        /// <param name="startDelay">The delay between the start of the ramp and the first increment.</param>
        /// <param name="numSamplesPerStrobe">The number of samples per strobe.</param>
        /// <param name="readFormat">The reading format of the data.</param>
        /// <param name="timeout">The time to wait for sourcing on the PPMU before timeout.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        [TestMethod, Steppable, CustomValidation]
        public void ExecuteRampPatternControl(PinList rampPin, PinList capturePin, Pattern pattern, string signalName, double startingValue, double incrementValue,
            double incrementPeriod, int incrementCount, double startDelay, int numSamplesPerStrobe, string readFormat, double timeout, string setup = "") {
            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(rampPin, "", out _rampPin);
                TheLib.Validate.Pins(capturePin, "", out _capturePin);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _patternInfo);
                TheLib.Validate.Enum(readFormat.ToLower(), "", out _readFormat);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                TheLib.Setup.Digital.Disconnect(_rampPin);
                TheLib.Setup.Digital.Disconnect(_capturePin);
                TheLib.Setup.Dc.Connect(_rampPin);
                TheLib.Setup.Dc.Connect(_capturePin);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Digital.ConfigureRamp(_rampPin, signalName, startingValue, incrementValue, incrementPeriod, startDelay, incrementCount);
                TheLib.Setup.Digital.ConfigurePatternControl(_capturePin, numSamplesPerStrobe, _readFormat);
                TheLib.Execute.Digital.RunPatternSyncRamp(_rampPin, _patternInfo, signalName, timeout);
                _patResult = TheLib.Acquire.Digital.PatternResults();
                TheLib.Acquire.Dc.ReadMeasured(_capturePin, 1);
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestFunctional(_patResult, pattern);
            }
        }
        #endregion 
    }
}
