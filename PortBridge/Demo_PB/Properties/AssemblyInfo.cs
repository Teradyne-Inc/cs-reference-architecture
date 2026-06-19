using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Teradyne.Igxl.Interfaces.Public;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TER_PB")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("Teradyne Inc")]
[assembly: AssemblyProduct("TER_PB")]
[assembly: AssemblyCopyright("Copyright © Teradyne 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("91df2118-a086-4785-a166-9d364faab497")]

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
[assembly: AssemblyVersion(Demo.PB.Info.VersionDefinition)]
[assembly: AssemblyFileVersion(Demo.PB.Info.VersionDefinition)]
namespace Demo.PB {

    internal static class Info {

        internal const string VersionDefinition = "0.18.0";
    }
}
