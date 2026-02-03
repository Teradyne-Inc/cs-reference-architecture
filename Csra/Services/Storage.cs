using System;
using System.Collections.Generic;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra.Services {

    /// <summary>
    /// StorageService - centralized persistent data storage.
    /// </summary>
    [Serializable]
    public class Storage : IStorageService {

        private static IStorageService _instance = null;
        private readonly Dictionary<string, object> _storage;

        protected Storage() {
            _storage = [];
        }

        public static IStorageService Instance => _instance ??= new Storage();

        public int Count => _storage.Count;

        public IEnumerable<string> Keys => _storage.Keys;

        public void AddOrUpdate(string key, object value) => _storage[key] = value;
        public bool ContainsKey(string key) => _storage.ContainsKey(key);
        public bool Remove(string key) => _storage.Remove(key);
        public void Reset() => _storage.Clear();
        public bool TryGetValue<T>(string key, out T value) {
            if (_storage.TryGetValue(key, out object unboxedValue)) {
                if (unboxedValue is T typedValue) {
                    value = typedValue;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
