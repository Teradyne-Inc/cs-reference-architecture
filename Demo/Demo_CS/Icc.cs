using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS {
    [TestClass]
    public class Icc : TestCodeBase {
        [TestMethod]
        public void Icc_static(PinList powerPin, double vccValue, double meterCurrentRange, PinList initLowPins, PinList initHighPins,
                                double currentLimit, string tNames_ = "Icc_static") {

            PinListData iccMeasure = new PinListData();

            // ''''Apply HSD levels, Init States, and PowerSupply pin values''''
            // ''''Connect all pins,load levels,do not load timing (not needed), do not hot-switch'''
            // ''''Set up initial state of pins as hi,low, and hi-z'''''''''
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered, initHighPins.Value, initLowPins.Value);

            //Set up VCC pin to measure current
            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Meter.Mode = tlDCVSMeterMode.Current;
            }

            //Program Current Range and Source Fold Limit
            TheHdw.DCVS.Pins(powerPin).CurrentRange.Value = meterCurrentRange;
            TheHdw.DCVS.Pins(powerPin).CurrentLimit.Source.FoldLimit.Level.Value = currentLimit;

            // Wait 5ms'''
            TheHdw.Wait(0.005);

            //Strobe the meter on the VCC pin and store it in an pinListdata variable defined
            iccMeasure.Value = TheHdw.DCVS.Pins(powerPin).Meter.Read(tlStrobeOption.Strobe, 10, 1000);

            Random rand = new Random();
            // '''Setup OFFLINE Simulation by stuffing the pinListdata variable with simulation data'''''''
            if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                ForEachSite(site => {
                    iccMeasure.Pins[0].set_Value(site, 0.028 + rand.NextDouble() / 99);
                });
            }

            // '''''''''DATALOG RESULTS''''''''''''''''''''''''''''''''''
            TheExec.Flow.TestLimit(ResultVal: iccMeasure, Unit: UnitType.Amp, TName: "Icc_static", PinName: powerPin, ForceVal: vccValue,
                ForceUnit: UnitType.Volt, ForceResults: tlLimitForceResults.Flow);
            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Gate = false;
                dcvs.Disconnect(tlDCVSConnectWhat.Default);
            }

            // PROFILE MARKER 
            //Icc_Static_Marker.ProcedureProfileMarkLeave();
        }


        [TestMethod]
        public void Icc_dynamic(Pattern patternFile, PinList powerPin, double vccValue, double currentRange, string tNames_ = "Icc_Icc_dynamic") {
            PinListData iccMeasure = new PinListData();

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            //Setup the Hardware settings
            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Voltage.Value = vccValue;
                dcvs.Meter.Mode = tlDCVSMeterMode.Current;
                dcvs.CurrentRange.Value = currentRange;
            }
            /// Wait 1ms'''
            TheHdw.Wait(0.001);

            // Start the pattern
            TheHdw.Patterns(patternFile).Start();

            // Wait for the pattern to be in loop condition
            TheHdw.Digital.Patgen.FlagWait((int)CpuFlag.A, 0);

            //Strobe the meter on the VCC pin and store it in an pinListdata variable defined
            TheHdw.DCVS.Pins(powerPin).Meter.Strobe();

            TheHdw.Digital.Patgen.Continue(0, (int)CpuFlag.A);
            TheHdw.Digital.Patgen.HaltWait();

            //Read the meter on the VCC pin and store it in an pinListdata variable defined
            iccMeasure.Value = TheHdw.DCVS.Pins(powerPin).Meter.Read(tlStrobeOption.Strobe);

            Random rand = new Random();
            // '''Setup OFFLINE Simulation by stuffing the pinListdata variable with simulation data'''''''
            if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                ForEachSite(site => {
                    iccMeasure.Pins[0].set_Value(site, 0.028 + rand.NextDouble() / 99);
                });
            }

            // '''''''''DATALOG RESULTS''''''''''''''''''''''''''''''''''
            TheExec.Flow.TestLimit(ResultVal: iccMeasure, Unit: UnitType.Amp, PinName: powerPin, ForceVal: vccValue, ForceUnit: UnitType.Volt,
                ForceResults: tlLimitForceResults.Flow);

            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Gate = false;
                dcvs.Disconnect(tlDCVSConnectWhat.Default);
            }
        }

        [TestMethod]
        public void Icc_flag_static(Pattern patternFile, PinList powerPin, double vccValue, double currentRange, int flagLoops,
            string tNames_ = "Icc_Icc_dynamic") {
            PinListData iccMeasure = new PinListData();

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            //Setup the Hardware settings
            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Voltage.Value = vccValue;
                dcvs.Meter.Mode = tlDCVSMeterMode.Current;
                dcvs.CurrentRange.Value = currentRange;
            }

            /// Wait 1ms'''
            TheHdw.Wait(0.001);

            // Start the pattern
            TheHdw.Patterns(patternFile).Start();

            for (int loop = 0; loop < flagLoops; loop++) {
                TheHdw.Digital.Patgen.FlagWait((int)CpuFlag.A, 0);
                //Strobe the meter on the VCC pin a
                TheHdw.DCVS.Pins(powerPin).Meter.Strobe();
                TheHdw.Digital.Patgen.Continue(0, (int)CpuFlag.A);
            }

            // Wait for the pattern to be in loop condition
            TheHdw.Digital.Patgen.HaltWait();

            //Read the meter on the VCC pin and store it in an pinListdata variable defined
            iccMeasure.Value = TheHdw.DCVS.Pins(powerPin).Meter.Read(tlStrobeOption.NoStrobe, 2, -1, tlDCVSMeterReadingFormat.Array);

            Random rand = new Random();
            //'''Setup OFFLINE Simulation by stuffing the pinListdata variable with simulation data'''''''
            //if (TheExec.TesterMode == tlLangTestModeType.Offline) {
            //    ForEachSite(site => {
            //        iccMeasure.Pins[0].set_Value(site, 0.028 + rand.NextDouble() / 99);
            //    });
            //}

            // extract data
            SiteDouble result1 = new();
            SiteDouble result2 = new();
            ForEachSite(site => {
                double[] s = (double[])iccMeasure.Pins[0].get_Value(site);
                result1[site] = s[0];
                result2[site] = s[1];
            });

            // '''''''''DATALOG RESULTS''''''''''''''''''''''''''''''''''
            TheExec.Flow.TestLimit(ResultVal: result1, Unit: UnitType.Amp, PinName: powerPin, ForceVal: vccValue, ForceUnit: UnitType.Volt,
                ForceResults: tlLimitForceResults.Flow);
            TheExec.Flow.TestLimit(ResultVal: result2, Unit: UnitType.Amp, PinName: powerPin, ForceVal: vccValue, ForceUnit: UnitType.Volt,
                ForceResults: tlLimitForceResults.Flow);

            {
                var dcvs = TheHdw.DCVS.Pins(powerPin);
                dcvs.Gate = false;
                dcvs.Disconnect(tlDCVSConnectWhat.Default);
            }
        }
    }
}
