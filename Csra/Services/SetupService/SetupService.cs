using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;

namespace Csra.Services {

    /// <summary>
    /// Services.Setup - centralized test configuration management.
    /// </summary>
    [Serializable]
    public class Setup : ISetupService {

        private static ISetupService _instance = null;
        private double _settleWaitTimeOut;
        private readonly Dictionary<string, Csra.Setup> _setups;
        private static StringBuilder _stringBuilder = new();

        protected Setup() {
            _settleWaitTimeOut = 1 * s;
            _setups = [];
        }

        public static ISetupService Instance => _instance ??= new Setup();

        public bool AuditMode { get; set; }

        public int Count => _setups.Count;

        public bool VerboseMode { get; set; }

        public bool RespectSettlingTimeDefault { get; set; }

        public double SettleWaitTimeOut {
            get => _settleWaitTimeOut;
            set {
                if (value < 0) Api.Services.Alert.Error<ArgumentOutOfRangeException>("Settle wait time-out must be non-negative.");
                _settleWaitTimeOut = value;
            }
        }

        public void Add(Csra.Setup setup) => _setups.Add(setup.Name, setup);

        public bool Remove(string setupName) => _setups.Remove(setupName);

        public void Apply(string setupNames) {
            if (string.IsNullOrWhiteSpace(setupNames)) return;
            string[] split = setupNames.Split([','], StringSplitOptions.RemoveEmptyEntries);
            foreach (string setup in split) _setups[setup.Trim()].Apply();
            TheHdw.SettleWait(_settleWaitTimeOut); // now wait for all that (may) have accumulated
        }

        public void Diff(string setupNames) {
            if (string.IsNullOrWhiteSpace(setupNames)) return;
            string[] split = setupNames.Split([','], StringSplitOptions.RemoveEmptyEntries);
            foreach (string setup in split) _setups[setup.Trim()].Diff();
        }

        public void Init(InitMode initMode) { // this should have the exec interpose attribute, but that currently doesn't work ...
            //TheExec.AddOutput($"Init {initMode} ...");
            foreach (Csra.Setup setup in _setups.Values) setup.Init(initMode);
        }

        public void Dump() {
            const int headlineWidth = 80;
            Log($"\r\n### SETUPSERVICE: Dump All Setups {new string('#', headlineWidth)}".Substring(0, headlineWidth + 2), 0, 0x0);
            foreach (Csra.Setup setup in _setups.Values) setup.Dump();
        }

        public void Export(string path, string setupNames) {
            if (string.IsNullOrWhiteSpace(path)) Api.Services.Alert.Error<ArgumentException>("Argument 'path' must not be null or empty.");
            if (File.Exists(path)) File.Delete(path); // delete existing file

            string setupNamesToSplit = string.IsNullOrWhiteSpace(setupNames)
                ? string.Join(",", _setups.Keys)
                : setupNames;

            string[] rawSplitNames = setupNamesToSplit.Split([','], StringSplitOptions.RemoveEmptyEntries);
            string[] splitSetupNames = rawSplitNames.Select(s => s.Trim()).ToArray();

            _stringBuilder.AppendLine("{");
            int count = 0;
            foreach (string setup in splitSetupNames) {
                if (_setups[setup].Count > 0) {
                    _stringBuilder.AppendLine($"  \"{_setups[setup.Trim()].Name}\":{{");
                    _setups[setup].Export(path);
                    if (count < splitSetupNames.Length - 1) {
                        _stringBuilder.AppendLine("  },");
                    } else {
                        _stringBuilder.AppendLine("  }");
                    }
                }
                count++;
            }
            _stringBuilder.AppendLine("}");
            File.WriteAllText(path, _stringBuilder.ToString());
            _stringBuilder.Clear();
        }

        public static void EnqueueExportData(string line, bool newLine = true) {
            if (newLine) {
                _stringBuilder.AppendLine(line);
            } else {
                _stringBuilder.Append(line);
            }
        }

