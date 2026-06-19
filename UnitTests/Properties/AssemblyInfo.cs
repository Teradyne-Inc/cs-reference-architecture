using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("UnitTestExample")]
[assembly: AssemblyDescription("Unit Test Example")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("Teradyne")]
[assembly: AssemblyProduct("UnitTestExample")]
[assembly: AssemblyCopyright("Copyright © Teradyne 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("0416ce27-9c45-453f-ad58-5cf3ddad0b20")]

// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(UnitTestExample.Info.VersionDefinition)]
[assembly: AssemblyFileVersion(UnitTestExample.Info.VersionDefinition)]

namespace UnitTestExample {

    internal static class Info {

        internal const string VersionDefinition = "0.18.0";
    }
}
