﻿using Android.App;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Rocks.Wasabee.Mobile.Android")]
[assembly: AssemblyDescription("Wasabee is not for dating.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Rocks.Wasabee.Mobile.Android")]
[assembly: AssemblyCopyright("Copyright © Sébastien FORAY 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("en-US")]
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Add some common permissions, these can be removed if not needed
#if DEBUG
[assembly: UsesPermission(Android.Manifest.Permission.WakeLock)]
#endif