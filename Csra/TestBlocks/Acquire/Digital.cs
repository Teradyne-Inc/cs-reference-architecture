using System;
using System.Linq;
using Csra.Interfaces;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib.Acquire {
    public class Digital : ILib.IAcquire.IDigital {
        public virtual PinSite<double> MeasureFrequency(Pins pins) {
            if (pins.Digital != null) {
                DriverDigPinsFreqCtr freqCtr = pins.Digital.HardwareApi.FreqCtr;
                freqCtr.Start();
                return freqCtr.MeasureFrequency().ToPinSite<double>();
            }
            Api.Services.Alert.Error("Digital pins are required.");
            return null;
        }

        public virtual Site<bool> PatternResults() => TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();

        public virtual PinSite<Samples<int>> Read(Pins pins, int startIndex = 0, int cycle = 0) {
            if (pins.Digital != null) {
                int numCapturedCycles = TheHdw.Digital.HRAM.CapturedCycles;
                PinSite<string[]> pinData = pins.Digital.HardwareApi.HRAM.PinData(startIndex, cycle, numCapturedCycles).ToPinSite<string[]>();
                PinSite<Samples<int>> returnValue = new PinSite<Samples<int>>();
                foreach (Site<string[]> pin in pinData) {
                    Site<Samples<int>> siteValues = new Site<Samples<int>>();
                    siteValues.PinName = pin.PinName;
                    ForEachSite(site => {
                        string[] values = pin[site];
                        int[] pinValues = values.Select(ConvertDigitalValueToInt).ToArray();
                        siteValues[site] = new Samples<int>(pinValues);
                    });
                    returnValue.Add(siteValues);
                }
                return returnValue;

                // Local function to convert digital pin values to integers
                static int ConvertDigitalValueToInt(string digitalValue) => digitalValue switch { "H" => 1, "L" => 0, _ => -1 };
            }
            Api.Services.Alert.Error("Digital pins are required.");
            return null;
        }
        
        public virtual PinSite<Samples<int>> ReadWords(Pins pins, int startIndex, int length, int wordSize, tlBitOrder bitOrder) {
            string[] digPins = pins.ExtractByFeature(InstrumentFeature.Digital).Select(p => p.Name).ToArray();
            if (digPins.Length > 0) {
                PinSite<Samples<int>> returnValue = new PinSite<Samples<int>>();
                foreach (string pin in digPins) {
                    ISiteLong[] hramWords = (ISiteLong[])TheHdw.Digital.Pins(pin).HRAM.ReadDataWord(startIndex, length, wordSize, bitOrder);
                    Site<Samples<int>> siteWords = new Site<Samples<int>>();
                    siteWords.PinName = pin;
                    ForEachSite(site => {
                        int[] wordValues = hramWords.Select(word => (int)word[site]).ToArray();
                        siteWords[site] = new Samples<int>(wordValues);
                    });
                    returnValue.Add(siteWords);
                }
                return returnValue;
            }
            Api.Services.Alert.Error("Digital pins are required.");
            return null;
        }
    }
}
