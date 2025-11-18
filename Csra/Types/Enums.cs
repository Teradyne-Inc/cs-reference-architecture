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
    public enum InitMode { Creation = 0, OnProgramStarted = 1 }

    /// <summary>
    /// Physical instrument types.
    /// </summary>
    public enum InstrumentType { NC, UP2200, UPHP, UVI264, UVS64, UVS256, SupportBoard }

    /// <summary>
    /// Logical features provided through the instrument driver.
    /// </summary>
    public enum InstrumentFeature { Ppmu, Dcvi, Dcvs, Digital, Utility }

    /// <summary>
    /// Functional domain available by the instrument.
    /// </summary>
    public enum InstrumentDomain { Dc, Digital, Utility }

    /// <summary>
    /// Transaction type to use with the TransactionService.
    /// </summary>
    public enum TransactionType { PortBridge, CsraGeneric }

    /// <summary>
    /// Specifies the options for logging ScanNetwork test results from object <see cref="ScanNetworkPatternResults"/>.
    /// </summary>
    /// <remarks>This enumeration provides different logging strategies that can be used to capture ScanNetwork test results.
    /// Each option represents a preference of logging, and can be combined to allow for flexibility in how data
    /// is recorded and analyzed.</remarks>
    [Flags]
    public enum ScanNetworkDatalogOption {
        LogPatGenWithFTR = 1,
        LogByCoreInstance = 2,
        LogByIclInstance = 4,
        LogFailedInstancesOnly = 8,
        LogVerboseInfoByDTR = 16
    }

    public enum Kelvin {
        Force = 1,
        Sense = 2,
    }

    public enum Measure {
        Voltage,
        Current,
    }

    public enum TLibOutputMode {
        ForceVoltage,
        ForceCurrent,
        HighImpedance,
    }

    public enum TLibDiffLvlValType {
        VID,
        dVID0,
        dVID1,
        VICM,
        dVICM0,
        dVICM1,
        VOD,
        dVOD0,
        dVOD1,
        VCL,
        VCH,
        VT,
        IOL,
        IOH,
        VodTyp,
        VocmTyp
    }
}
