using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    [Serializable]
    public abstract class Factory {
        protected static IPpmuPins CreatePpmuPins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new PpmuPins(pinList);
        }
        protected static IPpmuPins CreatePpmuPins(string pinList, tlDriverPPMUPins tlDriverPpmuPins) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new PpmuPins(pinList, tlDriverPpmuPins);
        }
        protected static IDcviPins CreateDcviPins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DcviPins(pinList);
        }
        protected static IDcviPins CreateDcviPins(string pinList, DriverDCVIPins driverDcviPins) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DcviPins(pinList, driverDcviPins);
        }
        protected static IDcvsPins CreateDcvsPins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DcvsPins(pinList);
        }
        protected static IDcvsPins CreateDcvsPins(string pinList, DriverDCVSPins driverDcvsPins) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DcvsPins(pinList, driverDcvsPins);
        }
        protected static IDigitalPins CreateDigitalPins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DigitalPins(pinList);
        }
        protected static IDigitalPins CreateDigitalPins(string pinList, DriverDigitalPins driverDigitalPins) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DigitalPins(pinList, driverDigitalPins);
        }
        protected static IUtilityPins CreateUtilityPins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new UtilityPins(pinList);
        }
        protected static IUtilityPins CreateUtilityPins(string pinList, tlDriverUtilityPins driverUtilityPins) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new UtilityPins(pinList, driverUtilityPins);
        }
        protected static IDigitalCapture CreateCmemCapture(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new CmemCapture();
        }
        protected static IDigitalCapture CreateHramCapture(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new HramCapture();
        }
        protected static IDigitalCapture CreateDsscCapture(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            return new DsscCapture();
        }
    }
}
