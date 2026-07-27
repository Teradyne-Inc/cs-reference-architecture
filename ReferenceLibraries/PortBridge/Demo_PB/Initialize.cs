using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.PortBridge;
using Teradyne.Igxl.Interfaces.Public;
using static System.Net.Mime.MediaTypeNames;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System.Reflection;
using PortBridgeUtils;
using static PortBridgeUtils.PBLib;
using System.Runtime.Remoting.Channels;
using Csra;
using static Csra.Api;


namespace Demo.PB {
    public partial class Initialize {
        /// <summary>
        /// Initializes the PortBridge framework and configures the logger to display information entries.
        /// Launches the PortBridge status monitor for real-time monitoring.
        /// </summary>
        public static void PortBridgeInit() {

            PortBridgeLanguage portBridge = new PortBridgeLanguage();
            // Initialize PortBridge
            portBridge.Initialize(TheHdw.Tester.Type);

            // Display Information Entries in the Log Tool
            portBridge.Utilities.Logger.ShowInformationEntries = true;

            //Launch status monitor
            portBridge.Utilities.ShowStatusMonitor();

        }

        /// <summary>
        /// Configures PortBridge by loading the register map from an SVD file and creating a test configuration.
        /// Unloads any existing register map, loads the demo.svd file, and associates it with the "RM1" test configuration.
        /// </summary>
        public static void PortBridgeSetup() {

            PBLib.Initialize("Demo");
            bool addOutput;
            addOutput = false;
            PBLib.PortBridge.RegisterMaps.Unload(REGISTER_MAP);

            if (PBLib.PortBridge.RegisterMaps.Contains(REGISTER_MAP) == false) {

                // Swith on PortBridge Debug Mode
                int month = Int32.Parse(s: DateTime.Now.ToString(format: "MM"));
                int day = Int32.Parse(s: DateTime.Now.ToString(format: "dd"));
                int year = Int32.Parse(s: DateTime.Now.ToString(format: "yyyy"));
                int password = month * year * (day * day);


                Services.Alert.Log("Run PortBridgeSetup in C#");

                //Load Register Map
                PBLib.PortBridge.RegisterMaps.Load(
                    REGISTER_MAP,                           // Name
                    ".\\Portbridge_Files\\demo.svd",        // FileNameAndPath
                    PortBridgeRegisterMapFormat.SVD,         // Format
                    string.Empty,                            // CustomParserPluginPath (null or empty for default parser)
                    false,                                   // FlattenRegisters
                    false,                                   // FlattenNamespace
                    string.Empty,                            // Filter (null or empty for no filter)
                    false);                                  // MergeWithExisting

                Services.Alert.Log("RegMap has been loaded");

                // Create a basic PortBridge test configuration
                IPortBridgeTestConfiguration pb = PBLib.PortBridge.Tests.Configurations["RM1"];

                // Associate the register map with the configuration
                pb.RegisterMap.Add(REGISTER_MAP);

                Services.Alert.Log("PortBridge configuration complete");

            }
        }

    }

}
