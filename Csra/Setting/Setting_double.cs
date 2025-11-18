using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting {

    public abstract class Setting_double : SettingBase<double> {

        // TODO: improve floating point compare - https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/977
        //protected override bool CompareValue(double a, double b) => Math.Abs(a - b) < 1e-6;

        protected override string SerializeValue(double value) => $"{value:0.000000}{_unit}";
    }
}
