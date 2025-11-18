namespace Csra.Interfaces {

    public interface ISetupService : IService {

        /// <summary>
        /// Reads or sets audit mode (performs hardware reads to confirm cached data is correct).
        /// </summary>
        public bool AuditMode { get; set; }

        /// <summary>
        /// Gets the number of setups contained in the SetupService.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Reads or sets the global default for respecting settling times when applying settings.
        /// </summary>
        public bool RespectSettlingTimeDefault { get; set; }

        /// <summary>
        /// Reads or sets the global default for settling time-out.
        /// </summary>
        public double SettleWaitTimeOut { get; set; }

        /// <summary>
        /// Reads or sets verbose mode (prints detailed information to log output target).
        /// </summary>
        public bool VerboseMode { get; set; }

        /// <summary>
        /// Adds the specified setup to the SetupService.
        /// </summary>
        /// <param name="setup">The setup to add.</param>
        public void Add(Setup setup);

        /// <summary>
        /// Applies one or multiple named setups from the SetupService. If more than one is specified, the requested sequence is maintained. Does nothing if
        /// the name is null or empty.
        /// </summary>
        /// <param name="setupNames">The setup(s) to be applied (CSV).</param>
        public void Apply(string setupNames);

        /// <summary>
        /// Performs a diff of the specified setups against current hardware state and logs the differences to the output window.
        /// </summary>
        /// <param name="setupNames"></param>
        public void Diff(string setupNames);

        /// <summary>
        /// Dumps all setups to the log output target.
        /// </summary>
        public void Dump();

        /// <summary>
        /// Exports the specified setups to a file at the specified path. The file format is JSON.
        /// If the file already exists, it will be overwritten. If the path is null or empty, the setups will not be exported.
        /// An empty parameter <paramref name="setupNames"/> will export all setups in the SetupService.
        /// </summary>
        /// <param name="setupNames">The setup(s) to be exported (CSV).</param>
        /// <param name="path">The file system path where the setups will be exported.</param>
        public void Export(string path, string setupNames);

        /// <summary>
        /// Imports setups from a file at the specified path. If overwrite is true, setups with the same name are overwritten.
        /// </summary>
        /// <param name="path">The file system path from where the setups will be imported.</param>
        /// <param name="overwrite">Optional. If set to <c>true</c>, existing setups will be overwritten.</param>
        public void Import(string path, bool overwrite = true);

        /// <summary>
        /// Performs the specified initialization to all setups in the SetupService.
        /// </summary>
        /// <param name="initMode">The init mode.</param>
        public void Init(InitMode initMode);

        /// <summary>
        /// Removes the setup with the specified setupName from the SetupService.
        /// </summary>
        /// <param name="setupName">The setup to be removed.</param>
        /// <returns><see langword="true"/> if the `setup` is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string setupName);

        /// <summary>
        /// Logs a message with a specified severity level and color.
        /// </summary>
        /// <param name="message">The message to be logged. Cannot be null or empty.</param>
        /// <param name="level">The level of indentation for the message.</param>
        /// <param name="rgb">The RGB color value used to display the message. Must be a valid RGB integer.</param>
        public void Log(string message, int level, int rgb);
    }
}
