using System;
using System.Collections.Generic;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using Csra.Interfaces;

namespace Csra.TheLib.Acquire {
    public class Dc : ILib.IAcquire.IDc {

        public PinSite<double> Measure(Pins pins, Measure? meterMode = null) {
            List<PinSite<double>> resultPin = new();
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                resultPin.Add(TheHdw.PPMU.Pins(ppmuPins).Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>());
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                if (meterMode.HasValue) MeterModeDcvi(dcviPins, meterMode.Value);
                resultPin.Add(TheHdw.DCVI.Pins(dcviPins).Meter.Read(tlStrobeOption.Strobe, 1, Format: tlDCVIMeterReadingFormat.Average).ToPinSite<double>());
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                if (meterMode.HasValue) MeterModeDcvs(dcvsPins, meterMode.Value);
                resultPin.Add(TheHdw.DCVS.Pins(dcvsPins).Meter.Read(tlStrobeOption.Strobe, 1, Format: tlDCVSMeterReadingFormat.Average).ToPinSite<double>());
            }
            return pins.ArrangePinSite(resultPin);
        }

        public PinSite<double> Measure(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null)
            => MeterSub(pins, sampleSize, true, sampleRate, meterMode);

        public PinSite<double> Measure(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null) {
            PinSite<double> resultMeasure = new();
            bool sampleSizesUniform = sampleSizes.Length == 1;
            bool sampleRatesUniform = (sampleRates?.Length ?? 1) == 1;
            bool meterModesUniform = (meterModes?.Length ?? 1) == 1;
            bool allUniform = sampleSizesUniform && sampleRatesUniform && meterModesUniform;
            if (allUniform) {
                resultMeasure = MeterSub(Pins.Join(pinGroups), sampleSizes[0], true, sampleRates?[0], meterModes?[0]);
            } else {
                for (int i = 0; i < pinGroups.Length; i++) {
                    resultMeasure.AddRange(MeterSub(pinGroups[i], sampleSizes.SingleOrAt(i), true, sampleRates?.SingleOrAt(i), meterModes?.SingleOrAt(i)));
                }
            }
            return resultMeasure;
        }

        public PinSite<Samples<double>> MeasureSamples(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null)
            => MeterSamplesSub(pins, sampleSize, true, sampleRate, meterMode);

        public PinSite<Samples<double>> MeasureSamples(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null) {
            List<PinSite<Samples<double>>> resultMeasure = new();
            bool sampleSizesUniform = sampleSizes.Length == 1;
            bool sampleRatesUniform = (sampleRates?.Length ?? 1) == 1;
            bool meterModesUniform = (meterModes?.Length ?? 1) == 1;
            bool allUniform = sampleSizesUniform && sampleRatesUniform && meterModesUniform;
            if (allUniform) {
                resultMeasure.Add(MeterSamplesSub(Pins.Join(pinGroups), sampleSizes[0], true, sampleRates?[0], meterModes?[0]));
            } else {
                for (int i = 0; i < pinGroups.Length; i++) {
                    resultMeasure.Add(MeterSamplesSub(pinGroups[i], sampleSizes.SingleOrAt(i), true, sampleRates?.SingleOrAt(i), meterModes?.SingleOrAt(i)));
                }
            }
            return ArrangePinSiteSampleDouble(Pins.Join(pinGroups), resultMeasure);
        }

