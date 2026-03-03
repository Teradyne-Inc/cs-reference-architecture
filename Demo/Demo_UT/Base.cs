using MsTest = Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using Moq;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using IGXLFakes;

namespace Demo_UT {

    /// <exclude />
    [MsTest.TestClass]
    public class Base {

        /// <summary>
        /// Represents a mock object for the <see cref="IHdwL"/> interface used in testing scenarios.
        /// </summary>
        /// <remarks>This protected field is intended for use in unit tests to simulate the behavior of
        /// the <see cref="IHdwL"/> interface. It allows for the setup of expected calls and verification of
        /// interactions with the hardware layer.</remarks>
        protected Mock<IHdwL> _mockTheHdw;

        /// <summary>
        /// Represents a mock object for the <see cref="IExecL"/> interface used in testing scenarios.
        /// </summary>
        /// <remarks>This field is intended for use in unit tests to simulate and verify interactions with
        /// the <see cref="IExecL"/> interface. It allows for setting up expectations and verifying that the expected
        /// methods are called with the correct parameters.</remarks>
        protected Mock<IExecL> _mockTheExec;

        /// <summary>
        /// Represents a mock object for the <see cref="IProgramL"/> interface.
        /// </summary>
        /// <remarks>This field is used to simulate the behavior of the <see cref="IProgramL"/> interface
        /// in unit tests. It allows for the verification of interactions and the setup of expected behaviors.</remarks>
        protected Mock<IProgramL> _mockTheProgram;

        /// <summary>
        /// Initializes the test assembly before any tests are run.
        /// </summary>
        /// <param name="context">The test context provided by the MSTest framework, containing information about the test environment.</param>
        [MsTest.AssemblyInitialize]
        public static void AssemblyInit(MsTest.TestContext context) {
            TestHarness.SetupTestHarnessUtilityFactory();
        }

        /// <summary>
        /// Initializes the test environment by setting up mock objects and resetting services.
        /// </summary>
        [MsTest.TestInitialize]
        public void TestInit() {
            _mockTheHdw = new Mock<IHdwL>() { DefaultValue = DefaultValue.Mock };
            _mockTheExec = new Mock<IExecL>() { DefaultValue = DefaultValue.Mock };
            _mockTheProgram = new Mock<IProgramL>() { DefaultValue = DefaultValue.Mock };

            Api.Services.Alert.Reset();
            Api.Services.Setup.Reset();
            Api.Services.Storage.Reset();
            Api.Services.Behavior.Reset();
            Api.Services.Transaction.Reset();

            TestHarness.SiteCount = 2;
            TestHarness.SetupTestHarnessMoq(_mockTheHdw, _mockTheExec, null, null, _mockTheProgram);

            Api.Configure();
        }

        /// <summary>
        /// Setup the Mock calls for the IHdwL and IExcecL interfaces to create a FakePins for testing purposes. 
        /// </summary>
        /// <param name="pinArray">An array of pin names.</param>
        /// <param name="types">An array of pin types corresponding to the pin names.</param>
        /// <param name="mockTheHdw">A mock object for the IHdwL interface.</param>
        /// <param name="mockTheExec">A mock object for the IExcecL interface.</param>
        /// <returns>A comma-separated string of pin names.</returns>
        /// <exception cref="ArgumentException">Thrown when the lengths of pinArray and types do not match.</exception>
        public static Pins CreateFakePins(string[] pinArray, InstrumentType[] types, Mock<IHdwL> mockTheHdw, Mock<IExecL> mockTheExec) {
            if (pinArray.Length != types.Length) throw new ArgumentException("Arguments does not match length.");
            pinArray = pinArray.Distinct().ToArray();
            int count = pinArray.Length;
            string pinList = string.Join(", ", pinArray);
            mockTheExec.Setup(x => x.DataManager.DecomposePinList(pinList, out pinArray, out count)).Returns(0);
            for (int i = 0; i < pinArray.Length; i++) {
                string[] chanTypes = { "nonsense" };
                int countPins = pinArray[i].Split(',').Length;
                int defaultCount = 1;
                string[] defaultPin = { pinArray[i] };
                mockTheExec.Setup(x => x.DataManager.DecomposePinList(pinArray[i], out defaultPin, out defaultCount)).Returns(0);
                mockTheExec.Setup(x => x.DataManager.GetChannelTypes(pinArray[i], out countPins, out chanTypes));
                mockTheHdw.Setup(x => x.ChanFromPinSite(pinArray[i], It.IsAny<int>(), It.IsAny<string>())).Returns($"{i}.x");
                mockTheHdw.Setup(x => x.Config.Slots[i].Type).Returns(Pins.Pin.GetInstrumentName(types[i]));
            }
            return new Pins(pinList);
        }

