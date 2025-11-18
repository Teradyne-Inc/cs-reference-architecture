using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    public abstract class Setting_int : SettingBase<int> {

        protected override string SerializeValue(int value) => $"{value}{_unit}";
    }
}
