using System;

namespace HumanStoryteller.Util {
    public abstract class OSUtil {
        public static bool IsWindows {
            get {
                var p = (int) Environment.OSVersion.Platform;
                return p != 4 && p != 6 && p != 128;
            }
        }
    }
}