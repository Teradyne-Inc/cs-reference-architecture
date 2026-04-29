using System.Collections.Generic;

namespace Csra.Interfaces {

    public interface IStorageService : IServiceBase {

        /// <summary>
        /// Gets the number of key/value pairs contained in the storage.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a collection containing the keys in the storage.
        /// </summary>
        public IEnumerable<string> Keys { get; }
        
        
        /// <summary>
        /// Determines whether the storage contains the specified key for the given type.
        /// </summary>
        /// <typeparam name="T">The type of the value to check.</typeparam>
        /// <param name="key">The key to locate in the storage.</param>
        /// <returns><see langword="true"/> if the storage contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey<T>(string key); 

        /// <summary>
        /// Gets the type-safe value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this methods returns, contains the value associated with the specified key, if the key is found; otherwise, the default
        /// value for the type of the value parameter. This parameter is passed unintialized.</param>
        /// <returns><see langword="true"/> if the storage contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Adds a key/value pair to the storage if the key does not already exist, or updates the value for an existing key.
        /// </summary>
        /// <param name="key">They key to be added or whose value should be updated.</param>
        /// <param name="value">The value of the element to add, or update.</param>
        public void AddOrUpdate<T>(string key, T value);
       

        /// <summary>
        /// Removes the value with the specified key from the storage for the given type.
        /// </summary>
        /// <typeparam name="T">The type of the value to remove.</typeparam>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
        public bool Remove<T>(string key); 
    }
}
