using Teradyne.Igxl.Interfaces.Public;

namespace Csra {

    public class DigitalPinsParameters {
        public tlHSDMAlarm? AlarmType { get; set; } = null;
        public tlAlarmBehavior? AlarmBehavior { get; set; } = null;
        public bool? DisableCompare { get; set; } = null;
        public bool? DisableDrive { get; set; } = null;
        public ChInitState? InitState { get; set; } = null;
        public ChStartState? StartState { get; set; } = null;
        public bool? CalibrationExcluded { get; set; } = null;
        public bool? CalibrationHighAccuracy { get; set; } = null;
    }

    public class DigitalPinsLevelsParameters {
        public ChDiffPinLevel? DifferentialLevelsType { get; set; } = null;
        public double? DifferentialLevelsValue { get; set; } = null;
        public TLibDiffLvlValType[] DifferentialLevelsValuesType { get; set; } = null;
        public double[] DifferentialLevelsValues { get; set; } = null;
        public tlDriverMode? LevelsDriverMode { get; set; } = null;
        public ChPinLevel? LevelsType { get; set; } = null;
        public double? LevelsValue { get; set; } = null;
        public SiteDouble LevelsValuePerSite { get; set; } = null;
        public PinListData LevelsValues { get; set; } = null;
    }

    public class DigitalPinsTimingParameters {
        public double? TimingClockOffset { get; set; } = null;
        public double? TimingClockPeriod { get; set; } = null;
        public bool? TimingDisableAllEdges { get; set; } = null;
        public string TimingEdgeSet { get; set; } = null;
        public chEdge? TimingEdgeVal { get; set; } = null;
        public bool? TimingEdgeEnabled { get; set; } = null;
        public double? TimingEdgeTime { get; set; } = null;
        public double? TimingRefOffset { get; set; } = null;
        public string TimingSetup1xDiagnosticCapture { get; set; } = null;
        public double? TimingSrcSyncDataDelay { get; set; } = null;
        public tlOffsetType? TimingOffsetType { get; set; } = null;
        public double? TimingOffsetValue { get; set; } = null;
        public bool? TimingOffsetEnabled { get; set; } = null;
        public SiteLong TimingOffsetSelectedPerSite { get; set; } = null;
        public int? TimingOffsetValuePerSiteIndex { get; set; } = null;
        public SiteDouble TimingOffsetValuePerSiteValue { get; set; } = null;
        public AutoStrobeEnableSel? AutoStrobeEnabled { get; set; } = null;
        public int? AutoStrobeNumSteps { get; set; } = null;
        public int? AutoStrobeSamplesPerStep { get; set; } = null;
        public double? AutoStrobeStartTime { get; set; } = null;
        public double? AutoStrobeStepTime { get; set; } = null;
        public bool? FreeRunningClockEnabled { get; set; } = null;
        public double? FreeRunningClockFrequency { get; set; } = null;
        public FreqCtrEnableSel? FreqCtrEnable { get; set; } = null;
        public FreqCtrEventSlopeSel? FreqCtrEventSlope { get; set; } = null;
        public FreqCtrEventSrcSel? FreqCtrEventSource { get; set; } = null;
        public double? FreqCtrInterval { get; set; } = null;

    }
}
