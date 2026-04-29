using Teradyne.Igxl.Interfaces.Public;

namespace Tol {
    public class DsscCapture : IDigitalCapture {
        public PinSite<int[]> FailingCycleData => throw new System.NotImplementedException();

        public PinSite<double[]> DeviceData => throw new System.NotImplementedException();

        public IDigitalPins Pins => throw new System.NotImplementedException();

        internal DsscCapture() {
        }

        public void Configure() {
            throw new System.NotImplementedException();
        }

        public void Reset() {
            throw new System.NotImplementedException();
        }

        public void Read() {
            throw new System.NotImplementedException();
        }
    }
}
