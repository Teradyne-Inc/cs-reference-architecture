using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {
    /// <summary>
    /// HRAM (Hardware RAM) implementation of <see cref="IDigitalCapture"/> that records digital pattern execution data, either failing cycle indices or raw STV device data.
    /// </summary>
    public class HramCapture : IDigitalCapture {
        /// <inheritdoc/>
        public IDigitalPins Pins => _digitalPins;

        private CaptType _captureType;
        private IDigitalPins _digitalPins;
        private TrigType _triggerType;
        private int _captureLimit;
        private string _timeDomain;

        private DriverDigHRAM _hram;
        private DriverDigitalDomain _hwdDigitalTimeDomain;

        /// <summary>
        /// Initializes a new <see cref="HramCapture"/> and configures the HRAM hardware immediately.
        /// </summary>
        /// <param name="captureType">The capture mode. Must be <see cref="CaptType.Fail"/> or <see cref="CaptType.STV"/> — all other <see cref="CaptType"/> values are rejected.</param>
        /// <param name="digitalPins">Pins used to retrieve data during <see cref="ReadFails"/>/<see cref="ReadStv"/>. Required for reading captured data.</param>
        /// <param name="captureLimit">
        /// Maximum number of cycles to store in HRAM.
        /// </param>
        /// <param name="timeDomain">Optional parameter. Digital time domain name to configure. Default value is an empty string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="digitalPins"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="captureType"/> is not <see cref="CaptType.Fail"/> or <see cref="CaptType.STV"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="captureLimit"/> is less than <c>-1</c></exception>
        /// <remarks>
        /// <para>
        /// Passing an empty string for <paramref name="timeDomain"/> explicitly targets the <b>System</b> time domain (the domain comprised of all pins defined in the test program) via <c>TheHdw.Digital.TimeDomains("")</c>.
        /// Always go through <c>TimeDomains(...)</c>, even with an empty string, to guarantee reads/writes are scoped to a single, well-defined domain.
        /// </para>
        /// <para>
        /// Value for <paramref name="captureLimit"/> must be <c>-1</c> (max depth), <c>0</c> (hardware default), or a positive value not exceeding the hardware's <c>MaxDepth</c> for the active PatGen mode.
        /// </para>
        /// <para>
        /// HRAM is a single hardware resource per time domain — <c>CaptureType</c>, <c>Size</c>, and the trigger arm/disarm state
        /// configured here are not scoped to this <see cref="HramCapture"/> instance. Constructing a second <see cref="HramCapture"/>
        /// against the same <paramref name="timeDomain"/> reconfigures the hardware out from under the first instance, and calling
        /// <see cref="Reset"/> on either instance disarms the trigger for both. Do not construct multiple concurrent
        /// <see cref="HramCapture"/> instances on the same time domain; finish reading one capture (<see cref="ReadFails"/>/<see cref="ReadStv"/>)
        /// and call <see cref="Reset"/> before constructing the next.
        /// </para>
        /// </remarks>
        public HramCapture(CaptType captureType, IDigitalPins digitalPins, int captureLimit, string timeDomain = "") {
            if (digitalPins is null) {
                throw new ArgumentNullException(nameof(digitalPins), "Digital pins must be specified to read captured data.");
            }

            if (captureType != CaptType.Fail && captureType != CaptType.STV) {
                throw new ArgumentException($"Hram Capture object does not allow for {nameof(captureType)} to be other than 'Fail' or 'STV'", nameof(captureType));
            }

            if (captureLimit < -1) {
                throw new ArgumentOutOfRangeException(nameof(captureLimit), captureLimit, $"{nameof(captureLimit)} must be -1 (max depth), 0 (hardware default), or a positive value.");
            }

            _captureType = captureType;
            _digitalPins = digitalPins;
            _captureLimit = captureLimit;
            _timeDomain = timeDomain;

            _hwdDigitalTimeDomain = TheHdw.Digital.TimeDomains(_timeDomain);
            _hram = _hwdDigitalTimeDomain.HRAM;

            try {
                this.Configure();
            }
            catch {
                // If SetTrigger fails, the HRAM hardware may be left in an inconsistent state. Reset to a known idle state before propagating the exception.
                // Using throw instead of throw ex to preserve the original stack trace so callers still see the source of the failure.
                this.Reset();
                throw;
            }
        }

        /// <summary>
        /// Clears the HRAM capture configuration. Call this after processing captured data to return the hardware to an idle state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>Size</c> is intentionally not reset — it is inert once <c>CaptureType</c> is <see cref="CaptType.None"/>, and every new capture object sets its own <c>Size</c> before use.
        /// </para>
        /// <para>
        /// HRAM's <c>CaptureType</c> and trigger arm state are shared per time domain, not scoped to this instance (see the
        /// constructor's remarks). Calling <see cref="Reset"/> disarms the trigger for <b>any</b> <see cref="HramCapture"/>
        /// instance currently configured against the same time domain, not just this one.
        /// </para>
        /// </remarks>
        public void Reset() {
            _hram.CaptureType = CaptType.None;
            _captureType = CaptType.None;

            // Trig=[Never]: Never start a new capture.
            // WaitForEvent=[false]: Not applicable with Trig=Never.
            // PreTrigCycleCnt=[0]: No pre-trigger cycles to retain, since disarming the trigger discards any pending capture window.
            // StopOnFull=[true]: Kept for backward compatibility; inert while Trig=Never.
            _hram.SetTrigger(TrigType.Never, false, 0, true);
        }

        /// <summary>
        /// Reads the cycle offsets of failing captures recorded in HRAM.
        /// </summary>
        /// <returns>
        /// Failing cycle offsets per pin/site. Pins/sites without failures are omitted, including when HRAM captured no
        /// cycles at all (e.g. no trigger occurred) or when the hardware returns no cycle data for the read.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current capture type is not <see cref="CaptType.Fail"/>, or when the hardware returns <c>PinResults</c>
        /// and <c>CycleOffsets</c> arrays of different lengths for a site.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <c>CapturedCycles</c> is a scalar whose derivation across multiple sites with differing capture depths is undocumented,
        /// so the maximum captured-cycle count across active sites (via <c>CapturedCyclesPerSite</c>) is used instead; when that
        /// maximum is <c>0</c> or less, an empty result is returned without issuing a read. As a further guard,
        /// a <see langword="null"/> <c>captureCycleData</c> from the hardware also short-circuits to an empty result rather than throwing.
        /// </para>
        /// <para>
        /// Reads are issued via <c>TheHdw.Digital.Pins(...)</c> rather than through <c>TheHdw.Digital.TimeDomains(_timeDomain)</c>,
        /// because <see cref="DriverDigitalDomain"/> does not expose a pin-scoped read API. In test programs that use multiple
        /// time domains over the same pins, this can resolve against the <b>System</b> time domain instead of <c>_timeDomain</c>.
        /// </para>
        /// <para>
        /// Only <c>WhichDataCycle 0</c> is read from each captured cycle. This is the only result for 1X pins, but for 2X/4X pins
        /// a failure that only shows up on DUT cycle 1..N-1 of a captured cycle is never reported — a silent false negative.
        /// Handling the pin multiplier is a known limitation of this implementation.
        /// </para>
        /// <para>
        /// <c>-1</c> reads all cycles captured in HRAM; <see langword="true"/> restricts the result to failing pins only (passing
        /// pins are stripped by the hardware). With <c>captureLimit -1</c> (max depth) this marshals the entire HRAM contents
        /// across all pins/sites in a single call — be mindful of the cost for large captures.
        /// </para>
        /// </remarks>
        public PinSite<int[]> ReadFails() {
            if (_captureType != CaptType.Fail) {
                throw new InvalidOperationException($"ReadFails is only supported for capture type '{CaptType.Fail}', but current type is '{_captureType}'.");
            }

            if (GetMaxCapturedCycles() <= 0) {
                return new PinSite<int[]>();
            }

            const int allCapturedCycles = -1;
            const bool failingPinsOnly = true;
            IHramCycleData captureCycleData = TheHdw.Digital.Pins(_digitalPins.Name).HRAM.ReadCapturedCycleData(allCapturedCycles, failingPinsOnly);
            if (captureCycleData is null) {
                return new PinSite<int[]>();
            }

            // WhichDataCycle: selects which DUT cycle's result to read for 2X/4X pins (0-based); irrelevant for 1X pins, where only one result exists per captured cycle.
            const int whichDataCycle = 0;
            PinSite<tlResultType[]> pinResults = captureCycleData.GetPinResults(whichDataCycle).ToPinSite<tlResultType[]>();

            PinSite<double[]> result = new();
            foreach (Site<tlResultType[]> pin in pinResults) {
                Site<double[]> siteResult = new();
                bool hasData = false;

                ForEachSite(site => {
                    if (pin[site] is null) return;

                    double[] siteOffsets = (double[])captureCycleData.CycleOffsets[site];
                    if (siteOffsets is null || pin[site].Length != siteOffsets.Length) {
                        throw new InvalidOperationException($"PinResults length ({pin[site].Length}) does not match CycleOffsets length ({siteOffsets?.Length ?? 0}) for site {site}.");
                    }

                    double[] failingCyclesBuffer = new double[pin[site].Length];
                    int failingCount = 0;
                    for (int index = 0; index < pin[site].Length; index++) {
                        if (pin[site][index] == tlResultType.Fail) {
                            failingCyclesBuffer[failingCount] = siteOffsets[index];
                            failingCount++;
                        }
                    }

                    if (failingCount == 0) return;

                    double[] failingCycles = new double[failingCount];
                    Array.Copy(failingCyclesBuffer, failingCycles, failingCount);

                    hasData = true;
                    siteResult[site] = failingCycles;
                });

                if (hasData) {
                    siteResult.PinName = pin.PinName;
                    result.Add(siteResult);
                }
            }

            return ConvertToIntArray(result);
        }

        /// <summary>
        /// Reads the raw digitized bit values captured in HRAM.
        /// </summary>
        /// <returns>
        /// Raw captured bit values per pin/site. Values are <c>0</c> (below VOL), <c>1</c> (above VOH), or <c>255</c> (midband/undefined).
        /// Pins/sites with no captured data are omitted, including when HRAM captured no cycles at all (e.g. no trigger occurred).
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current capture type is not <see cref="CaptType.STV"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <c>CapturedCycles</c> is a scalar whose derivation across multiple sites with differing capture depths is undocumented,
        /// so the maximum captured-cycle count across active sites (via <c>CapturedCyclesPerSite</c>) is used to drive how many
        /// cycles are requested from <c>ReadDataBits</c>, avoiding truncation of sites captured with more cycles than others.
        /// Sites that captured fewer cycles than this maximum may be over-read as a result — a deliberate tradeoff, since
        /// truncating a site's real data is worse than reading a few extra (likely stale/undefined) cycles for it.
        /// When <c>Size</c> is positive it also bounds what's visible on readback, but IG-XL does not document what
        /// <c>ReadDataBits</c> does when the requested cycle count exceeds <c>Size</c> (unlike <c>ReadCapturedCycleData</c>, which
        /// is documented to clamp) — so the requested count is defensively clamped to <c>Size</c> here when <c>Size</c> is positive.
        /// <c>Size &lt;= 0</c> (hardware default depth, or max depth via <c>-1</c>) means no such limit applies.
        /// </para>
        /// <para>
        /// Reads are issued via <c>TheHdw.Digital.Pins(...)</c> rather than through <c>TheHdw.Digital.TimeDomains(_timeDomain)</c>,
        /// because <see cref="DriverDigitalDomain"/> does not expose a pin-scoped read API. In test programs that use multiple
        /// time domains over the same pins, this can resolve against the <b>System</b> time domain instead of <c>_timeDomain</c>.
        /// </para>
        /// <para>
        /// Only <c>DutCycleOffset 0</c> is read for every captured cycle. This is the only sub-cycle for 1X pins, but for
        /// 2X/4X pins the other 1-3 DUT sub-cycles per minor cycle are silently dropped. Handling the pin multiplier is a
        /// known limitation of this implementation.
        /// </para>
        /// </remarks>
        public PinSite<double[]> ReadStv() {
            if (_captureType != CaptType.STV) {
                throw new InvalidOperationException($"ReadStv is only supported for capture type '{CaptType.STV}', but current type is '{_captureType}'.");
            }

            int dutCycleCount = GetMaxCapturedCycles();
            if (dutCycleCount <= 0) {
                return new PinSite<double[]>();
            }

            // ReadDataBits' behavior for a cycle count beyond Size is undocumented; clamp defensively rather than risk an out-of-range/garbage read (see remarks).
            if (_hram.Size > 0 && dutCycleCount > _hram.Size) {
                dutCycleCount = _hram.Size;
            }

            // startIndex: first captured cycle to read back. dutCycleOffset: which DUT sub-cycle to return within a minor cycle (see remarks).
            const int startIndex = 0;
            const int dutCycleOffset = 0;
            PinSite<byte[]> rawBits = TheHdw.Digital.Pins(_digitalPins.Name).HRAM.ReadDataBits(startIndex, dutCycleCount, dutCycleOffset).ToPinSite<byte[]>();

            PinSite<double[]> result = new();
            foreach (Site<byte[]> pin in rawBits) {
                Site<double[]> siteResult = new();
                bool hasData = false;

                ForEachSite(site => {
                    if (pin[site] is null || pin[site].Length == 0) return;

                    double[] values = new double[pin[site].Length];
                    for (int index = 0; index < pin[site].Length; index++) {
                        values[index] = pin[site][index];
                    }

                    hasData = true;
                    siteResult[site] = values;
                });

                if (hasData) {
                    siteResult.PinName = pin.PinName;
                    result.Add(siteResult);
                }
            }

            return result;
        }

        // Maximum captured-cycle count across active sites. CapturedCycles is a scalar with an undocumented multisite
        // derivation, so CapturedCyclesPerSite (the true per-site depth) is used instead; see ReadFails/ReadStv remarks.
        private int GetMaxCapturedCycles() {
            int maxCycles = 0;
            ForEachSite(site => {
                int cycles = (int)_hram.CapturedCyclesPerSite[site];
                if (cycles > maxCycles) {
                    maxCycles = cycles;
                }
            });

            return maxCycles;
        }

        // Converts cycle offsets from double to int, mirroring CmemCapture's ConvertToIntArray.
        private PinSite<int[]> ConvertToIntArray(PinSite<double[]> source) {
            PinSite<int[]> converted = new();
            foreach (Site<double[]> pin in source) {
                Site<int[]> siteConverted = new();
                siteConverted.PinName = pin.PinName;

                ForEachSite(site => {
                    if (pin[site] is not null) {
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

        // Applies the capture configuration to HRAM hardware based on the capture type.
        // TriggerType is derived from CaptureType: Fail -> TrigType.Fail, STV -> TrigType.First; WaitForEvent=false, PreTrigCycleCnt=0, StopOnFull=true.
        // Writes shared, per-time-domain hardware state (CaptureType, Size, trigger arm) — see the constructor's remarks on the resulting hazard with concurrent HramCapture instances.
        private void Configure() {
            _hram.CaptureType = _captureType;
            _hram.Size = _captureLimit;
            _triggerType = _captureType == CaptType.Fail ? TrigType.Fail : TrigType.First;

            // Trig=[_triggerType]: Fail -> capture starts at the first failing cycle; First -> capture starts at the first cycle (STV).
            // WaitForEvent=[false]: Do not wait for an additional cycle/vector/loop event before arming — start capturing based on Trig alone.
            // PreTrigCycleCnt=[0]: No pre-trigger cycles retained (see remarks above); keeps the full Size budget for post-trigger data.
            // StopOnFull=[true]: Halt the capture once HRAM fills up rather than wrapping and overwriting earlier captured cycles.
            _hram.SetTrigger(_triggerType, false, 0, true);
        }
    }
}
