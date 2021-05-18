using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

#if !NET5_0_OR_GREATER
[assembly: TestCategory("SkipWhenLiveUnitTesting")]
#endif
