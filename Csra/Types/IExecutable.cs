namespace Csra {

    /// <summary>
    /// Defines a contract that a customer needs to fulfill to use with TestBlock (e.g. RunPatternConditionalStop).
    /// </summary>
    public interface IExecutable {

        /// <summary>
        /// Method that will be executed by using TestBlock (e.g. RunPatternConditionalStop).
        /// </summary>
        public void Execute();

        /// <summary>
        /// Method that will be executed to clear result fields.
        /// Can be empty if not needed.
        /// </summary>
        public void Clear();
    }
}