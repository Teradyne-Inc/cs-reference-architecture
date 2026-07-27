using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Teradyne.Igxl.Interfaces.Public;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CsTestMethods")]
[assembly: AssemblyDescription("Demo Test Code: C#")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("Teradyne")]
[assembly: AssemblyProduct("CsTestMethods")]
[assembly: AssemblyCopyright("Copyright © Teradyne 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2538c8c4-cf92-45e3-a1b4-d26c75d6ea69")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(CsTestMethods.Info.VersionDefinition)]
[assembly: AssemblyFileVersion(CsTestMethods.Info.VersionDefinition)]

namespace CsTestMethods {

    internal static class Info {

        internal const string VersionDefinition = "0.19.0";
    }
}
