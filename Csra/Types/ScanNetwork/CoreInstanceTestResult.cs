using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store the test result of a core instance, part of <see cref="ScanNetworkPatternResults"/> of a ScanNetwork pattern(set).
    /// </summary>
    [Serializable]
    public class CoreInstanceTestResult : IEnumerable<string> {

        private List<string> _iclsInCore = [];

        #region Properties

        /// <summary>
        /// Gets the core instance name.
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// Gets or sets the per-site test result of the core instance. True = failed, False = passed.
        /// </summary>
        public Site<bool> IsFailed { get; set; }

        /// <summary>
        /// Result is valid if core instance is fully tested. i.e. all icl instances under this core instance are tested.
        /// </summary>
        public Site<bool> IsResultValid { get; set; } = new(false);

        /// <summary>
        /// Returns the per-site error status if any. 0 = no error.
        /// </summary>
        public Site<int> ErrorStatus { get; } = new(site => 0);

        /// <summary>
        /// Gets or sets the test number for datalogging the result of this core instance.
        /// </summary>
        public int? TestNumber { get; set; } = null;

        /// <summary>
        /// Gets or sets the test name for datalogging the result of this core instance.
        /// </summary>
        public string TestName { get; set; } = string.Empty;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="CoreInstanceTestResult"/> object for a core instance.
        /// </summary>
        /// <param name="coreInstanceName">The core instance for which this result is representing.</param>
        /// <param name="iclInstanceNames">The names of icl instances that belongs to this core instance.</param>
        public CoreInstanceTestResult(string coreInstanceName, IEnumerable<string> iclInstanceNames) {
            InstanceName = coreInstanceName;
            _iclsInCore.AddRange(iclInstanceNames);
        }

        /// <summary>
        /// Creates a new <see cref="CoreInstanceTestResult"/> by copying another instance. This can be useful when you want to create a new
        /// instance to hold updated results while keeping the original results unchanged.
        /// </summary>
        /// <param name="source">The source <see cref="CoreInstanceTestResult"/> to copy.</param>
        public CoreInstanceTestResult(CoreInstanceTestResult source) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            InstanceName = source.InstanceName;
            _iclsInCore.AddRange(source._iclsInCore);
            IsFailed = new Site<bool>(source.IsFailed);
            IsResultValid = new Site<bool>(source.IsResultValid);
            ErrorStatus = new Site<int>(source.ErrorStatus);
            TestNumber = source.TestNumber;
            TestName = source.TestName;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns an enumerator that iterates through the collection of strings.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator() => _iclsInCore.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _iclsInCore.GetEnumerator();

        /// <summary>
        /// Merges the test result data from the other core instance test result into the current instance.
        /// </summary>
        /// <remarks>After merging, the combined result will include all unique test data from both
        /// instances. The failure and validity flags are updated to reflect the merged state.</remarks>
        /// <param name="other">The core instance test result to merge with the current result. Must represent the same core instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> represents a different core instance than the current result.</exception>
        public void MergeWith(CoreInstanceTestResult other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (InstanceName != other.InstanceName) throw new ArgumentException("Cannot merge results from different core instances.", nameof(other));

            _iclsInCore.AddRange(other._iclsInCore);
            _iclsInCore = _iclsInCore.Distinct().ToList();

            IsFailed |= other.IsFailed;
            IsResultValid |= other.IsResultValid;           
        }
        #endregion
    }
}
