using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;

namespace Csra {
    public class CSRATransactionConfig : ITransactionConfig {

        private List<string> _registerMaps;
        private Dictionary<string, Pins.Pin> _pins;
        private Dictionary<string, Port> _ports;

        public CSRATransactionConfig() {
            _registerMaps = new List<string>();
            _pins = new Dictionary<string, Pins.Pin>();
            _ports = new Dictionary<string, Port>();
        }

        public bool Valid {
            get {
                // ToDo: Implement validation logic
                return false;
            }
        }

        public string DefaultPort { get; set; }

        public bool AddPin(string pin, string atePin = "", string type = "", string defaultState = "", string initState = "") {
            if (atePin == string.Empty) {
                atePin = pin;
            }
            if (!_pins.ContainsKey(pin)) {
                _pins.Add(pin, new Pins.Pin(atePin));
                return true;
            }
            return false;
        }

        public class Port {
            public string Name;
            public string Protocol;
            public List<string> Pins;
            public string RegisterMap;

            public Port(string name, string protocol, List<string> pins, string registerMap) {
                Name = name;
                Protocol = protocol;
                Pins = new List<string>(pins);
                RegisterMap = registerMap;
            }
        }
    }
}
