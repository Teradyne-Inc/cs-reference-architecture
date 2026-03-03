namespace Csra.Interfaces {

    public interface ISetting {

        /// <summary>
        /// Applies the setting to the hardware, in case and only for those pins that need it.
        /// </summary>
        void Apply();

        /// <summary>
        /// Performs the specified initialization to the setting.
        /// </summary>
        /// <param name="initMode">The init mode.</param>
        void Init(InitMode initMode);

        /// <summary>
        /// Dumps the setting to the log output target.
        /// </summary>
        void Dump();

        /// <summary>
        /// Performs a diff on the setting. This is used to check if the setting is already applied to the hardware.
        /// </summary>
        void Diff();

        /// <summary>
        /// Exports the setting to a file at the specified path. The file format is implementation specific, but should be human readable.
        /// </summary>
        /// <param name="path">The file system path where the setups will be exported.</param>
        void Export(string path);
    }
}
