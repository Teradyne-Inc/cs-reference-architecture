using System;
using System.IO;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System.Linq;
using Csra;
using Csra.Interfaces;

namespace Csra.Setting {

    /// <summary>
    /// Base class for all Services.Setup settings.
    /// </summary>
    /// <typeparam name="T">The setting's type.</typeparam>
    [Serializable]
    public abstract class SettingBase<T> : ISettingInternal, ISetting {

        private T _value;
        protected List<string> _pins;
        private bool _readFuncDefined = false;
        private Dictionary<string, T> _cache;
        private bool _doubleReadCompare = false; //for the instrument features where the readback can't be directly compared to the cached (=previous set) value
        private T _initValue = default;
        protected string _unit = string.Empty; // protected required for derived classes to access in serializer
        private InitMode _initMode = InitMode.Creation;

        /// <summary>
        /// Hands the setting's argument parameters to the base class. Typically used for instrument-related settings with or without pin relation.
        /// </summary>
        /// <param name="value">The target value of this setting.</param>
        /// <param name="pins">The pins this setting is for (empty if not applicable).</param>
        /// <param name="=isNoPinSetting">Required to avoid warnings for settings that don't need pins (e.g. SettleWait)</param>
        protected void SetArguments(T value, string pinList, bool hasPins) {
            _value = value;
            if (pinList is not null) {
                if (TheExec.DataManager.DecomposePinList(pinList, out string[] pinArray, out _) != 0)
                    Api.Services.Alert.Warning($"Services.Setup.Setting<{typeof(T).Name}> - DecomposePinList failed");
                _pins = pinArray?.ToList() ?? [];
            } else {
                _pins = [];
            }
            if (_pins.Count == 0 && hasPins) Api.Services.Alert.Error("Missing pins! No pins given in definition.");
        }

        protected abstract void SetAction(string pinList, T value);

        protected abstract T[] ReadFunc(string pin);

        /// <summary>
        /// Hands the setting's argument parameters to the base class. Typically used for custom settings.
        /// </summary>
        /// <param name="value">The value to call setAction.</param>
        /// <param name="key">A key to feed the cache.</param>
        protected void SetArguments(T value, string key) {
            _value = value;
            _pins = string.IsNullOrWhiteSpace(key) ? [] : [key];
        }

        /// <summary>
        /// Hands the setting's behavior parameters to the base class.
        /// </summary>
        /// <param name="initValue">The value this setting is being initialized to by the system.</param>
        /// <param name="unit">The unit string for the setting's value.</param>
        /// <param name="initMode">The event that initialization of this setting in the system.</param>
        protected void SetBehavior(T initValue, string unit, InitMode initMode, bool doubleReadCompare) {
            _initValue = initValue;
            _unit = unit;
            _initMode = initMode;
            _doubleReadCompare = doubleReadCompare;
        }

        /// <summary>
        /// Hands the setting's context parameters to the base class.
        /// </summary>
        /// <param name="setAction">The action required to apply the setting to the system.</param>
        /// <param name="readFunc">The delegate to call to read back the setting's state from the system.</param>
        /// <param name="staticCache">The handle to the static cache object for the setting's type.</param>
        protected void SetContext(bool readFuncDefined, Dictionary<string, T> staticCache) {
            _readFuncDefined = readFuncDefined;
            if (staticCache is not null) {
                _cache = staticCache;// tricky, a static cache for each derived class, handed down to the generic base class so it can centrally handle all ...
                foreach (string pin in _pins) {
                    if (!_cache.ContainsKey(pin)) _cache.Add(pin, _initValue);
                    else _cache[pin] = _initValue; // required to get back to a defined state after re-running the setup definitions.
                }
            }
        }

