using System;
using System.Collections.Generic;
using Csra.Interfaces;

namespace Csra.Services {
    /// <summary>
    /// StorageService - centralized persistent data storage.
    /// Uses type-specific dictionaries to avoid casting.
    /// Optimized for performance with O(1) lookups.
    /// </summary>
    [Serializable]
    public class Storage : IStorageService {

        private static IStorageService _instance;

        /// <summary>
        /// Static class to hold type-specific dictionaries without casting.
        /// </summary>
        private static class TypedStorage<T> {
            public static readonly Dictionary<string, T> Items = new Dictionary<string, T>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Stores removal actions per (type, key) to avoid reflection and casting.
        /// </summary>
        private readonly Dictionary<(Type type, string key), Action> _removeActions;

        /// <summary>
        /// Stores clear actions per type to avoid reflection and casting.
        /// </summary>
        private readonly Dictionary<Type, Action> _clearActions;

        /// <summary>
        /// Fast lookup for ContainsKey(string) - maps key to count of types storing it.
        /// </summary>
        private readonly Dictionary<string, int> _keyCount;

        protected Storage() {
            _removeActions = new Dictionary<(Type, string), Action>();
            _clearActions = new Dictionary<Type, Action>();
            _keyCount = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        public static IStorageService Instance => _instance ??= new Storage();

        public int Count => _removeActions.Count;

        public IEnumerable<string> Keys => _keyCount.Keys;

        public void AddOrUpdate<T>(string key, T value) {
            var typeKey = (typeof(T), key);
            bool isNewKey = !_removeActions.ContainsKey(typeKey);

            TypedStorage<T>.Items[key] = value;
            _removeActions[typeKey] = () => TypedStorage<T>.Items.Remove(key);
            _clearActions[typeof(T)] = () => TypedStorage<T>.Items.Clear();

            if (isNewKey) {
                if (_keyCount.TryGetValue(key, out int count)) {
                    _keyCount[key] = count + 1;
                } else {
                    _keyCount[key] = 1;
                }
            }
        }
        

        public bool ContainsKey<T>(string key) => TypedStorage<T>.Items.ContainsKey(key);
        

        public bool Remove<T>(string key) {
            var typeKey = (typeof(T), key);
            if (!TypedStorage<T>.Items.Remove(key)) return false;

            _removeActions.Remove(typeKey);

            if (_keyCount.TryGetValue(key, out int count)) {
                if (count <= 1) {
                    _keyCount.Remove(key);
                } else {
                    _keyCount[key] = count - 1;
                }
            }
            return true;
        }

        public void Reset() {
            foreach (var clearAction in _clearActions.Values) {
                clearAction();
            }
            _removeActions.Clear();
            _clearActions.Clear();
            _keyCount.Clear();
        }

        public bool TryGetValue<T>(string key, out T value) {
            return TypedStorage<T>.Items.TryGetValue(key, out value);
        }
    }
}
