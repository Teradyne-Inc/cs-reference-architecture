using Teradyne.Igxl.Interfaces.Public;

namespace Tol {
    public interface IDigitalCapture {
        /// <summary>
        /// Capture memory failing cycle data.
        /// </summary>
        public PinSite<int[]> FailingCycleData { get; }

        /// <summary>
        /// Capture memory device data.
        /// </summary>
        public PinSite<double[]> DeviceData { get; }

        /// <summary>
        /// Pins to capture memory from.
        /// </summary>
        public IDigitalPins Pins { get; }

        /// <summary>
        /// Configures the hardware for capture memory.
        /// </summary>
        public void Configure();

        /// <summary>
        /// Resets capture memory.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Reads capture data.
        /// </summary>
        public void Read();
    }
}
