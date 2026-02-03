namespace Csra.Interfaces {

    public interface IServiceBase {

        /// <summary>
        /// Initialize the service. This is called by the API when the service is first used.
        /// </summary>
        public void Reset();
    }
}