        /// <summary>
        /// Compares two values of the setting's type. Override if special treatment is needed.
        /// </summary>
        /// <param name="a">The first value for the comparison.</param>
        /// <param name="b">The second value for the comparision.</param>
        /// <returns>Returns 'true' if equal; otherwise false.</returns>
        protected virtual bool CompareValue(T a, T b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        /// <summary>
        /// Validates the setting's value. Override needed per setting.
        /// </summary>
        // ToDo: switch back to commented code below when issue #1652 is done
        public virtual void Validate() { }
        //public virtual void Validate() { Services.Alert.Warning($"Services.Setup.Setting {typeof(T).Name} - Validation not implemented"); }

        /// <summary>
        /// Serializes the setting's value to a string. Override if special treatment is needed.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The string representation of value.</returns>
        protected virtual string SerializeValue(T value) => value.ToString();

        /// <summary>
        /// Updates the value for specified pins in the cache. Used to make SetupService aware of manual hardware changes.
        /// </summary>
        /// <param name="value">The new value (typically what the hardware has manually be set to).</param>
        /// <param name="pins">The list of pins the new value applies to.</param>
        /// <param name="cache">A reference to that static cache object per setting.</param>
        protected static void SetCacheInternal(T value, string pinList, Dictionary<string, T> cache) {
            if (cache is null) return;
            string[] pinArray;
            if (cache.ContainsKey(pinList)) {
                pinArray = [pinList];
            } else if (TheExec.DataManager.DecomposePinList(pinList, out pinArray, out _) != 0) {
                Api.Services.Alert.Warning($"Services.Setup.Cache<{typeof(T).Name}> - DecomposePinList failed");
            }
            List<string> pins = pinArray?.ToList() ?? [];
            foreach (string pin in pins) {
                if (cache.ContainsKey(pin)) cache[pin] = value;
                else Api.Services.Alert.Warning($"Services.Setup.Cache<{typeof(T).Name}> - pin '{pin}' not found");
            }
        }

        public void Apply() { // refactor & clean this one up
            List<string> programPins = [];
            if (_cache is null) { // for settings that don't have a cache, like TheHdw.Wait()
                string allPins = string.Join(", ", _pins);
                SetAction(allPins, _value);
                if (Api.Services.Setup.VerboseMode) {
                    Api.Services.Alert.Log($"          {ToString()} {(_pins.Count > 0 ? $"need to program '{allPins}'" : string.Empty)}", (ColorConstants)0x777700);
                }
                return;
            } else if (!_readFuncDefined) { // Cache but no readFunc, like Custom()
                foreach (string pin in _pins) {
                    T cachedState = _cache[pin];
                    if (!CompareValue(cachedState, _value)) {
                        programPins.Add(pin);
                    }
                }
            } else { // setAction, readFunc and setAction are present
                foreach (string pin in _pins) {
                    bool hardwareBusted = false; // be optimistic
                    T cachedState = _cache[pin];
                    if (Api.Services.Setup.AuditMode) {
                        if (_doubleReadCompare) {
                            T[] hardwareState1 = ReadFunc(pin);
                            SetAction(pin, _value);
                            T[] hardwareState2 = ReadFunc(pin);
                            ForEachSite(site => {
                                if (!CompareValue(hardwareState1[site], hardwareState2[site])) {
                                    if (Api.Services.Setup.VerboseMode) Api.Services.Alert.Log($"          Hardware mismatch: pin '{pin}' @ site '{site}' reads back " +
                                        $"'{SerializeValue(hardwareState1[site])}' but should be '{SerializeValue(hardwareState2[site])}' after being set to " +
                                        $"'{SerializeValue(cachedState)}'", (ColorConstants)0x77);
                                    hardwareBusted = true;
                                }
                            });
                        } else {
                            T[] hardwareState = ReadFunc(pin);
                            ForEachSite(site => {
                                // Note: Update cahe here since the some command need latest hardware status in _setAction()
                                _cache[pin] = hardwareState[site];
                                if (!CompareValue(hardwareState[site], cachedState)) {
                                    if (Api.Services.Setup.VerboseMode) Api.Services.Alert.Log($"          Hardware mismatch: pin '{pin}' @ site '{site}' is " +
                                        $"'{SerializeValue(hardwareState[site])}' but expected to be '{SerializeValue(cachedState)}'", (ColorConstants)0x77);
                                    hardwareBusted = true;
                                }
                            });
                        }
                    }
                    if (!CompareValue(cachedState, _value) || hardwareBusted) {
                        programPins.Add(pin);
                    }
                }
            }
            if (programPins.Count > 0) {
                string programPinList = string.Join(", ", programPins);
                SetAction(programPinList, _value);
                if (Api.Services.Setup.VerboseMode) Api.Services.Alert.Log($"          {ToString()} need to program '{programPinList}'", (ColorConstants)0x770000);
                // Note: Cache update must happen after _setAction() to allow it to read unmodified state
                foreach (string pin in programPins) _cache[pin] = _value;
            } else if (Api.Services.Setup.VerboseMode) Api.Services.Alert.Log($"          {ToString()} no action needed", (ColorConstants)0x007700);
        }

        /// <summary>
        /// Reads the hardware state and compares it to the setting/cached value. Target and actual status are output.
        /// </summary>
        public void Diff() {
            if (!_readFuncDefined || _cache is null) return;
            foreach (string pin in _pins) {
                T cachedState = _cache[pin];
                T[] hardwareState = ReadFunc(pin);
                ForEachSite(site => {
                    if (!CompareValue(hardwareState[site], _value)) {
                        if (_doubleReadCompare)
                            Api.Services.Alert.Log($"          Hardware does not match setting value but may still be correct (doublecompare type): pin '{pin}' @ " +
                                $"site '{site}' reads back '{SerializeValue(hardwareState[site])}', setting value is '{SerializeValue(_value)}', cache value " +
                                $"is '{SerializeValue(cachedState)}'", (ColorConstants)0x77);
                        else
                            Api.Services.Alert.Log($"          Hardware does not match setting value: pin '{pin}' @ site '{site}' reads back " +
                                $"'{SerializeValue(hardwareState[site])}' but should be '{SerializeValue(_value)}'", (ColorConstants)0x77);
                    } else {
                        Api.Services.Alert.Log($"          Hardware matches setting value: pin '{pin}' @ site '{site}' reads back " +
                            $"'{SerializeValue(hardwareState[site])}' and should be '{SerializeValue(_value)}'", (ColorConstants)0x007700);
                    }
                });
            }
        }

        /// <summary>
        /// Gets a string representation of the setting in a human-readable form.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{GetType().Name}{(_pins.Count > 0 ? $" @ '{string.Join(", ", _pins)}'" : string.Empty)} --> '" +
            $"{SerializeValue(_value)}':";

        public void Init(InitMode initMode) {
            if (_cache is null) return;
            if (_initMode.HasFlag(initMode)) foreach (string pin in _pins) _cache[pin] = _initValue;
        }

        public void Dump() {
            ColorConstants bgr = (ColorConstants)((0x0 & 0xff0000) >> 16 | 0x0 & 0xff00 | (0x0 & 0xff) << 16);
            Api.Services.Alert.Log($"{new string(' ', 2 * 5)}{ToString()}", bgr, 2 == 0);
        }

        public void Export(string path) {
            string settingType = GetType().ToString().Substring(GetType().ToString().IndexOf("TheHdw"));
            Services.Setup.EnqueueExportData($"{Indent(2)}\"{settingType}\": {{");
            Services.Setup.EnqueueExportData($"{Indent(3)}\"Value\": \"{_value.ToString()}\",");
            Services.Setup.EnqueueExportData($"{Indent(3)}\"Pins\": \"{string.Join(",", _pins.ToArray())}\",");
            Services.Setup.EnqueueExportData($"{Indent(2)}}}", false);
        }

        public string Indent(int level) => new string(' ', level * 2); // 2 spaces per indent level

        protected static TEnum ParseEnum<TEnum>(string value) where TEnum : struct {
            if (Enum.TryParse(value, out TEnum result)) {
                return result;
            } else {
                Api.Services.Alert.Error($"Invalid value for {nameof(TEnum)}: {value}");
                // default return value
                return default;
            }
        }
    }
}
