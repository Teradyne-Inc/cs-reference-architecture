using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    public interface IUtilityPins : IPins<IUtilityPins> {

        /// <summary>
        /// Handle to legacy IG-XL object.
        /// </summary>
        public tlDriverUtilityPins HardwareApi { get; }

        /// <summary>
        /// The Utility state.
        /// </summary>
        public IValuePerSite<tlUtilBitState> State { get; }
    }
}