        public static Pins CreateFakePins(string csvPins, string csvTypes, Mock<IHdwL> mockTheHdw, Mock<IExecL> mockTheExec) {
            if (csvPins == string.Empty) return new Pins(string.Empty);
            var pins = csvPins.Split(',').Select(p => p.Trim()).ToArray();
            var stringTypes = csvTypes.Split(',').Select(t => t.Trim()).ToArray();
            InstrumentType[] types = new InstrumentType[pins.Length];
            for (int i = 0; i < pins.Length; i++) {
                if (Enum.TryParse(stringTypes[i], out InstrumentType type)) types[i] = type;
                else throw new Exception($"Could not parse string '{stringTypes[i]}' to an enum of '{nameof(InstrumentType)}'.");
            }
            return CreateFakePins(pins, types, mockTheHdw, mockTheExec);
        }

        public static PatternInfo CreateFakePatternInfo(Pattern pattern, bool threadingEnabled, string timeDomains, Mock<IHdwL> mockTheHdw) {
            mockTheHdw.Setup(hdw => hdw.Patterns(pattern).Threading.Enable).Returns(threadingEnabled);
            mockTheHdw.Setup(hdw => hdw.Patterns(pattern).TimeDomains).Returns(timeDomains);
            return new PatternInfo(pattern, threadingEnabled);
        }

        public static void FakeDecomposePinList(string pinList, Mock<IExecL> mockTheExec, int success = 0) {
            if (pinList == string.Empty) return;
            var pinArray = pinList.Split(',').Select(p => p.Trim()).ToArray();
            int count = pinArray.Length;
            mockTheExec.Setup(x => x.DataManager.DecomposePinList(pinList, out pinArray, out count)).Returns(success);
        }

        public static void FakeDCVIType(string pinList, string dcviType, Mock<IHdwL> mockTheExec) {
            // ToDo -> implement to be more flexible to return different types
            if (pinList == string.Empty) return;
            var pinArray = pinList.Split(',').Select(p => p.Trim()).ToArray();
            //var dcviTypeArray = dcviTypes.Split(',').Select(p => p.Trim()).ToArray();
            for (int i = 0; i < pinArray.Length; i++) {
                mockTheExec.Setup(x => x.DCVI.Pins(pinArray[i]).DCVIType).Returns(dcviType);
            }
            mockTheExec.Setup(x => x.DCVI.Pins(string.Join(", ", pinArray)).DCVIType).Returns(dcviType);
        }

        public static void FakeDCVSType(string pinList, string dcvsType, Mock<IHdwL> mockTheExec) {
            // ToDo -> implement to be more flexible to return different types
            if (pinList == string.Empty) return;
            string[] pinArray = pinList.Split(',').Select(p => p.Trim()).ToArray();
            //var dcviTypeArray = dcviTypes.Split(',').Select(p => p.Trim()).ToArray();
            for (int i = 0; i < pinArray.Length; i++) {
                mockTheExec.Setup(x => x.DCVS.Pins(pinArray[i]).DCVSType).Returns(dcvsType);
            }
            mockTheExec.Setup(x => x.DCVS.Pins(string.Join(", ", pinArray)).DCVSType).Returns(dcvsType);
        }
    }
}
