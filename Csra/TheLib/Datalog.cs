using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib {
    public class Datalog : ILib.IDatalog {

        const string _behaviorFeatureOfflinePassResults = "Datalog.Parametric.OfflinePassResults";

        //TODO: implement non-stalling datalog once IG-XL fix available (https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/538)

        public void TestFunctional(Site<bool> result, string pattern = "") => TheExec.Flow.FunctionalTestLimit(result, pattern);

        public void TestParametric(Site<int> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                int midWayPass = (int)MidWayBetweenLimits();
                result = new(midWayPass, result.PinName);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public void TestParametric(Site<double> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                double midWayPass = MidWayBetweenLimits();
                result = new(midWayPass, result.PinName);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public void TestParametric(PinSite<int> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                int midWayPass = (int)MidWayBetweenLimits();
                result = result.Select2d(r => r = midWayPass);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public void TestParametric(PinSite<double> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                double midWayPass = MidWayBetweenLimits();
                result = result.Select2d(r => r = midWayPass);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public void TestParametric(Site<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            SiteSamplesSub(result, forceValue, forceUnit, sameLimitForAllSamples);
        }

        public void TestParametric(Site<Samples<double>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            SiteSamplesSub(result, forceValue, forceUnit, sameLimitForAllSamples);
        }

        public void TestParametric(PinSite<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            PinSiteSamplesSub(result, forceValue, forceUnit, sameLimitForAllSamples);
        }

        public void TestParametric(PinSite<Samples<double>> result, double forceValue = 0, string forceUnit = "",
            bool sameLimitForAllSamples = false) {
            PinSiteSamplesSub(result, forceValue, forceUnit, sameLimitForAllSamples);
        }

        public void TestScanNetwork(ScanNetworkPatternResults result, ScanNetworkDatalogOption datalogOptions) {
            throw new NotImplementedException();
        }
        
        private bool OfflinePassResults => TheExec.TesterMode == tlLangTestModeType.Offline &&
            Api.Services.Behavior.GetFeature<bool>(_behaviorFeatureOfflinePassResults);

        private double MidWayBetweenLimits() {
            TheExec.Flow.GetTestLimits(out IFlowLimitsInfo limits);
            limits.GetLowLimits(out string[] lLim);
            limits.GetHighLimits(out string[] hLim);
            int curLimitIndex = TheExec.Flow.TestLimitIndex;
            double loLim = Convert.ToDouble(lLim[curLimitIndex]);
            double hilim = Convert.ToDouble(hLim[curLimitIndex]);
            return (loLim + hilim) / 2;
        }

        private void SiteSamplesSub<T>(Site<Samples<T>> result, double forceValue, string forceUnit, bool sameLimitForAllSamples) {
            var counts = new Site<int>();

            ForEachSite(site => counts[site] = result[site].Count); //result.Select(s => s.Count) won't work - .Select() not supported for Site<Samples<T>> yet
            if (!counts.IsUniform(out var count)) Api.Services.Alert.Error<InvalidOperationException>($"PinListData error: Varying sample counts per site are not " +
                $"supported: '{counts}'.");
            for (int i = 0; i < count; i++) {
                Site<T> sample = result.GetSample(i);
                if (sameLimitForAllSamples && i > 0) TheExec.Flow.TestLimitIndex--;
                if (OfflinePassResults) {
                    T midWayPass = (T)Convert.ChangeType(MidWayBetweenLimits(), typeof(T));
                    sample = new(midWayPass, result.PinName);
                }
                TheExec.Flow.TestLimit(ResultVal: sample, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                    ForceResults: tlLimitForceResults.Flow);
            }
        }

        private void PinSiteSamplesSub<T>(PinSite<Samples<T>> result, double forceValue, string forceUnit, bool sameLimitForAllSamples) {
            for (int i = 0; i < result.Count; i++) {
                if (sameLimitForAllSamples && i > 0) TheExec.Flow.TestLimitIndex--;
                SiteSamplesSub(result[i], forceValue, forceUnit, sameLimitForAllSamples);
            }
        }
    }
}
