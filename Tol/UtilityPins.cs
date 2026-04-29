using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    [Serializable]
    public class UtilityPins : IUtilityPins {

        [NonSerialized]
        private protected tlDriverUtilityPins _hardwareApi; // IG-XL object
        [NonSerialized]
        private IValuePerSite<tlUtilBitState> _state;
        private IUtilityPins _utility;
        private string _name;

        internal UtilityPins(string pinList) : this(pinList, TheHdw.Utility.Pins(pinList)) { }

        internal UtilityPins(string pinList, tlDriverUtilityPins utility) {
            _name = pinList;
            _hardwareApi = utility;
            _ = State; // prevent lazy loading
        }

        public tlDriverUtilityPins HardwareApi => _hardwareApi ??= TheHdw.Utility.Pins(Name);

        public IValuePerSite<tlUtilBitState> State => _state ??= new ValuePerSiteType<tlUtilBitState>(
                    setValue: value => { HardwareApi.State = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.State = siteValues[site]; });
                    });

        public string Name => _name;

        public IUtilityPins[] GetIndividualPins() {
            TheExec.DataManager.DecomposePinList(Name, out string[] individualPins, out _);
            return individualPins.Select(pin => new UtilityPins(pin)).ToArray();
        }

        public IUtilityPins[] GetPinListItem() {
            string[] pins = Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();
            if (pins.Count() == 1) return [this];
            return pins.Select(pin => new UtilityPins(pin)).ToArray();
        }
    }
}
