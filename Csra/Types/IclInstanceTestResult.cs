using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store the metadata and test result of an icl instance, part of <see cref="ScanNetworkPatternResults"/> of a ScanNetwork pattern(set).
    /// </summary>
    [Serializable]
    public class IclInstanceTestResult {

        #region Properties
        /// <summary>
        /// Gets the icl instance name.
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// Gets the core instance name that this icl instance belongs to.
        /// </summary>
        public string CoreInstanceName { get; private set; }

        /// <summary>
        /// Returns true if this icl instance is an on-chip compare instance.
        /// </summary>
        public bool IsOnChipCompare { get; private set; }

        /// <summary>
        /// Gets or sets the per-site test result of the icl instance. True = failed, False = passed.
        /// </summary>
        public Site<bool> IsFailed { get; set; }

        /// <summary>
        /// Result is valid if the icl instance is tested. i.e. not masked or set to disable-contribution.
        /// </summary>
        public Site<bool> IsResultValid { get; set; } = new(false);

        /// <summary>
        /// Returns the per-site error status if any. 0 = no error.
        /// </summary>
        public Site<int> ErrorStatus { get; } = new(site => 0);

        /// <summary>
        /// Gets or sets the test number for datalogging the result of this icl instance.
        /// </summary>
        public int? TestNumber { get; set; } = null;

        /// <summary>
        /// Gets or sets the test name for datalogging the result of this icl instance.
        /// </summary>
        public string TestName { get; set; } = string.Empty;
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new <see cref="IclInstanceTestResult"/> for an icl instance.
        /// </summary>
        /// <param name="iclInstance">The icl instance for which this result is representing.</param>
        public IclInstanceTestResult(IclInstanceInfo iclInstance) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
