using System;
using System.Diagnostics;

namespace Freecon.Core.Objects
{
    public class DeadlockDetectedException:Exception
    {
        new public StackTrace StackTrace { get; set; }
    
    }
}
