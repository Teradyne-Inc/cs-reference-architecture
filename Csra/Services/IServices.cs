namespace Csra.Interfaces {

    public interface IServices {

        public IAlertService Alert { get; }
        public IBehaviorService Behavior { get; }
        public ISetupService Setup { get; }
        public IStorageService Storage { get; }
        public ITransactionService Transaction { get; }

        /// <summary>
        /// Exchanges the default branch implementations with user-provided implementations.
        /// </summary>
        /// <param name="alert">User-provided implementation of <see cref="IAlertService"/>.</param>
        /// <param name="behavior">User-provided implementation of <see cref="IBehaviorService"/>.</param>
        /// <param name="setup">User-provided implementation of <see cref="ISetupService"/>.</param>
        /// <param name="storage">User-provided implementation of <see cref="IStorageService"/>.</param>
        /// <param name="transaction">User-provided implementation of <see cref="ITransactionService"/>.</param>
        /// <remarks>
        /// If any parameter is <c>null</c>, the method will revert to the original implementation 
        /// provided by the csra library.
        /// </remarks>
        public void Configure(IAlertService alert = null, IBehaviorService behavior = null, ISetupService setup = null, IStorageService storage = null,
            ITransactionService transaction = null);
    }
}
