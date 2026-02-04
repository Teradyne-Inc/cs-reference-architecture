using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    [Serializable]
    public class DigitalPins : IDigitalPins {

        [NonSerialized]
        private protected DriverDigitalPins _hardwareApi; // IG-XL object
        [NonSerialized]
        private IValuePerSite<ChInitState> _initState;
        [NonSerialized]
        private IValuePerSite<ChStartState> _startState;
        [NonSerialized]
        private IValuePerSite<tlLockState> _lockState;
        [NonSerialized]
        private IPpmuPins _ppmu;
        private string _name;

        internal DigitalPins(string pinList) : this(pinList, TheHdw.Digital.Pins(pinList)) { }

        internal DigitalPins(string pinList, DriverDigitalPins digital, PpmuPins ppmu = null) {
            _name = pinList;
            _hardwareApi = digital;
            _ppmu = ppmu;
            _ = InitState; // prevent lazy loading
            _ = StartState;
            _ = LockState;
        }

        public DriverDigitalPins HardwareApi => _hardwareApi ??= TheHdw.Digital.Pins(Name);

        public IValuePerSite<ChInitState> InitState => _initState ??= new ValuePerSiteType<ChInitState>(
                    setValue: value => { HardwareApi.InitState = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.InitState = siteValues[site]; });
                    });

        public IValuePerSite<ChStartState> StartState => _startState ??= new ValuePerSiteType<ChStartState>(
                    setValue: value => { HardwareApi.StartState = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.StartState = siteValues[site]; });
                    });

        public IValuePerSite<tlLockState> LockState => _lockState ??= new ValuePerSiteType<tlLockState>(
                    setValue: value => { HardwareApi.LockState = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.LockState = siteValues[site]; });
                    });

        public IPpmuPins Ppmu => _ppmu;

        public string Name => _name;

        public virtual void Connect() {
            HardwareApi.Connect();
        }

        public virtual void Disconnect() {
            HardwareApi.Disconnect();
        }

        public IDigitalPins[] GetIndividualPins() {
            TheExec.DataManager.DecomposePinList(Name, out string[] individualPins, out _);
            return individualPins.Select(pin => new DigitalPins(pin)).ToArray();
        }

        public IDigitalPins[] GetPinListItem() {
            string[] pins = Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();
            if (pins.Count() == 1) return [this];
            return pins.Select(pin => new DigitalPins(pin)).ToArray();
        }
    }
}
