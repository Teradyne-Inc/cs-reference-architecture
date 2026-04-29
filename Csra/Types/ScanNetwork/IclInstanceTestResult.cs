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
            InstanceName = iclInstance.IclInstanceName;
            CoreInstanceName = iclInstance.CoreInstanceName;
            IsOnChipCompare = iclInstance.IsOnChipCompare;
        }

        /// <summary>
        /// Create a new instance of <see cref="IclInstanceTestResult"/> by copying another instance. This can be useful when you want to create a new
        /// instance to hold updated results while keeping the original results unchanged.
        /// </summary>
        /// <param name="source">The source <see cref="IclInstanceTestResult"/> to copy.</param>
        public IclInstanceTestResult(IclInstanceTestResult source) {
            InstanceName = source.InstanceName;
            CoreInstanceName = source.CoreInstanceName;
            IsOnChipCompare = source.IsOnChipCompare;
            IsFailed = new Site<bool>(source.IsFailed);
            IsResultValid = new Site<bool>(source.IsResultValid);
            ErrorStatus = new Site<int>(source.ErrorStatus);
            TestNumber = source.TestNumber;
            TestName = source.TestName;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Merges the test result data from the other icl instance into the current icl instance, combining relevant status
        /// flags.
        /// </summary>
        /// <remarks>This method updates the current instance's failure and validity status by combining
        /// them with those from the specified instance. Only results from the same ICL instance can be
        /// merged.</remarks>
        /// <param name="other">The test result instance to merge with the current instance. Must represent the same ICL instance as the
        /// current object.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> represents a different ICL instance than the current object.</exception>
        public void MergeWith(IclInstanceTestResult other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (InstanceName != other.InstanceName) throw new ArgumentException("Cannot merge results of different icl instances.");

            IsFailed |= other.IsFailed;
            IsResultValid |= other.IsResultValid;
        }
        #endregion
    }
}
