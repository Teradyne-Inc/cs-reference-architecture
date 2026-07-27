using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    public interface IDigitalPins : IPins<IDigitalPins> {

        /// <summary>
        /// Handle to legacy IG-XL object.
        /// </summary>
        public DriverDigitalPins HardwareApi { get; }

        /// <summary>
        /// Handle to PPMU pin settings.
        /// </summary>
        public IPpmuPins Ppmu { get; }

        /// <summary>
        /// The PPMU init state, will be set immediately.
        /// </summary>
        public IValuePerSite<ChInitState> InitState { get; }

        /// <summary>
        /// The PPMU start state, will be set before each pattern burst starts.
        /// </summary>
        public IValuePerSite<ChStartState> StartState { get; }

        /// <summary>
        /// The PPMU lock state, locks the state during a pattern burst.
        /// </summary>
        public IValuePerSite<tlLockState> LockState { get; }

        /// <summary>
        /// Connects the specified DUT pins to the Digital.
        /// </summary>
        public void Connect();

        /// <summary>
        /// Disconnects the specified DUT pins from the Digital.
        /// </summary>
        public void Disconnect();
    }
}