        public void Import(string path, bool overwrite = true) {
            if (string.IsNullOrWhiteSpace(path)) {
                Api.Services.Alert.Error<ArgumentNullException>("Path must not be null or empty.");
            }
            if (!File.Exists(path)) {
                Api.Services.Alert.Error<FileNotFoundException>($"File not found: {path}");
            }

            string[] lines = File.ReadAllLines(path);
            int level = 0;

            string pendingSetupName = string.Empty;
            string pendingSettingName = string.Empty;
            string pendingValue = string.Empty;
            string pendingPins = string.Empty;

            Csra.Setup currentSetup = null;

            foreach (string rawLine in lines.Select(l => l.Trim())) {
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                if (rawLine == "{") {
                    level++;
                } else if (rawLine.StartsWith("}")) {
                    level--;
                } else if (level == 1) {
                    pendingSetupName = RemoveSpecialCharacters(rawLine);
                    level++;
                } else if (level == 2 && pendingSetupName != string.Empty) {
                    currentSetup = new Csra.Setup(pendingSetupName);
                    if (overwrite && _setups.ContainsKey(currentSetup.Name)) {
                        _setups[currentSetup.Name] = currentSetup; // overwrite existing setup
                    } else if (!_setups.ContainsKey(currentSetup.Name)) {
                        _setups.Add(currentSetup.Name, currentSetup);
                    } else {
                        Api.Services.Alert.Error<InvalidOperationException>($"Setup '{currentSetup.Name}' already exists. Use overwrite option to replace it.");
                    }
                    pendingSetupName = string.Empty;
                    pendingSettingName = RemoveSpecialCharacters(rawLine);
                    level++;
                } else if (level == 2 && pendingSettingName == string.Empty) {
                    pendingSettingName = RemoveSpecialCharacters(rawLine);
                    level++;
                } else if (level == 3 && pendingSettingName != string.Empty) {
                    if (pendingValue == string.Empty) {
                        pendingValue = RemoveSpecialCharacters(rawLine.Substring(rawLine.IndexOf(':') + 2));
                    } else if (pendingPins == string.Empty) {
                        pendingPins = RemoveSpecialCharacters(rawLine.Substring(rawLine.IndexOf(':') + 2), true);
                        if (pendingPins == string.Empty) pendingPins = "N/A";
                    }
                    if (pendingPins != string.Empty) {
                        if (pendingPins == "N/A") pendingPins = string.Empty;
                        currentSetup.Add(GetSetting(pendingSettingName, pendingValue, pendingPins));
                        pendingSettingName = string.Empty;
                        pendingValue = string.Empty;
                        pendingPins = string.Empty;
                    }
                }
            }
        }

        public void Reset() {
            _setups.Clear();
            SettleWaitTimeOut = 1 * s;
            AuditMode = false;
            VerboseMode = false;
            RespectSettlingTimeDefault = false;
        }

        internal void Validate() {
            foreach (Csra.Setup setup in _setups.Values) setup.Validate();
        }

