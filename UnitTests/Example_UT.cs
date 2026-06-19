using System;
using MsTest = Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using Csra.Interfaces;
using static Csra.Api;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace UnitTestExample {

    [MsTest.TestClass]
    public class Example_UT : Base {

        /// <summary>
        /// Assert: "checks certain states/objects" (Assert.AreEqual, IsTrue/False)
        /// </summary>
        #region Process_ReturnsSum_StateAssertion
        [MsTest.TestMethod]
        public void Process_ReturnsSum_StateAssertion() {
            // Arrange
            int a = 5;
            int b = 7;
            // Act
            int c = a + b;
            // Assert
            MsTest.Assert.AreEqual(12, c);
            MsTest.Assert.IsTrue(c > 0);
            MsTest.Assert.IsFalse(c == 0);
        }
        #endregion

        /// <summary>
        /// SiteGenerics: "checks certain states/objects" (Assert.AreEqual)
        /// </summary>
        #region Process_IgXlObject_StateAssertion
        [MsTest.TestMethod]
        public void Process_IgXlObject_StateAssertion() {
            // Arrange
            Site<int> a = new(5);
            Site<int> b = new(7);
            // Act
            Site<int> c = a + b;
            // Assert
            MsTest.Assert.AreEqual(new Site<int>(12), c);
        }
        #endregion

        /// <summary>
        /// DataTestMethod: "parametrized test method example"
        /// </summary>
        /// <param name="a">First integer</param>
        /// <param name="b">Second integer</param>
        /// <param name="expected">Expected result</param>
        #region Process_ReturnsSum_Parametrized
        [MsTest.DataTestMethod]
        [MsTest.DataRow(10, 15, 25)]
        [MsTest.DataRow(-5, 5, 0)]
        [MsTest.DataRow(0, 0, 0)]
        public void Process_ReturnsSum_Parametrized(int a, int b, int expected) {
            // Act
            int c = a + b;
            // Assert
            MsTest.Assert.AreEqual(expected, c);
        }
        #endregion

        /// <summary>
        /// Setup: "setup return values that would have been otherwise default"
        /// </summary>
        #region Process_ReadFromDependency_SetupReturnValue
        [MsTest.TestMethod]
        public void Process_ReadFromDependency_SetupReturnValue() {
            // Arrange
            string pinName = "VDD";
            CreateFakePins([pinName], [InstrumentType.UVI264], _mockTheHdw, _mockTheExec); // Create fake pins for the test
            PinSite<double> returnValue = new(pinName, 3.3);
            _mockTheHdw.Setup(hdw => hdw.DCVI.Pins(pinName).Meter.Read(It.IsAny<tlStrobeOption>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<tlDCVIMeterReadingFormat>()))
                .Returns(returnValue.ToPinListData());
            // Act
            PinSite<double> actualValue = TheLib.Acquire.Dc.Measure(new Pins(pinName), DcMeterMode.Voltage); // Call the method that uses the dependency
            // Assert
            MsTest.Assert.AreEqual(returnValue, actualValue); // Verify the result
        }
        #endregion

        /// <summary>
        /// Verify: "checks if specific method was called"
        /// </summary>
        #region Process_ForceV_VerifyMethodCall
        [MsTest.TestMethod]
        public void Process_ForceV_VerifyMethodCall() {
            // Arrange
            string pinName = "PORTB";
            CreateFakePins([pinName], [InstrumentType.UP2200], _mockTheHdw, _mockTheExec); // Create fake pins for the test
            double voltageToSet = 3.3;
            // Act
            TheLib.Setup.Dc.ForceV(new Pins(pinName), voltageToSet); // Call the method that uses the dependency
            // Assert
            _mockTheHdw.Verify(hdw => hdw.PPMU.Pins(pinName).ForceV(voltageToSet), Times.Once); // Verify that the method was called once with the expected parameter
        }
        #endregion

        /// <summary>
        /// VerifySet: "checks if specific property was set"
        /// </summary>
        #region Process_ForceV_PropertyWasSet
        [MsTest.TestMethod]
        public void Process_ForceV_PropertyWasSet() {
            // Arrange
            string pinName = "VDD";
            CreateFakePins([pinName], [InstrumentType.UVI264], _mockTheHdw, _mockTheExec); // Create fake pins for the test
            double voltageToSet = 3.3;
            // Act
            TheLib.Setup.Dc.ForceV(new Pins(pinName), voltageToSet); // Call the method that uses the dependency
            // Assert
            _mockTheHdw.VerifySet(hdw => hdw.DCVI.Pins(pinName).Voltage.Value = voltageToSet, Times.Once); // Verify that the method was called once with the expected parameter
        }
        #endregion

        /// <summary>
        /// Times.Never: a very important blueprint for "should NOT happen"
        /// </summary>
        #region Process_ValidInput_TimesNever
        [MsTest.TestMethod]
        public void Process_ValidInput_TimesNever() {
            // Arrange
            Mock<IAlertService> mockAlertService = new() { DefaultValue = DefaultValue.Mock };
            Services.Configure(mockAlertService.Object);
            // Act
            TheLib.Validate.InRange(7, 5, 10, ""); // 7 is within the range of 5 to 10
            // Assert
            mockAlertService.Verify(alert => alert.Error(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        /// <summary>
        /// ThrowsException: "checks if specific exception has been thrown"
        /// </summary>
        #region Process_DivideByZero_ThrowsException
        [MsTest.TestMethod]
        public void Process_DivideByZero_ThrowsException() {
            // Arrange
            int a = 5;
            int b = 0;
            // Act && Assert
            MsTest.Assert.ThrowsException<DivideByZeroException>(() => { int c = a / b; });
        }
        #endregion

        /// <summary>
        /// Callback: "overwrite framework messages by routing alerts to custom handlers via Callback"
        /// </summary>
        #region Process_AlertMessage_OverwrittenWithCallback
        [MsTest.TestMethod]
        public void Process_AlertMessage_OverwrittenWithCallback() {
            // Arrange
            Mock<IAlertService> mockAlert = new() { DefaultValue = DefaultValue.Mock };
            mockAlert.Setup(a => a.Error(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Callback<string, int, string>((msg, _, __) => System.Diagnostics.Trace.WriteLine($"Alert.Error: {msg}"));
            mockAlert.Setup(a => a.Warning(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((msg, _) => System.Diagnostics.Trace.WriteLine($"Alert.Warning: {msg}"));
            mockAlert.Setup(a => a.Log(It.IsAny<string>(), It.IsAny<ColorConstants>(), It.IsAny<bool>()))
                .Callback<string, ColorConstants, bool>((msg, _, __) => System.Diagnostics.Trace.WriteLine($"Alert.Log: {msg}"));
            Services.Configure(mockAlert.Object);
            // Act
            TheLib.Validate.InRange(15, 5, 10, ""); // 15 is out of range 5–10, triggers Alert.Error
            // Assert
            mockAlert.Verify(a => a.Error(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }
        #endregion
    }
}
