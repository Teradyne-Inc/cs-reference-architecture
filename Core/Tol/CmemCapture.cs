using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {
    /// <summary>
    /// CMEM (Capture Memory) implementation of <see cref="IDigitalCapture"/> that records digital pattern execution data, either failing cycle indices or raw STV device data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Construct instances via <see cref="Factory"/> helpers:
    /// <list type="bullet">
    ///   <item><description><c>CreateCmemFailsCapture</c> — captures failing cycle indices (<see cref="CmemCaptType.Fail"/>).</description></item>
    ///   <item><description><c>CreateCmemStvCapture</c> — captures raw device data (<see cref="CmemCaptType.STV"/>).</description></item>
    ///   <item><description><c>CreateCmemCapture</c> — full control over all parameters.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Typical usage sequence:
    /// <list type="number">
    ///   <item><description>Create via a <see cref="Factory"/> helper (configures CMEM hardware immediately).</description></item>
    ///   <item><description>Execute a pattern burst.</description></item>
    ///   <item><description>Call <see cref="ReadFails"/> or <see cref="ReadStv"/> to retrieve captured data from hardware into <see cref="FailingCycleData"/> or <see cref="DeviceData"/>.</description></item>
    ///   <item><description>Call <see cref="Reset"/> to release hardware resources after processing.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class CmemCapture : IDigitalCapture {
        /// <inheritdoc/>
        public IDigitalPins Pins => _digitalPins;

        private CmemCaptType _captureType;
        private tlCMEMCaptureSource _captureSource;
        private IDigitalPins _digitalPins;
        private int _returnValues;
        private int _wordSize;
        private int _captureSize;
        private int _captureLimit;
        private bool _fullSpeed;
        private bool _vectorOffset;
        private string _pattern;
        private string _timeDomain;

        private DriverDigCMEM _cmem;
        private DriverDigitalDomain _hwdDigitalTimeDomain;

        /// <summary>
        /// Initializes a new <see cref="CmemCapture"/> and configures the CMEM hardware immediately.
        /// </summary>
        /// <param name="captureType">
        /// The capture mode. Must be <see cref="CmemCaptType.Fail"/> or <see cref="CmemCaptType.STV"/>;
        /// <see cref="CmemCaptType.None"/> and <see cref="CmemCaptType.All"/> are not allowed.
        /// </param>
        /// <param name="captureSource">
        /// The hardware data source. Must be compatible with <paramref name="captureType"/>:
        /// <list type="bullet">
        ///   <item><description><see cref="CmemCaptType.Fail"/>: <c>PassFailData</c> or <c>PatPassFailData</c>.</description></item>
        ///   <item><description><see cref="CmemCaptType.STV"/>: <c>DutData</c> or <c>PatDutData</c>.</description></item>
        /// </list>
        /// </param>
        /// <param name="digitalPins">Pins used to retrieve data during <see cref="Read{T}"/>. Required for reading captured data.</param>
        /// <param name="returnValues">Number of expected return values per word. Used together with <paramref name="wordSize"/> to compute the STV capture buffer size (<c>returnValues × wordSize</c>). Ignored for <see cref="CmemCaptType.Fail"/>.</param>
        /// <param name="wordSize">Width of each word in bits. Used together with <paramref name="returnValues"/> to compute the STV capture buffer size. Ignored for <see cref="CmemCaptType.Fail"/>.</param>
        /// <param name="captureSize">Maximum number of cycles to store in CMEM. Defaults to 4096. Applies to <see cref="CmemCaptType.Fail"/> only; STV size is derived from <paramref name="returnValues"/> and <paramref name="wordSize"/>.</param>
        /// <param name="captureLimit">Maximum number of captures before the hardware stops recording. Set to 0 (default) for unlimited captures.</param>
        /// <param name="fullSpeed">When <see langword="true"/>, captures at full pattern speed without inserting wait cycles.</param>
        /// <param name="vectorOffset">Reserved for future use; not currently applied to <c>SetCaptureConfig</c>.</param>
        /// <param name="pattern">Pattern name to associate with this capture. Pass an empty string (default) for any pattern.</param>
        /// <param name="timeDomain">Digital time domain name to configure. Pass an empty string (default) to use the default time domain.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="captureType"/> is <see cref="CmemCaptType.None"/> or <see cref="CmemCaptType.All"/>, or when <paramref name="captureSource"/> is incompatible with <paramref name="captureType"/>.
        /// </exception>
        public CmemCapture(CmemCaptType captureType, tlCMEMCaptureSource captureSource, IDigitalPins digitalPins = null,
                             int returnValues = 0, int wordSize = 0, int captureSize = 4096, int captureLimit = 0,
                             bool fullSpeed = false, bool vectorOffset = false,
                             string pattern = "", string timeDomain = "") {

            if (captureType == CmemCaptType.None || captureType == CmemCaptType.All) {
                throw new ArgumentException("Cmem Capture object does not allow for CaptureType to be either 'All' or 'None'", nameof(captureType));
            }

            if (captureType == CmemCaptType.Fail) {
                if (captureSource != tlCMEMCaptureSource.PassFailData && captureSource != tlCMEMCaptureSource.PatPassFailData) {
                    throw new ArgumentException("For Fail capture type, captureSource must be either PassFailData or PatPassFailData", nameof(captureSource));
                }
            }

            if (captureType == CmemCaptType.STV) {
                if (captureSource != tlCMEMCaptureSource.DutData && captureSource != tlCMEMCaptureSource.PatDutData) {
                    throw new ArgumentException("For STV capture type, captureSource must be either DutData or PatDutData", nameof(captureSource));
                }
            }

            _captureType = captureType;
            _captureSource = captureSource;
            _digitalPins = digitalPins;
            _returnValues = returnValues;
            _wordSize = wordSize;
            _captureSize = captureSize;
            _captureLimit = captureLimit;
            _fullSpeed = fullSpeed;
            _vectorOffset = vectorOffset;
            _pattern = pattern;
            _timeDomain = timeDomain;

            _hwdDigitalTimeDomain = TheHdw.Digital.TimeDomains(_timeDomain);
            _cmem = TheHdw.Digital.TimeDomains(_timeDomain).CMEM;

            this.Configure();
        }

        /// <summary>
        /// Clears the CMEM capture configuration and discards any previously captured data.
        /// Call this after processing captured data to return the hardware to an idle state.
        /// </summary>
        public void Reset() {
            _cmem.CentralFields = tlCMEMCaptureFields.All;
            _cmem.SetCaptureConfig(0, CmemCaptType.None);

            if (_captureLimit > 0) {
                _cmem.CaptureLimit = 0;
                _cmem.CaptureLimitMode = tlDigitalCMEMCaptureLimitMode.Disable;
            }
        }

        /// <summary>
        /// Reads captured data from the CMEM hardware and stores the result in either
        /// <see cref="FailingCycleData"/> (for <see cref="CmemCaptType.Fail"/>) or
        /// <see cref="DeviceData"/> (for <see cref="CmemCaptType.STV"/>).
        /// </summary>
        /// <typeparam name="T">
        /// The expected return type. Use <c>PinSite&lt;int[]&gt;</c> when configured for
        /// <see cref="CmemCaptType.Fail"/>, or <c>PinSite&lt;double[]&gt;</c> for
        /// <see cref="CmemCaptType.STV"/>.
        /// </typeparam>
        /// <returns>
        /// The captured data cast to <typeparamref name="T"/>:
        /// <list type="bullet">
        ///   <item><description><see cref="CmemCaptType.Fail"/>: <see cref="FailingCycleData"/> as <typeparamref name="T"/> — integer cycle offsets.</description></item>
        ///   <item><description><see cref="CmemCaptType.STV"/>: <see cref="DeviceData"/> as <typeparamref name="T"/> — double-precision cycle offsets.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the capture type is neither <see cref="CmemCaptType.Fail"/> nor
        /// <see cref="CmemCaptType.STV"/>, or when the hardware returns inconsistent
        /// <c>CycleData</c> and <c>CycleOffsets</c> array lengths for a site.
        /// </exception>
        private T Read<T>() where T: class {
            if (_digitalPins == null) {
                throw new InvalidOperationException("Digital pins must be specified to read captured data.");
            }

            if (_captureType != CmemCaptType.Fail && _captureType != CmemCaptType.STV) {
                throw new InvalidOperationException($"Read is not supported for capture type '{_captureType}'.");
            }

            ICmemModuleCycleData moduleCycleData = TheHdw.Digital.Pins(_digitalPins.Name).CMEM.ModuleCycleData(_captureType == CmemCaptType.Fail, false);

            PinSite<byte[]> cycleData = moduleCycleData.CycleData.ToPinSite<byte[]>();

            PinSite<double[]> result = new();
            foreach (Site<byte[]> pin in cycleData) {
                Site<double[]> siteResult = new();
                bool hasData = false;

                ForEachSite(site => {
                    if (pin[site] is null) return;
                    double[] siteOffsets = (double[])moduleCycleData.CycleOffsets[site];
                    if (pin[site].Length != siteOffsets.Length) {
                        throw new InvalidOperationException($"CycleData length ({pin[site].Length}) does not match CycleOffsets length ({siteOffsets.Length}) for site {site}.");
                    }

                    List<double> matchedCycles = new List<double>();
                    for (int index = 0; index < pin[site].Length; index++) {
                        if (pin[site][index] == 1) {
                            matchedCycles.Add(siteOffsets[index]);
                        }
                    }
                    double[] matchedCyclesArray = matchedCycles.ToArray();

                    if (matchedCyclesArray.Length == 0) return;
                    hasData = true;
                    siteResult[site] = matchedCyclesArray;
                });

                if (hasData) {
                    siteResult.PinName = pin.PinName;
                    result.Add(siteResult);
                }
            }

            switch (_captureType) {
                case CmemCaptType.Fail:
                    return ConvertToIntArray(result) as T;
                case CmemCaptType.STV:
                    return result as T;
                default:
                    return null;
            }
        }

        public PinSite<int[]> ReadFails() {
            if (_captureType != CmemCaptType.Fail) {
                throw new InvalidOperationException($"ReadFails is only supported for capture type '{CmemCaptType.Fail}', but current type is '{_captureType}'.");
            }

            return Read<PinSite<int[]>>();
        }

        public PinSite<double[]> ReadStv() {
            if (_captureType != CmemCaptType.STV) {
                throw new InvalidOperationException($"ReadStv is only supported for capture type '{CmemCaptType.STV}', but current type is '{_captureType}'.");
            }
            return Read<PinSite<double[]>>();
        }

        // Applies the capture configuration to CMEM hardware based on the capture type.
        // For Fail: restricts captured fields to cycles only to minimise memory usage.
        // For STV: sizes the capture buffer as returnValues × wordSize.
        private void Configure() {
            switch (_captureType) {
                case CmemCaptType.Fail:
                    _cmem.SetCaptureConfig(_captureSize, _captureType, _captureSource, _fullSpeed, false);
                    _cmem.CentralFields = tlCMEMCaptureFields.CyclesOnly;
                    break;
                case CmemCaptType.STV:
                    int stvCaptureCount = _returnValues * _wordSize;
                    _cmem.SetCaptureConfig(stvCaptureCount, _captureType, _captureSource, _fullSpeed, false);
                    break;
            }

            if (_captureLimit > 0) {
                _cmem.CaptureLimit = _captureLimit;
                _cmem.CaptureLimitMode = tlDigitalCMEMCaptureLimitMode.Enable;
            }
        }

        // Converts cycle offsets from double to int (via Math.Round) for storage in FailingCycleData.
        private PinSite<int[]> ConvertToIntArray(PinSite<double[]> source) {
            PinSite<int[]> converted = new();
            foreach (Site<double[]> pin in source) {
                Site<int[]> siteConverted = new();
                siteConverted.PinName = pin.PinName;
                
                ForEachSite(site => {
                    if (pin[site] != null) {
                        int[] intArray = new int[pin[site].Length];
                        for (int i = 0; i < pin[site].Length; i++) {
                            intArray[i] = (int)Math.Round(pin[site][i]);
                        }
                        siteConverted[site] = intArray;
                    }
                });
                
                converted.Add(siteConverted);
            }
            return converted;
        }
    }
}