        public void Log(string message, int level, int rgb) {
            ColorConstants bgr = (ColorConstants)((rgb & 0xff0000) >> 16 | rgb & 0xff00 | (rgb & 0xff) << 16);
            Api.Services.Alert.Log($"{new string(' ', level * 5)}{message}", bgr, level == 0);
        }
        internal string RemoveSpecialCharacters(string input, bool excludeNonTrailingCommas = false) {
            const string specialChars = " \t\n\r;:\"'{}";
            const string specialCharsAndComma = specialChars + ",";
            if (excludeNonTrailingCommas) {
                string cleanString = new([.. input.Where(c => !specialChars.Contains(c))]);
                return cleanString.Substring(0, cleanString.Length - 1);
            } else {
                return new string([.. input.Where(c => !specialCharsAndComma.Contains(c))]);
            }
        }
        internal static ISetting GetSetting(string settingType, string value, string pins) {
            if (settingType == "TheHdw.Dcvi.Pins.BleederResistor.CurrentLoad") {
                #region DCVI
                return new Setting.TheHdw.Dcvi.Pins.BleederResistor.CurrentLoad(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.BleederResistor.Mode") {
                return new Setting.TheHdw.Dcvi.Pins.BleederResistor.Mode(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.FoldCurrentLimit.Behavior") {
                return new Setting.TheHdw.Dcvi.Pins.FoldCurrentLimit.Behavior(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.FoldCurrentLimit.Timeout") {
                return new Setting.TheHdw.Dcvi.Pins.FoldCurrentLimit.Timeout(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.ComplianceRange_Negative") {
                return new Setting.TheHdw.Dcvi.Pins.ComplianceRange_Negative(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.ComplianceRange_Positive") {
                return new Setting.TheHdw.Dcvi.Pins.ComplianceRange_Positive(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.Connect") {
                return new Setting.TheHdw.Dcvi.Pins.Connect(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.Current") {
                return new Setting.TheHdw.Dcvi.Pins.Current(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.CurrentRange") {
                return new Setting.TheHdw.Dcvi.Pins.CurrentRange(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.Gate") {
                return new Setting.TheHdw.Dcvi.Pins.Gate(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.Mode") {
                return new Setting.TheHdw.Dcvi.Pins.Mode(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.NominalBandwidth") {
                return new Setting.TheHdw.Dcvi.Pins.NominalBandwidth(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.Voltage") {
                return new Setting.TheHdw.Dcvi.Pins.Voltage(value, pins);
            } else if (settingType == "TheHdw.Dcvi.Pins.VoltageRange") {
                return new Setting.TheHdw.Dcvi.Pins.VoltageRange(value, pins);
                #endregion
                #region DCVS
            } else if (settingType == "TheHdw.Dcvs.Pins.CurrentLimit.Sink.FoldLimit.Level") {
                return new Setting.TheHdw.Dcvs.Pins.CurrentLimit.Sink.FoldLimit.Level(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.CurrentLimit.Sink.OverloadLimit.Level") {
                return new Setting.TheHdw.Dcvs.Pins.CurrentLimit.Sink.OverloadLimit.Level(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.CurrentLimit.Source.FoldLimit.Level") {
                return new Setting.TheHdw.Dcvs.Pins.CurrentLimit.Source.FoldLimit.Level(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.CurrentLimit.Source.OverloadLimit.Level") {
                return new Setting.TheHdw.Dcvs.Pins.CurrentLimit.Source.OverloadLimit.Level(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.BleederResistor") {
                return new Setting.TheHdw.Dcvs.Pins.BleederResistor(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.Connect") {
                return new Setting.TheHdw.Dcvs.Pins.Connect(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.CurrentRange") {
                return new Setting.TheHdw.Dcvs.Pins.CurrentRange(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.Gate") {
                return new Setting.TheHdw.Dcvs.Pins.Gate(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.Mode") {
                return new Setting.TheHdw.Dcvs.Pins.Mode(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.Voltage") {
                return new Setting.TheHdw.Dcvs.Pins.Voltage(value, pins);
            } else if (settingType == "TheHdw.Dcvs.Pins.VoltageRange") {
                return new Setting.TheHdw.Dcvs.Pins.VoltageRange(value, pins);
                #endregion
                #region Digital
            } else if (settingType == "TheHdw.Digital.Pins.Levels.DriverMode") {
                return new Setting.TheHdw.Digital.Pins.Levels.DriverMode(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Ioh") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Ioh(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Iol") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Iol(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vch") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vch(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vcl") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vcl(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vih") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vih(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vil") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vil(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Voh") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Voh(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vol") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vol(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Levels.Value_Vt") {
                return new Setting.TheHdw.Digital.Pins.Levels.Value_Vt(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.Connect") {
                return new Setting.TheHdw.Digital.Pins.Connect(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.InitState") {
                return new Setting.TheHdw.Digital.Pins.InitState(value, pins);
            } else if (settingType == "TheHdw.Digital.Pins.StartState") {
                return new Setting.TheHdw.Digital.Pins.StartState(value, pins);
                #endregion
                #region Ppmu
            } else if (settingType == "TheHdw.Ppmu.Pins.Connect") {
                return new Setting.TheHdw.Ppmu.Pins.Connect(value, pins);
            } else if (settingType == "TheHdw.Ppmu.Pins.Gate") {
                return new Setting.TheHdw.Ppmu.Pins.Gate(value, pins);
                #endregion
                #region Utility
            } else if (settingType == "TheHdw.Utility.Pins.State") {
                return new Setting.TheHdw.Utility.Pins.State(value, pins);
                #endregion
                #region General
            } else if (settingType == "TheHdw.SetSettlingTimer") {
                return new Setting.TheHdw.SetSettlingTimer(value);
            } else if (settingType == "TheHdw.SettleWait") {
                return new Setting.TheHdw.SettleWait(value);
            } else if (settingType == "TheHdw.Wait") {
                return new Setting.TheHdw.Wait(value);
                #endregion
            }
            throw new NotSupportedException($"Setting type '{settingType}' is not supported.");
            // }
        }
    }
}
