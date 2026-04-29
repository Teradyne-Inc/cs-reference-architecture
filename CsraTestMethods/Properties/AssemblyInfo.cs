using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CsraTestMethods")]
[assembly: AssemblyDescription("Demo Test Code: C#RA")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("Teradyne")]
[assembly: AssemblyProduct("CsraTestMethods")]
[assembly: AssemblyCopyright("Copyright © Teradyne 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f7581e15-d703-4931-b68c-6310f3f75f1e")]

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
[assembly: AssemblyVersion(CsraTestMethods.Info.VersionDefinition)]
[assembly: AssemblyFileVersion(CsraTestMethods.Info.VersionDefinition)]
namespace CsraTestMethods {

    internal static class Info {

        internal const string VersionDefinition = "0.17";
    }
}
