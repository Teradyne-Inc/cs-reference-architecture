using System;
using System.IO;
using System.Collections.Generic;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Setting;
using Csra.Interfaces;

namespace Csra {

    /// <summary>
    /// Setup - a named collection of settings.
    /// </summary>
    [Serializable]
    public class Setup {

        private List<ISetting> _settings { get; set; } = [];

        /// <summary>
        /// Gets the name of the setup.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new setup with the specified name.
        /// </summary>
        /// <param name="name">The setup name - an arbitrary, case sensitive string that can include numbers, spaces and special characters. Empty and
        /// null strings are not supported.</param>
#pragma warning disable IDE0290 // Use primary constructor - use explicit constructor for dedicated XML comments
        public Setup(string name) {
            if (string.IsNullOrEmpty(name)) Api.Services.Alert.Error<ArgumentException>("Setup name must not be null or empty.");
            Name = name;
        }
#pragma warning restore IDE0290 // Use primary constructor

        /// <summary>
        /// Adds a setting to the setup.
        /// </summary>
        /// <param name="setting">The setting to add.</param>
        public void Add(ISetting setting) => _settings.Add(setting);

        /// <summary>
        /// Gets the number of settings contained in the setup.
        /// </summary>
        public int Count => _settings.Count;

        internal void Apply() {
            if (Api.Services.Setup.VerboseMode) {
                Api.Services.Alert.Log($"\r\nTest: '{TheExec.DataManager.InstanceName}'", bold: true);
                Api.Services.Alert.Log($"     Apply Setup: '{Name}'");
            }
            foreach (ISetting setting in _settings) setting.Apply();
        }

        internal void Diff() {
            if (Api.Services.Setup.VerboseMode) {
                Api.Services.Alert.Log($"\r\nTest: '{TheExec.DataManager.InstanceName}'", bold: true);
                Api.Services.Alert.Log($"     Diff hardware state to Setup: '{Name}'");
            }
            foreach (ISetting setting in _settings) setting.Diff();
        }

        internal void Init(InitMode initMode) {
            foreach (ISetting setting in _settings) setting.Init(initMode);
        }

        internal void Dump() {
            var bgr = (ColorConstants)((0x0 & 0xff0000) >> 16 | 0x0 & 0xff00 | (0x0 & 0xff) << 16);
            Api.Services.Alert.Log($"{new string(' ', 1 * 5)}{$"Setup: '{Name}'"}", bgr, 1 == 0);
            foreach (ISetting setting in _settings) setting.Dump();
        }

        internal void Export(string path) {
            for (int i = 0; i < _settings.Count; i++) {
                _settings[i].Export(path);
                if (i < _settings.Count - 1) {
                    Services.Setup.EnqueueExportData(",");
                } else {
                    Services.Setup.EnqueueExportData("");
                }
            }
        }

        internal void Validate() {
            foreach (var setting in _settings) {
                ((ISettingInternal)setting).Validate();
            } 
        }
    }
}
