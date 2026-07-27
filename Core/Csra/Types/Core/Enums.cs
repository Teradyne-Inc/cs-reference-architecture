using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csra {

    /// <summary>
    /// The available output targets for Alert Service messages.
    /// </summary>
    [Flags]
    public enum AlertOutputTarget { OutputWindow = 1, Datalog = 2, File = 4 }

    /// <summary>
    /// Events in the lifetime of a Setup that will cause a reset of the hardware state.
    /// </summary>
    [Flags]
    public enum SetupResetTrigger { Creation = 0, OnProgramStarted = 1 }

    /// <summary>
    /// Physical instrument types.
    /// </summary>
    public enum InstrumentType {

        /// <summary>
        /// Not connected - no instrument resource assigned.
        /// </summary>
        NC,

        /// <summary>
        /// The UP2200 (internal: Paradise) digital instrument on UltraFLEX+.
        /// </summary>
        UP2200,

        /// <summary>
        /// The UP5000 (internal: Utopia) digital instrument on UltraFLEX+.
        /// </summary>
        UP5000,

        /// <summary>
        /// The UVI264 (internal: Raiden) DCVI instrument on UltraFLEX+.
        /// </summary>
        UVI264,

        /// <summary>
        /// The UVS64 (internal: Tesla) DCVS instrument on UltraFLEX+.
        /// </summary>
        UVS64,

        /// <summary>
        /// The UVS256 (internal: ?) DCVS instrument on UltraFLEX+.
        /// </summary>
        UVS256,

        /// <summary>
        /// The UVS64HP (internal: Zebra) DCVS instrument on UltraFLEX+.
        /// </summary>
        UVS64HP,

        /// <summary>
        /// The Support Board instrument on UltraFLEX+ (Paradise).
        /// </summary>
        Support,

        /// <summary>
        /// The UP1600 (internal: Utah) digital instrument on UltraFLEX.
        /// </summary>
        UP1600,

        /// <summary>
        /// The VSM (internal: ?) DCVI instrument on UltraFLEX.
        /// </summary>
        VSM,

        /// <summary>
        /// The HexVS (internal: ?) DCVS instrument on UltraFLEX.
        /// </summary>
        HexVS,

        /// <summary>
        /// The UVI80 (internal: ?) DCVI instrument on UltraFLEX.
        /// </summary>
        UVI80,

        /// <summary>
        /// The Support Board instrument on UltraFLEX (classic).
        /// </summary>
        SupportBoard
    }

    /// <summary>
    /// Logical features provided through the instrument driver.
    /// </summary>
    public enum InstrumentFeature { Ppmu, Dcvi, Dcvs, Digital, Utility }

    /// <summary>
    /// Functional domain available by the instrument.
    /// </summary>
    public enum InstrumentDomain { Dc, Digital, Utility }

    /// <summary>
    /// Specifies the options for logging ScanNetwork test results from object <see cref="ScanNetworkPatternResults"/>.
    /// </summary>
    /// <remarks>This enumeration provides different logging strategies that can be used to capture ScanNetwork test results.
    /// Each option represents a preference of logging, and can be combined to allow for flexibility in how data
    /// is recorded and analyzed.</remarks>
    [Flags]
    public enum ScanNetworkDatalogOption {
        PatGenWithFtr = 1,
        ByCoreInstance = 2,
        ByIclInstance = 4,
        FailedInstancesOnly = 8,
        VerboseInfoByDtr = 16
    }

    public enum KelvinMode {
        Force = 1,
        Sense = 2,
    }

    public enum DcMeterMode {
        Voltage,
        Current,
    }

    public enum DcOutputMode {
        ForceVoltage,
        ForceCurrent,
        HighImpedance,
    }

    public enum DiffLevelValueType {
        Vid,
        DVid0,
        DVid1,
        Vicm,
        DVicm0,
        DVicm1,
        Vod,
        DVod0,
        DVod1,
        Vcl,
        Vch,
        Vt,
        Iol,
        Ioh,
        VodTypical,
        VocmTypical
    }
}
