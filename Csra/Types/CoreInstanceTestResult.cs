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
        #endregion

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
    }
}
