
namespace Tol {

    /// <summary>
    /// The interface IPins.
    /// </summary>
    public interface IPins<out TPin> where TPin : IPins<TPin> {

        /// <summary>
        /// Gets the name of the pin, pin group or comma separated pin list.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the pin list item (array of pins as a single item).
        /// </summary>
        /// <returns>Returns an array of pins as single item.</returns>
        public TPin[] GetPinListItem();

        /// <summary>
        /// Gets the individual pins (array of single pins).
        /// </summary>
        /// <returns>Returns an array of single pins.</returns>
        public TPin[] GetIndividualPins();
    }
}
