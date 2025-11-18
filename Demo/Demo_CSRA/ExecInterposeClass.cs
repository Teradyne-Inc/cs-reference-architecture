using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Csra.Api;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CSRA {
    /// <summary>
    /// This class contains empty Exec Interpose functions.
    /// </summary>
    /// <remarks>
    /// <para>They are here for convenience and are completely optional.</para>
    /// <para>It is not necessary to delete them if they are not being used, nor is it
    /// necessary that they exist in the program.</para>
    /// </remarks>
    [TestClass]
    public class ExecInterposeClass : TestCodeBase {
        /// <summary>
        /// Immediately after the test program has been loaded successfully.
        /// </summary>
        [ExecInterpose_OnProgramLoaded(255)]
        public static void OnProgramLoaded() {
            // Put code here
            TheHdw.Digital.EnablePinRespecification = true;

            // Temporary fix for 11.00 and 11.10, until we move to native SiteGenerics solution
            var pld = new PinListData();
            pld.GlobalSort = false;

        }

        /// <summary>
        /// Immediately at the beginning of the validation process.
        /// </summary>
        [ExecInterpose_OnValidationStart(255)]
        public static void OnValidationStart() {
            // Put code here

        }

        /// <summary>
        /// Immediately at the conclusion of the validation process. Called only if validation succeeds.
        /// </summary>
        [ExecInterpose_OnProgramValidated(255)]
        public static void OnProgramValidated() {
            // Put code here
            //Setup initLeakage = new Setup("InitLeakageTest");
            //initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, new List<string> { "nLEAB","nOEAB" }));
            //initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, new List<string> { "nLEBA","nOEBA" }));
            //initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Off, new List<string> { "porta" }));
            //Services.Setup.Add(initLeakage);

            //Setup initIccStatic = new Setup("InitIccStaticTest");
            //initIccStatic.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, new List<string> { "nOEBA" }));
            //initIccStatic.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, new List<string> { "nLEAB","nLEBA","nOEAB","porta" }));
            //Services.Setup.Add(initIccStatic);
        }

        /// <summary>
        /// Immediately at the conclusion of the validation process. Called only if validation fails.
        /// </summary>
        [ExecInterpose_OnProgramFailedValidation(255)]
        public static void OnProgramFailedValidation() {
            // Put code here

        }

        /// <summary>
        /// Immediately after "pre-job reset" before each run of the test program starts.
        /// </summary>
        /// <remarks>
        /// <para>Note that "first run" actions can be enclosed in:</para>
        /// <para><code>    if (TheExec.ExecutionCount == 0) ...</code></para>
        /// </remarks>
        [ExecInterpose_OnProgramStarted(255)]
        public static void OnProgramStarted() {
            // Put code here
            Services.Behavior.SetFeature("Datalog.Parametric.OfflinePassResults", true);
        }

        /// <summary>
        /// Immediately after a site binning has been determined, before it is disconnected.
        /// </summary>
        /// <remarks>
        /// Use <c>TheExec.Sites.SiteNumber</c> to determine which site is being disconnected.
        /// </remarks>
        [ExecInterpose_OnPreShutDownSite(255)]
        public static void OnPreShutDownSite() {
            // Put code here

        }

        /// <summary>
        /// Immediately after a site is disconnected.
        /// </summary>
        /// <remarks>
        /// Use <c>TheExec.Sites.SiteNumber</c> to determine which site is being disconnected.
        /// </remarks>
        [ExecInterpose_OnPostShutDownSite(255)]
        public static void OnPostShutDownSite() {
            // Put code here

        }

        /// <summary>
        /// Immediately after an alarm is detected, before it is reported.
        /// </summary>
        /// <param name="alarmList">A tab-delimited string of alarm error messages.</param>
        [ExecInterpose_OnAlarmOccurred(255)]
        public static void OnAlarmOccurred(string alarmList) {
            // Put code here

        }

        /// <summary>
        /// Immediately after the flow has ended, before binning.
        /// </summary>
        [ExecInterpose_OnFlowEnded(255)]
        public static void OnFlowEnded() {
            // Put code here

        }

        /// <summary>
        /// Immediately after the test program has completed, before "post-job reset".
        /// </summary>
        /// <remarks>
        /// Note that any actions taken here with respect to modification of binning
        /// will affect the binning sent to the Operator Interface, but will not affect
        /// the binning reported in Datalog.
        /// </remarks>
        [ExecInterpose_OnProgramEnded(255)]
        public static void OnProgramEnded() {
            // Put code here

        }
    }
}
