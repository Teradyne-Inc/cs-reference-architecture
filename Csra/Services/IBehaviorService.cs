using System.Collections.Generic;

namespace Csra.Interfaces {

    public interface IBehaviorService : IService {

        /// <summary>
        /// Gets/Sets the import / export file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets a collection containing all defined features.
        /// </summary>
        public IEnumerable<string> Features { get; }

        /// <summary>
        /// Defines a feature's value. Creates a new entry, or updates an existing one if it already exists.
        /// </summary>
        /// <typeparam name="T">The feature value's type.</typeparam>
        /// <param name="feature">The feature name.</param>
        /// <param name="value">The feature's new value.</param>
        public void SetFeature<T>(string feature, T value);

        /// <summary>
        /// Reads a feature's value. Type must match the original definition, an exception is thrown otherwise.
        /// </summary>
        /// <typeparam name="T">The feature value's type.</typeparam>
        /// <param name="feature">The feature name.</param>
        /// <returns>The feature's value.</returns>
        public T GetFeature<T>(string feature);

        /// <summary>
        /// Writes all features to the specified file, or a previously defined <see cref="FilePath"/> if empty. Updates the <see cref="FilePath"/> setting.
        /// </summary>
        /// <param name="filePath">Optional. The (relative or absolute) file path.</param>
        public void Export(string filePath = "");

        /// <summary>
        /// Reads the specified file, or a previously defined <see cref="FilePath"/> if empty. Incrementally updates features with new values.
        /// Call <c>Services.Behavior.Reset()</c> to clear all data first. Updates the <see cref="FilePath"/> setting.
        /// </summary>
        /// <param name="filePath">Optional. The (relative or absolute) file path.</param>
        public void Import(string filePath = "");
    }
}
