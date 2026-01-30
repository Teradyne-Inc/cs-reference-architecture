using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("VersionAgnostic_UT")]
[assembly: InternalsVisibleTo("V1_UT")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Csra")]
[assembly: AssemblyDescription("C# Reference Architecture")]
[assembly: AssemblyConfiguration("Unofficial")]
[assembly: AssemblyCompany("Teradyne")]
[assembly: AssemblyProduct("Csra")]
[assembly: AssemblyCopyright("Copyright © Teradyne 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("97cb8077-87cd-4770-abbd-5ff05a156b0e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(Csra.Info.VersionDefinition)]
[assembly: AssemblyFileVersion(Csra.Info.VersionDefinition)]

namespace Csra {

    internal static class Info {

        internal const string VersionDefinition = "0.0.0.1";
    }
}
