using System;
using System.Collections.Generic;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    public class Custom<T> : SettingBase<T> {

        public Custom(T value, Action<T> setAction) {
            if (setAction is null) Api.Services.Alert.Error("The setAction parameter was not provided.");
            Action<string, T> convAction = (s1, s2) => setAction(s2);
            SetArguments(value, null);
            SetBehavior(default, "", InitMode.Creation, false);
            SetContext(convAction, null, null);
            if (TheExec.JobIsValid) Validate();
        }

        private static readonly Dictionary<string, T> _staticCache = [];

        public Custom(string key, T value, Action<T> setAction, T initValue, InitMode initMode) {
            if (string.IsNullOrWhiteSpace(key)) Api.Services.Alert.Error("Invalid Key: The key cannot be null or empty.");
            if (setAction is null) Api.Services.Alert.Error("The setAction parameter was not provided.");
            Action<string, T> convAction = (s1, s2) => setAction(s2);
            SetArguments(value, key);
            SetBehavior(initValue, "", initMode, false);
            SetContext(convAction, null, _staticCache);
        }

        public Custom(string key, T value, Action<T> setAction, Func<T[]> readFunc, T initValue, InitMode initMode) {
            if (string.IsNullOrWhiteSpace(key)) Api.Services.Alert.Error("Invalid Key: The key cannot be null or empty.");
            if (setAction is null) Api.Services.Alert.Error("The setAction parameter was not provided.");
            if (readFunc is null) Api.Services.Alert.Error("The readFunc parameter was not provided.");
            Action<string, T> convAction = (s1, s2) => setAction(s2);
            Func<string, T[]> convFunc = readFunc is not null ? (s1) => readFunc() : (s1) => new T[TestCodeBase.TheExec.Sites.Selected.Count];
            SetArguments(value, key);
            SetBehavior(initValue, "", initMode, false);
            SetContext(convAction, convFunc, _staticCache);
        }
        public static void SetCache(T value, string key) => SetCacheInternal(value, key, _staticCache);
    }
}
