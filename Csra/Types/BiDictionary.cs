using System;
using System.Collections;
using System.Collections.Generic;

namespace Csra {

    /// <summary>
    /// Represents a bidirectional dictionary that allows lookups in both directions.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    [Serializable]
    public class BiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {

        private readonly Dictionary<TKey, TValue> _forward = [];
        private readonly Dictionary<TValue, TKey> _reverse = [];

        /// <summary>
        /// Gets the number of key/value pairs contained in the BiDictionary.
        /// </summary>
        public int Count => _forward.Count;

        /// <summary>
        /// Gets a collection containing the keys in the BiDictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _forward.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines wheter the BiDictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the BiDictionary.</param>
        /// <returns><see langword="true"/> if the BiDictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

        /// <summary>
        /// Determines wheter the BiDictionary contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the BiDictionary.</param>
        /// <returns><see langword="true"/> if the BiDictionary contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
        public bool ContainsValue(TValue value) => _reverse.ContainsKey(value);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this methods returns, contains the value associated with the specified key, if the key is found; otherwise, false.</param>
        /// <returns><see langword="true"/> if the BiDictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value) => _forward.TryGetValue(key, out value);

        /// <summary>
        /// Gets the key associated with the specified value.
        /// </summary>
        /// <param name="value">The value of the key to get.</param>
        /// <param name="key">When this methods returns, contains the key associated with the specified value, if the value is found; otherwise, false.</param>
        /// <returns><see langword="true"/> if the BiDictionary contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
        public bool TryGetKey(TValue value, out TKey key) => _reverse.TryGetValue(value, out key);

        /// <summary>
        /// Adds a key/value pair to the BiDictionary if the key does not already exist.
        /// </summary>
        /// <param name="key">They key to be added.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Add(TKey key, TValue value) {
            if (_forward.ContainsKey(key) || _reverse.ContainsKey(value)) Api.Services.Alert.Error("Key or value already exists.");
            _forward[key] = value;
            _reverse[value] = key;
        }

        /// <summary>
        /// Removes the key/value pair with the specified key from the BiDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
        public bool RemoveByKey(TKey key) {
            if (_forward.TryGetValue(key, out TValue value)) {
                _forward.Remove(key);
                _reverse.Remove(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the key/value pair with the specified value from the BiDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
        public bool RemoveByValue(TValue value) {
            if (_reverse.TryGetValue(value, out TKey key)) {
                _reverse.Remove(value);
                _forward.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all keys and values from the BiDictionary.
        /// </summary>
        public void Clear() {
            _forward.Clear();
            _reverse.Clear();
        }
    }
}
