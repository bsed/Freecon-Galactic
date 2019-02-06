using System;

namespace Freecon.Core.Utils
{
    public static class ExtensionUtils
    {
        public static string ToHexString(this byte[] data)
        {
            return BitConverter.ToString(data);
        }
    }
}
