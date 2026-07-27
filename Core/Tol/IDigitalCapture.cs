using Teradyne.Igxl.Interfaces.Public;

namespace Tol {
    /// <summary>
    /// Represents a digital capture memory (e.g. CMEM, HRAM, DSSC) instance that records and exposes pattern execution data on a per-pin or per-site basis.
    /// </summary>
    /// <remarks>
    /// Implementations are created via <see cref="Factory"/> methods.
    /// </remarks>
    public interface IDigitalCapture {
        /// <summary>
        /// The digital pins from which capture data is read.
        /// </summary>
        public IDigitalPins Pins { get; }

        /// <summary>
        /// Clears the capture memory configuration, discarding any previously captured data and returning the hardware to an unconfigured state.
        /// Call this after processing captured data to free hardware resources.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Reads failing cycle indices from the capture memory for each pin and site.
        /// </summary>
        /// <returns>
        /// A <see cref="PinSite{T}"/> of <c>int[]</c> where each entry contains the cycle offsets at which failures were recorded for that pin and site.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the capture is not configured for fail capture (e.g. <c>CmemCaptType.Fail</c>).
        /// </exception>
        public PinSite<int[]> ReadFails();

        /// <summary>
        /// Reads stored-vector (STV) data from the capture memory for each pin and site.
        /// </summary>
        /// <returns>
        /// A <see cref="PinSite{T}"/> of <c>double[]</c> where each entry contains the captured device data values for that pin and site.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the capture is not configured for STV capture (e.g. <c>CmemCaptType.STV</c>).
        /// </exception>
        public PinSite<double[]> ReadStv();
    }
}
