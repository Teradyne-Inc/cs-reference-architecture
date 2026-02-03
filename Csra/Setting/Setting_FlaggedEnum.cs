using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    [Serializable]
    public abstract class Setting_FlaggedEnum<T> : SettingBase<T> where T : Enum {

        protected override string SerializeValue(T value) {
            Type enumType = typeof(T);
            Array enumValues = Enum.GetValues(enumType);
            var result = new List<string>();

            foreach (object enumValue in enumValues) {
                if (value.HasFlag((Enum)enumValue) && Convert.ToInt32(enumValue) != 0) {
                    result.Add($"{enumType.Name}.{enumValue}");
                }
            }

            return result.Count > 0 ? string.Join(" | ", result) : $"{enumType.Name}.{value}";
        }
    }
}
