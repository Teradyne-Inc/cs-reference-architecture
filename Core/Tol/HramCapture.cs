using Teradyne.Igxl.Interfaces.Public;

namespace Tol {
    public class HramCapture : IDigitalCapture {
        public IDigitalPins Pins => throw new System.NotImplementedException();

        public HramCapture() {
        }

        public void Reset() {
            throw new System.NotImplementedException();
        }

        public PinSite<int[]> ReadFails() {
            throw new System.NotImplementedException();
        }

        public PinSite<double[]> ReadStv() {
            throw new System.NotImplementedException();
        }
    }
}
