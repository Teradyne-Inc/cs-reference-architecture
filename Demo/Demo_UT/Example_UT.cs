using MsTest = Microsoft.VisualStudio.TestTools.UnitTesting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Moq;
using Csra;
using Csra.Interfaces;
using static Csra.Api;

namespace UT {

    [MsTest.TestClass]
    public class Example_UT : Base {

        [MsTest.TestMethod]
        public void SiteIs_ReturnsCorrectValue() {
            Site<int> a = new(5);

            MsTest.Assert.AreEqual(5, a[0]);
        }

        [MsTest.DataTestMethod]
        [MsTest.DataRow(5, 2, 7)]
        [MsTest.DataRow(0, 5, 5)]
        [MsTest.DataRow(0, 0, 0)]
        [MsTest.DataRow(-10, 2, -8)]
        public void DataTestMethod_AddsSiteValues(int valueA, int valueB, int expected) {
            Site<int> a = new(valueA);
            Site<int> b = new(valueB);

            Site<int> c = a + b;

            MsTest.Assert.AreEqual(expected, c[0]);
        }

        [MsTest.TestMethod]
        public void MethodHasBeenCalled_VerifiesConnectCalled() {
            TheHdw.Pins("hello").DCVI.Connect();

            _mockTheHdw.Verify(x => x.Pins("hello").DCVI.Connect(tlDCVIConnectWhat.Default), Times.Once);
        }

        [MsTest.TestMethod]
        public void ValueHasBeenSet_VerifiesErrorOutputModeSet() {
            TheExec.ErrorOutputMode = tl_ErrorDest.DataTools;

            _mockTheExec.VerifySet(x => x.ErrorOutputMode = tl_ErrorDest.DataTools);
        }

        [MsTest.DataTestMethod]
        [MsTest.DataRow("hallo", "ThisString")]
        [MsTest.DataRow("bye", "CanBeAnything")]
        public void SetupReturnValue_ReturnsExpectedString(string pin, string returnString) {
            _mockTheHdw.Setup(x => x.DCVI.Pins(pin).Calibration.ToString()).Returns(returnString);

            string returnValue = TheHdw.DCVI.Pins(pin).Calibration.ToString();

            MsTest.Assert.AreEqual(returnString, returnValue);
        }

        [MsTest.TestMethod]
        public void SetupCsraMoq_AlertService_VerifiesErrorCalled() {
            Mock<IAlertService> alertService = new Mock<IAlertService>() { DefaultValue = DefaultValue.Mock };
            Services.Configure(alert: alertService.Object);
            PatternInfo patternInfo = new("TestPattern", false) {
                ClearFlags = 1860 // Set the invalid value
            };

            alertService.Verify(alert => alert.Error($"Value must be between 0 and '{PatternInfo.MaxFlags}' (inclusive).", 0,
                It.IsAny<string>()), Times.Once());
        }

        [MsTest.TestMethod]
        public void SetupCsraMoq_TheLib_VerifiesWaitCalled() {
            Mock<ILib> mockTheLib = new() { DefaultValue = DefaultValue.Mock };
            Configure(mockTheLib.Object);
            int waittime = 1860;

            TheLib.Execute.Wait(waittime);

            mockTheLib.Verify(lib => lib.Execute.Wait(waittime, It.IsAny<bool>(), It.IsAny<double>()), Times.Once());
        }

        [MsTest.TestMethod]
        public void FakePins_VerifiesConnectCalledForEachInstrument() {
            // Arrange            
            string[] pinsArray = ["dig", "dcvi", "dcvs"];
            InstrumentType[] typesArray = [InstrumentType.UP2200, InstrumentType.UVI264, InstrumentType.UVS256];
            Pins fakePins = CreateFakePins(pinsArray, typesArray, _mockTheHdw, _mockTheExec);

            // Act
            TheLib.Setup.Dc.Connect(fakePins);

            // Assert
            _mockTheHdw.Verify(hdw => hdw.PPMU.Pins(pinsArray[0]).Connect(), Times.Once);
            _mockTheHdw.Verify(hdw => hdw.DCVI.Pins(pinsArray[1]).Connect(tlDCVIConnectWhat.Default), Times.Once);
            _mockTheHdw.Verify(hdw => hdw.DCVS.Pins(pinsArray[2]).Connect(tlDCVSConnectWhat.Default), Times.Once);
        }
    }
}
