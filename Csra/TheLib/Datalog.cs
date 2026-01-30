using System;
using System.Linq;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib {
    public class Datalog : ILib.IDatalog {

        const string _behaviorFeatureOfflinePassResults = "Datalog.Parametric.OfflinePassResults";

        //TODO: implement non-stalling datalog once IG-XL fix available (https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/538)

        public virtual void TestFunctional(Site<bool> result, string pattern = "") => TheExec.Flow.FunctionalTestLimit(result, pattern);

        public virtual void TestParametric(Site<int> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                int midWayPass = (int)MidWayBetweenLimits();
                result = new(midWayPass, result.PinName);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(Site<double> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                double midWayPass = MidWayBetweenLimits();
                result = new(midWayPass, result.PinName);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(PinSite<int> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                int midWayPass = (int)MidWayBetweenLimits();
                result = result.Select2d(r => r = midWayPass);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(PinSite<double> result, double forceValue = 0, string forceUnit = "") {
            if (OfflinePassResults) {
                double midWayPass = MidWayBetweenLimits();
                result = result.Select2d(r => r = midWayPass);
            }
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
                ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(Site<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            if (OfflinePassResults) {
                result.Fill(new Samples<int>(GetSampleCount(result), (int)MidWayBetweenLimits()), tlSiteType.Existing);
            }
            TheExec.Flow.TestLimit(new PinSite<Samples<int>>("", result), CompareMode: tlLimitCompareType.EachSample, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
            ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(Site<Samples<double>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            if (OfflinePassResults) {
                result.Fill(new Samples<double>(GetSampleCount(result), MidWayBetweenLimits()), tlSiteType.Existing);
            }
            TheExec.Flow.TestLimit(new PinSite<Samples<double>>("", result), CompareMode: tlLimitCompareType.EachSample, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
            ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(PinSite<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false) {
            if (OfflinePassResults) {
                result.Fill(new Samples<int>(GetSampleCount(result), (int)MidWayBetweenLimits()), tlSiteType.Existing);
            }
            TheExec.Flow.TestLimit(result, CompareMode: tlLimitCompareType.EachSample, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
            ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestParametric(PinSite<Samples<double>> result, double forceValue = 0, string forceUnit = "",
            bool sameLimitForAllSamples = false) {
            if (OfflinePassResults) {
                result.Fill(new Samples<double>(GetSampleCount(result), MidWayBetweenLimits()), tlSiteType.Existing);
            }
            TheExec.Flow.TestLimit(result, CompareMode: tlLimitCompareType.EachSample, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: forceUnit,
            ForceResults: tlLimitForceResults.Flow);
        }

        public virtual void TestScanNetwork(ScanNetworkPatternResults result, ScanNetworkDatalogOption datalogOptions) {
            if (datalogOptions.HasFlag(ScanNetworkDatalogOption.LogByIclInstance)) {
                foreach (var instance in result.IclInstance) {
                    TheExec.Flow.TestLimit(ResultVal: instance.Value.IsFailed, 0, 0, TName: "fail flag");
                    TheExec.Flow.TestLimit(ResultVal: instance.Value.IsResultValid, -1, -1, TName: "tested flag");
                    TheExec.Datalog.WriteComment($"ssh-icl-instance: \t{instance.Key}\n" +
                        $"On-Chip Compare = {(instance.Value.IsOnChipCompare ? "On" : "Off")}\n" +
                        $"core instance: \t{instance.Value.CoreInstanceName}\n" +
                        new string('=', 120) + "\n");
                }
            } else if (datalogOptions.HasFlag(ScanNetworkDatalogOption.LogByCoreInstance)) {
                foreach (var instance in result.CoreInstance) {
                    TheExec.Flow.TestLimit(ResultVal: instance.Value.IsFailed, 0, 0, TName: "fail flag");
                    TheExec.Flow.TestLimit(ResultVal: instance.Value.IsResultValid, -1, -1, TName: "tested flag");
                    TheExec.Datalog.WriteComment($"core instance: \t{instance.Key}\n" +
                        "list of ssh instances:\n\t" + string.Join("\n\t", instance.Value) +
                        "\n" + new string('=', 120) + "\n");
                }
            }
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

        private int GetSampleCount<T>(Site<Samples<T>> result) {
            var counts = new Site<int>();
            ForEachSite(site => counts[site] = result[site].Count); //result.Select(s => s.Count) won't work - .Select() not supported for Site<Samples<T>> yet
            if (!counts.IsUniform(out int count)) Api.Services.Alert.Error<InvalidOperationException>($"PinListData error: Varying sample counts per site are not " +
                $"supported: '{counts}'.");
            return count;
        }

        private int GetSampleCount<T>(PinSite<Samples<T>> result) {
            var counts = new Site<int>();
            ForEachSite(site => counts[site] = result[0][site].Count); //result.Select(s => s.Count) won't work - .Select() not supported for PinSite<Samples<T>> yet
            if (!counts.IsUniform(out int count)) Api.Services.Alert.Error<InvalidOperationException>($"PinListData error: Varying sample counts per site are not " +
                $"supported: '{counts}'.");
            return count;
        }
    }
}
