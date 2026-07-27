using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {
    /// <summary>
    /// DSSC (Digital Sub-System Capture) implementation of <see cref="IDigitalCapture"/> that records high-speed
    /// digital waveform sample data via <c>TheHdw.DSSC</c> and exposes it on a per-pin, per-site basis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DSSC is a waveform/sample-oriented capture backend: it produces stored-vector (STV) style sample data and has
    /// no failing-cycle concept. Consequently <see cref="ReadStv"/> returns the captured samples while
    /// <see cref="ReadFails"/> always throws.
    /// </para>
    /// <para>
    /// <b>Shared signal collection:</b> the DSSC capture signals are owned per pins+pattern combination, not per
    /// <see cref="DsscCapture"/> instance. Multiple instances created for the same pins and pattern (but different
    /// signal names) share one underlying <c>Capture.Signals</c> collection, and <see cref="Reset"/> clears the whole
    /// collection. See <see cref="Reset"/> for the implications.
    /// </para>
    /// <para>
    /// Typical usage sequence:
    /// <list type="number">
    ///   <item><description>Create the capture (configures the DSSC signal on the hardware immediately).</description></item>
    ///   <item><description>Execute a pattern burst.</description></item>
    ///   <item><description>Call <see cref="ReadStv"/> to retrieve the captured waveform samples.</description></item>
    ///   <item><description>Call <see cref="Reset"/> to release the configured signal after processing.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class DsscCapture : IDigitalCapture {
        /// <inheritdoc/>
        public IDigitalPins Pins => _digitalPins;

        private readonly IDigitalPins _digitalPins;
        private readonly string _signalName;
        private readonly string _pattern;
        private readonly int _sampleSize;
        private readonly double _sampleRate;

        // Central accessor for this capture's DSSC signal collection on its pins + pattern.
        // Re-resolved on each access (the hardware chain is not cached), keeping the lookup in one place.
        private DriverDSSCCaptureSignals CaptureSignals =>
            TheHdw.DSSC.Pins(_digitalPins.Name).Pattern(_pattern).Capture.Signals;

        /// <summary>
        /// Initializes a new <see cref="DsscCapture"/> and configures the DSSC capture signal on the hardware immediately.
        /// </summary>
        /// <param name="digitalPins">The digital pins to capture waveform data from. Required.</param>
        /// <param name="signalName">The DSSC capture signal name (e.g. <c>"cap_tdo"</c>) added to the capture chain. Required.</param>
        /// <param name="pattern">The pattern name this capture is associated with. Required.</param>
        /// <param name="sampleSize">Number of samples to capture. Defaults to 65536.</param>
        /// <param name="sampleRate">Sample rate in Hz. Defaults to 200000.0 (200 kHz).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="digitalPins"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="signalName"/> or <paramref name="pattern"/> is null, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sampleSize"/> is not greater than zero, or when <paramref name="sampleRate"/> is not a finite, positive value.</exception>
        public DsscCapture(IDigitalPins digitalPins, string signalName, string pattern,
                           int sampleSize = 65536, double sampleRate = 200000.0) {
            if (digitalPins is null) {
                throw new ArgumentNullException(nameof(digitalPins), "Digital pins are required to configure a DSSC capture.");
            }

            if (string.IsNullOrWhiteSpace(signalName)) {
                throw new ArgumentException("A DSSC capture signal name is required.", nameof(signalName));
            }

            if (string.IsNullOrWhiteSpace(pattern)) {
                throw new ArgumentException("A pattern name is required for a DSSC capture.", nameof(pattern));
            }

            if (sampleSize <= 0) {
                throw new ArgumentOutOfRangeException(nameof(sampleSize), sampleSize, "Sample size must be greater than zero.");
            }

            if (double.IsNaN(sampleRate) || double.IsInfinity(sampleRate) || sampleRate <= 0) {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be a finite, positive value.");
            }

            _digitalPins = digitalPins;
            _signalName = signalName;
            _pattern = pattern;
            _sampleSize = sampleSize;
            _sampleRate = sampleRate;

            this.Configure();
        }

        /// <summary>
        /// Reinitializes the DSSC capture signal collection, clearing all configured signals so subsequent patterns do
        /// not unintentionally trigger capture recording. Safe to call multiple times.
        /// </summary>
        /// <remarks>
        /// The IG-XL DSSC driver exposes no way to remove an individual signal, so this calls
        /// <c>Capture.Signals.Reinitialize()</c>, which clears <b>every</b> signal configured on this pins+pattern
        /// combination — not just <see cref="_signalName"/>. If several <see cref="DsscCapture"/> instances share the
        /// same pins and pattern with different signal names, calling <see cref="Reset"/> on one instance will also
        /// discard the others' configured signals. In that scenario, read all signals before resetting any of them.
        /// </remarks>
        public void Reset() {
            CaptureSignals.Reinitialize();
        }

        /// <summary>
        /// Not supported for DSSC. DSSC captures waveform sample data and has no failing-cycle concept.
        /// </summary>
        /// <returns>This method does not return; it always throws.</returns>
        /// <exception cref="InvalidOperationException">Always thrown. Use <see cref="ReadStv"/> instead.</exception>
        public PinSite<int[]> ReadFails() {
            throw new InvalidOperationException($"{nameof(ReadFails)} is not supported for DSSC capture; DSSC captures waveform sample data. Use {nameof(ReadStv)} instead.");
        }

        /// <summary>
        /// Reads captured DSSC waveform samples for each pin and site.
        /// </summary>
        /// <returns>
        /// A <see cref="PinSite{T}"/> of <c>double[]</c> where each entry contains the captured sample values for that pin and site.
        /// </returns>
        public PinSite<double[]> ReadStv() {
            return CaptureSignals[_signalName].DspWave.ToPinSite<double[]>();
        }

        // Applies the DSSC capture configuration to the hardware: adds the signal to the capture chain (only if it is
        // not already registered, so reusing a signal name without a prior Reset does not fail on a duplicate Add),
        // sets sample size and rate, then loads the settings so the signal is armed for the next pattern.
        private void Configure() {
            DriverDSSCCaptureSignals signals = CaptureSignals;
            if (!signals.Contains(_signalName)) {
                signals.Add(_signalName);
            }

            DriverDSSCCaptureSignalsItem signal = signals[_signalName];
            signal.SampleSize = _sampleSize;
            signal.SampleRate = _sampleRate;
            signal.LoadSettings();
        }
    }
}
