using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    internal static class Extension {

        public static PinSite<Samples<double>> ToPinSiteSamplesDouble(this IPinListData pinListData) {
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
    }
}
