using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    public abstract class Setting_Enum<T> : SettingBase<T> where T : Enum {

        protected override string SerializeValue(T value) => $"{typeof(T).Name}.{value}";
    }
}
