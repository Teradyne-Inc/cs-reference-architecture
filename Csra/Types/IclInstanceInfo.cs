using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store the attributes of an icl instance, part of <see cref="ScanNetworkPatternInfo"/> for a ScanNetwork pattern(set).
    /// </summary>
    [Serializable]
    public class IclInstanceInfo {

        #region Properties

        /// <summary>
        /// Gets the name of the ssh instance.
        /// </summary>
        public string SshInstanceName { get; private set; }

        /// <summary>
        /// Gets the name of the icl instance.
        /// </summary>
        public string IclInstanceName { get; private set; }

        /// <summary>
        /// Gets the name of the core instance that this icl instance belongs to.
        /// </summary>
        public string CoreInstanceName { get; private set; }

        /// <summary>
        /// Gets the capture-global-group ID that this icl instance belongs to.
        /// </summary>
        public string GlobalGroupID { get; private set; }

        /// <summary>
        /// Whether OnChipCompare is enabled on this icl instance. <see langword="false"/> means this icl needs TesterCompare.
        /// </summary>
        public bool IsOnChipCompare { get; private set; }

        /// <summary>
        /// Gets the PIN for the disable-contribution-bits in the ssn_setup pattern. Only valid if OnChipCompare is true.
        /// </summary>
        public string ContribPin { get; private set; }

        /// <summary>
        /// Gets the label for the disable-contribution-bit in the ssn_setup pattern. Only valid if OnChipCompare is true.
        /// </summary>
        public string ContribLabel { get; private set; }

        /// <summary>
        /// Gets the offset of the disable-contribution-bit relative to the contribution label. Only valid if OnChipCompare is true.
        /// </summary>
        public int? ContribOffset { get; private set; }

        /// <summary>
        /// Gets the PIN for the sticky-bit in the ssn_end pattern. Only valid if OnChipCompare is true.
        /// </summary>
        public string StickyPin { get; private set; }

        /// <summary>
        /// Gets the absolute cycle (module cycle) of the sticky-bit in the ssn_end pattern. Only valid if OnChipCompare is true.
        /// </summary>
        public double? StickyCycle { get; private set; }

        /// <summary>
        /// Substring of the disable-contribution-bit to be patched. Only used when OnChipCompare is true.
        /// </summary>
        public Site<string> ModifyVectorData { get; private set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Creating a new <see cref="IclInstanceInfo"/> object which is a member of a collection in <see cref="ScanNetworkPatternInfo"/>.
        /// </summary>
        /// <param name="sshInstanceName">The name of the ssh-instance, <c>representative_ssh</c> use this instance name as reference.</param>
        /// <param name="sshIclInstanceName">The name of the ssh-icl-instance, ssn mapping section use this name as reference.</param>
        /// <param name="coreInstanceName">The name of the core-instance that this ssh-icl-instance is associated with.</param>
        /// <param name="isOnChipCompare">Optional. The attribute that indicates if OnChipCompare is enabled for this instance.<br/>
        ///                               <c>true</c>: OnChipCompare = on; <c>false</c>: OnChipCompare = off;</param>
        /// <param name="tckRatio">Optional. The speed ratio between the Scan Cycles over the Jtag Cycles.</param>
        /// <param name="contribPin">Optional. Pin name for modifying the <c>disable_contribution_bit</c>.</param>
        /// <param name="contribLabel">Optional. Pattern label for locating the <c>disable_contribution_bit</c>.</param>
        /// <param name="contribOffset">Optional. Offset of the <c>disable_contribution_bit</c> relative to the <c>ContribLabel</c>.</param>
        /// <param name="stickyPin">Optional. Pin name for retrieving the <c>sticky_bit</c> status.</param>
        /// <param name="stickyLabel">Optional. Label for locating the <c>sticky_bit</c>.</param>
        /// <param name="stickyOffset">Optional. Offset of the <c>sticky_bit</c> relative to the <c>stickyLabel</c>.</param>
        /// <param name="stickyCycle">Optional. Absolute cycle of the <c>sticky_bit</c> in the <c>ssn_end_pattern</c>.</param>
        /// <param name="globalGroupID">Optional. Capture Global Group ID for this ssh-icl-instance.</param>
        /// <param name="representativeSsh">Optional. The name of the ssh-instance that represent this instance.</param>
        /// <remarks>Users typically do not need to create new icl instances by calling this constructor manually.
        /// Instead, please use the <see cref="ScanNetworkPatternInfo"/> to create and manage all icl instances.</remarks>
        public IclInstanceInfo(string sshInstanceName, string sshIclInstanceName, string coreInstanceName, bool isOnChipCompare = false, int? tckRatio = null,
            string contribPin = null, string contribLabel = null, int? contribOffset = null, string stickyPin = null, string stickyLabel = null,
            int? stickyOffset = null, double? stickyCycle = null, string globalGroupID = null, string representativeSsh = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creating a new <see cref="IclInstanceInfo"/> object from the combined instance name: $"{ssh-icl-instance}@{core-instance}".
        /// </summary>
        /// <param name="iclAndCoreInstanceNames">The hybrid instance name following this format: "{ssh_icl_instance}@{core_instance}".</param>
        /// <remarks>This hybrid instance name is retrieved from IGXL API: TheHdw.Digital.ScanNetworks[ScanNetworkMapping].InstanceNames.</remarks>
        public IclInstanceInfo(string iclAndCoreInstanceNames) : this(sshInstanceName: "", sshIclInstanceName: "", coreInstanceName: "") {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sets or clears the disable-contribution-bit for a specified site.
        /// </summary>
        /// <param name="site">The identifier of the site for which the disable contribution bit is being modified.</param>
        /// <param name="value">The new state of the disable-contribution-bit.</param>
        public void SetDisableContributionBit(int site, char value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets or clears the disable-contribution-bit for each site.
        /// </summary>
        /// <param name="value">The new states of the disable-contribution-bit for each site.</param>
        public void SetDisableContributionBit(Site<char> value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets or clears the disable-contribution-bit for all sites.
        /// </summary>
        /// <param name="value">The new state of the disable-contribution-bit that applies to all sites.</param>
        public void SetDisableContributionBit(char value) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
