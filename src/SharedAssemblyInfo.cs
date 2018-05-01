using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Brimit LLC")]
[assembly: AssemblyCopyright("Copyright © 2017")]

#if SC81
[assembly: AssemblyProduct("GatherContent.Connector 2.08 SC8.1")]
#else
#if SC80
[assembly: AssemblyProduct("GatherContent.Connector 2.08 SC80")]
#else
#if SC72
 [assembly: AssemblyProduct("GatherContent.Connector 2.08 SC7.2")]
#else
[assembly: AssemblyProduct("GatherContent.Connector 2.08")]
#endif
#endif
#endif
[assembly: AssemblyVersion("1.17.5.4")]
[assembly: AssemblyFileVersion("1.17.5.4")]