        private static PinSite<double> MeterSub(Pins pins, int sampleSize, bool strobe, double? sampleRate = null, Measure? meterMode = null) {
            List<PinSite<double>> resultPin = new();
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                resultPin.Add(ReadMeterPpmuAverage(ppmuPins, sampleSize, strobe));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                resultPin.Add(ReadMeterDcviAverage(dcviPins, sampleSize, strobe, sampleRate, meterMode));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                resultPin.Add(ReadMeterDcvsAverage(dcvsPins, sampleSize, strobe, sampleRate, meterMode));
            }
            return pins.ArrangePinSite(resultPin);
        }

        private static PinSite<Samples<double>> MeterSamplesSub(Pins pins, int sampleSize, bool strobe, double? sampleRate = null, Measure? meterMode = null) {
            List<PinSite<Samples<double>>> resultPin = new();
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                resultPin.Add(ReadMeterPpmuArray(ppmuPins, sampleSize));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                resultPin.Add(ReadMeterDcviArray(dcviPins, sampleSize, strobe, sampleRate, meterMode));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                resultPin.Add(ReadMeterDcvsArray(dcvsPins, sampleSize, strobe, sampleRate, meterMode));
            }

            return ArrangePinSiteSampleDouble(pins, resultPin);
        }

        private static PinSite<double> ReadMeterPpmuAverage(string pins, int sampleSize, bool strobe) {
            IPinListData ppmu;
            if (strobe) {
                ppmu = TheHdw.PPMU.Pins(pins).Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Average);
            } else {
                ppmu = TheHdw.PPMU.Pins(pins).ReadNoStrobe(tlPPMUReadWhat.Measurements);
            }
            return ppmu.ToPinSite<double>();
        }

        private static PinSite<Samples<double>> ReadMeterPpmuArray(string pins, int sampleSize) =>
            ToPinSiteSamplesDouble(TheHdw.PPMU.Pins(pins).Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Array));

        private static PinSite<double> ReadMeterDcviAverage(string pins, int sampleSize, bool strobe, double? sampleRate = null, Measure? meterMode = null) {
            tlStrobeOption strobeOption = strobe ? tlStrobeOption.Strobe : tlStrobeOption.NoStrobe;
            if (meterMode.HasValue) MeterModeDcvi(pins, meterMode.Value);
            return TheHdw.DCVI.Pins(pins).Meter.Read(strobeOption, sampleSize, sampleRate ?? -1, tlDCVIMeterReadingFormat.Average).ToPinSite<double>();
        }

        private static PinSite<Samples<double>> ReadMeterDcviArray(string pins, int sampleSize, bool strobe, double? sampleRate = null,
            Measure? meterMode = null) {
            tlStrobeOption strobeOption = strobe ? tlStrobeOption.Strobe : tlStrobeOption.NoStrobe;
            if (meterMode.HasValue) MeterModeDcvi(pins, meterMode.Value);
            return ToPinSiteSamplesDouble(TheHdw.DCVI.Pins(pins).Meter.Read(strobeOption, sampleSize, sampleRate ?? -1, tlDCVIMeterReadingFormat.Array));
        }

        private static PinSite<double> ReadMeterDcvsAverage(string pins, int sampleSize, bool strobe, double? sampleRate = null, Measure? meterMode = null) {
            tlStrobeOption strobeOption = strobe ? tlStrobeOption.Strobe : tlStrobeOption.NoStrobe;
            if (meterMode.HasValue) MeterModeDcvs(pins, meterMode.Value);
            return TheHdw.DCVS.Pins(pins).Meter.Read(strobeOption, sampleSize, sampleRate ?? -1, tlDCVSMeterReadingFormat.Average).ToPinSite<double>();
        }

        private static PinSite<Samples<double>> ReadMeterDcvsArray(string pins, int sampleSize, bool strobe, double? sampleRate = null,
            Measure? meterMode = null) {
            tlStrobeOption strobeOption = strobe ? tlStrobeOption.Strobe : tlStrobeOption.NoStrobe;
            if (meterMode.HasValue) MeterModeDcvs(pins, meterMode.Value);
            return ToPinSiteSamplesDouble(TheHdw.DCVS.Pins(pins).Meter.Read(strobeOption, sampleSize, sampleRate ?? -1, tlDCVSMeterReadingFormat.Array));
        }

        private static void MeterModeDcvi(string pins, Measure meterMode) {
            var dcvi = TheHdw.DCVI.Pins(pins).Meter;
            if (meterMode == Csra.Measure.Voltage) dcvi.Mode = tlDCVIMeterMode.Voltage;
            else dcvi.Mode = tlDCVIMeterMode.Current;
        }

        private static void MeterModeDcvs(string pins, Measure meterMode) {
            var dcvs = TheHdw.DCVS.Pins(pins);
            if (meterMode == Csra.Measure.Voltage) dcvs.Meter.Mode = tlDCVSMeterMode.Voltage;
            else dcvs.Meter.Mode = tlDCVSMeterMode.Current;
        }

        private static PinSite<Samples<double>> ToPinSiteSamplesDouble(IPinListData pinListData) {
            var resultPin = new PinSite<Samples<double>>(pinListData.Pins.Count);
            for (int i = 0; i < pinListData.Pins.Count; i++) {
                var siteSamples = new Site<Samples<double>>();
                ForEachSite(site => {
                    double[] values = (double[])pinListData.Pins[i].get_Value(site);
                    siteSamples[site] = new Samples<double>(values);
                });
                resultPin[i].PinName = pinListData.Pins[i].Name;
                resultPin[i] = siteSamples;
            }
            return resultPin;
        }

        private static PinSite<Samples<double>> ArrangePinSiteSampleDouble(Pins pins, List<PinSite<Samples<double>>> reference) {
            int nrPins = pins.Count();
            var reordered = new PinSite<Samples<double>>(nrPins);

            var flatPinSite = reference.SelectMany(pinSite => Enumerable.Range(0, pinSite.Count)
            .Select(num => new { samples = pinSite[num], pinName = pinSite[num].PinName }))
            .ToDictionary(pinInfo => pinInfo.pinName, pinInfo => pinInfo.samples);

            pins.Select((pin, index) => new { indexPin = index, samples = flatPinSite[pin.Name], pinName = pin.Name }).ToList()
                .ForEach(element => { reordered[element.indexPin].PinName = element.pinName; reordered[element.pinName] = element.samples; });

            return reordered;
        }

        public PinSite<double> ReadCaptured(Pins pins, string signalName) {
            List<PinSite<double>> resultPin = new();
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                resultPin.Add(ReadCapturedDCVI(dcviPins, signalName));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                resultPin.Add(ReadCapturedDCVS(dcvsPins, signalName));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public PinSite<Samples<double>> ReadCapturedSamples(Pins pins, string signalName) {
            List<PinSite<Samples<double>>> resultPin = new();
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                resultPin.Add(ReadCapturedDCVISamples(dcviPins, signalName));
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                resultPin.Add(ReadCapturedDCVSSamples(dcvsPins, signalName));
            }
            return ArrangePinSiteSampleDouble(pins, resultPin);
        }

        private static PinSite<double> ReadCapturedDCVI(string pins, string signalName) =>
             TheHdw.DCVI.Pins(pins).Capture.Signals[signalName].DspWave.ToPinSite<double>();

        private static PinSite<double> ReadCapturedDCVS(string pins, string signalName) =>
            TheHdw.DCVS.Pins(pins).Capture.Signals[signalName].DspWave.ToPinSite<double>();

        private static PinSite<Samples<double>> ReadCapturedDCVISamples(string pins, string signalName) =>
            ToPinSiteSamplesDouble(TheHdw.DCVI.Pins(pins).Capture.Signals[signalName].DspWave);

        private static PinSite<Samples<double>> ReadCapturedDCVSSamples(string pins, string signalName) =>
             ToPinSiteSamplesDouble(TheHdw.DCVS.Pins(pins).Capture.Signals[signalName].DspWave);
        public PinSite<double> ReadMeasured(Pins pins, int sampleSize, double? sampleRate = null) => MeterSub(pins, sampleSize, false, sampleRate);

        public PinSite<Samples<double>> ReadMeasuredSamples(Pins pins, int sampleSize, double? sampleRate = null) => MeterSamplesSub(pins, sampleSize,
            false, sampleRate);
        public void Strobe(Pins pins) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) { // assuming current range is already set
                TheHdw.PPMU.Pins(ppmuPins).Strobe();
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) { // assuming the mode is already set 
                TheHdw.DCVI.Pins(dcviPins).Meter.Strobe();
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) { // assuming that the voltage and the range is already set
                TheHdw.DCVS.Pins(dcvsPins).Meter.Strobe();
            }
        }

        public void StrobeSamples(Pins pins, int sampleSize, double? sampleRate = null) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) { // assuming current range is already set
                if (sampleSize > 1) { } // ignore, PPMU can only strobe for 1 sample , user has to use the .Read instead to strobe for more than 1 sample
                if (sampleRate.HasValue) { } // ignore, PPMU can only strobe for 1 sample , user has to use the .Read instead to strobe for more than 1 sample
                TheHdw.PPMU.Pins(ppmuPins).Strobe();
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) { // assuming the mode is already set
                TheHdw.DCVI.Pins(dcviPins).Meter.Strobe(sampleSize, sampleRate ?? -1);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) { // assuming that the voltage and the range is already set
                TheHdw.DCVS.Pins(dcvsPins).Meter.Strobe(sampleSize, sampleRate ?? -1);
            }
        }
    }
}
