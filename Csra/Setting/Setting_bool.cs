using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    [Serializable]
    public abstract class Setting_bool : SettingBase<bool> { }
}
