using System.Collections.Generic;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib.Acquire {

    public class Dc : ILib.IAcquire.IDc {

        public virtual PinSite<double> Measure(Pins pins, Measure? meterMode = null) {
            return Measure(pins, 1, null, meterMode);
        }

        public virtual PinSite<double> Measure(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null) {
            List<PinSite<double>> resultPin = new();
            if (meterMode.HasValue) {
                Api.TheLib.Setup.Dc.SetMeter(pins, meterMode.Value);
            }
            if (pins.Ppmu is not null) resultPin.Add(pins.Ppmu.Measure(sampleSize));
            if (pins.Dcvi is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvi.Measure(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvi.Measure(sampleSize));
            }
            if (pins.Dcvs is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvs.Measure(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvs.Measure(sampleSize));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual PinSite<double> Measure(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null) {
            bool sampleSizesUniform = sampleSizes.Length == 1;
            bool sampleRatesUniform = (sampleRates?.Length ?? 1) == 1;
            bool meterModesUniform = (meterModes?.Length ?? 1) == 1;
            bool allUniform = sampleSizesUniform && sampleRatesUniform && meterModesUniform;
            if (allUniform) {
                return Measure(Pins.Join(pinGroups), sampleSizes[0], sampleRates?[0], meterModes?[0]);
            }
            PinSite<double> resultMeasure = new();
            for (int i = 0; i < pinGroups.Length; i++) {
                resultMeasure.AddRange(Measure(pinGroups[i], sampleSizes.SingleOrAt(i), sampleRates?.SingleOrAt(i), meterModes?.SingleOrAt(i)));
            }
            return resultMeasure;
        }

        public virtual PinSite<Samples<double>> MeasureSamples(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null) {
            List<PinSite<Samples<double>>> resultPin = new();
            if (meterMode.HasValue) {
                Api.TheLib.Setup.Dc.SetMeter(pins, meterMode.Value);
            }
            if (pins.Ppmu is not null) resultPin.Add(pins.Ppmu.MeasureSamples(sampleSize));
            if (pins.Dcvi is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvi.MeasureSamples(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvi.MeasureSamples(sampleSize));
            }
            if (pins.Dcvs is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvs.MeasureSamples(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvs.MeasureSamples(sampleSize));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual PinSite<Samples<double>> MeasureSamples(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null) {
            List<PinSite<Samples<double>>> resultMeasure = new();
            bool sampleSizesUniform = sampleSizes.Length == 1;
            bool sampleRatesUniform = (sampleRates?.Length ?? 1) == 1;
            bool meterModesUniform = (meterModes?.Length ?? 1) == 1;
            bool allUniform = sampleSizesUniform && sampleRatesUniform && meterModesUniform;
            if (allUniform) {
                resultMeasure.Add(MeasureSamples(Pins.Join(pinGroups), sampleSizes[0], sampleRates?[0], meterModes?[0]));
            } else {
                for (int i = 0; i < pinGroups.Length; i++) {
                    resultMeasure.Add(MeasureSamples(pinGroups[i], sampleSizes.SingleOrAt(i), sampleRates?.SingleOrAt(i), meterModes?.SingleOrAt(i)));
                }
            }
            return Pins.Join(pinGroups).ArrangePinSite(resultMeasure);
        }

        private PinSite<Samples<double>> ToPinSiteSamplesDouble(IPinListData pinListData) {
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

        public virtual PinSite<double> ReadCaptured(Pins pins, string signalName) {
            List<PinSite<double>> resultPin = new();
            if (pins.Dcvi != null) {
                resultPin.Add(pins.Dcvi.HardwareApi.Capture.Signals[signalName].DspWave.ToPinSite<double>());
            }
            if (pins.Dcvs != null) {
                resultPin.Add(pins.Dcvs.HardwareApi.Capture.Signals[signalName].DspWave.ToPinSite<double>());
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual PinSite<Samples<double>> ReadCapturedSamples(Pins pins, string signalName) {
            List<PinSite<Samples<double>>> resultPin = new();
            if (pins.Dcvi != null) {
                resultPin.Add(ToPinSiteSamplesDouble(pins.Dcvi.HardwareApi.Capture.Signals[signalName].DspWave));
            }
            if (pins.Dcvs != null) {
                resultPin.Add(ToPinSiteSamplesDouble(pins.Dcvs.HardwareApi.Capture.Signals[signalName].DspWave));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual PinSite<double> ReadMeasured(Pins pins, int sampleSize, double? sampleRate = null) {
            List<PinSite<double>> resultPin = new();
            if (pins.Ppmu is not null) resultPin.Add(pins.Ppmu.Read());
            if (pins.Dcvi is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvi.Read(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvi.Read(sampleSize));
            }
            if (pins.Dcvs is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvs.Read(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvs.Read(sampleSize));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual PinSite<Samples<double>> ReadMeasuredSamples(Pins pins, int sampleSize, double? sampleRate = null) {
            List<PinSite<Samples<double>>> resultPin = new();
            if (pins.Ppmu is not null) resultPin.Add(pins.Ppmu.MeasureSamples(sampleSize));
            if (pins.Dcvi is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvi.ReadSamples(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvi.ReadSamples(sampleSize));
            }
            if (pins.Dcvs is not null) {
                if (sampleRate.HasValue) resultPin.Add(pins.Dcvs.ReadSamples(sampleSize, sampleRate.Value));
                else resultPin.Add(pins.Dcvs.ReadSamples(sampleSize));
            }
            return pins.ArrangePinSite(resultPin);
        }

        public virtual void Strobe(Pins pins) {
            pins.Ppmu?.Strobe(); // assuming current range is already set
            pins.Dcvi?.Strobe(); // assuming the mode is already set 
            pins.Dcvs?.Strobe();// assuming that the voltage and the range is already set
        }

        public virtual void StrobeSamples(Pins pins, int sampleSize, double? sampleRate = null) {
            //PPMU can only strobe for 1 sample, user has to use the.Read instead to strobe for more than 1 sample
            pins.Ppmu?.Strobe(); // assuming current range is already set
            pins.Dcvi?.Strobe(sampleSize, sampleRate ?? -1); // assuming the mode is already set
            pins.Dcvs?.Strobe(sampleSize, sampleRate ?? -1); // assuming that the voltage and the range is already set
        }
    }
}
