using System;
using System.IO;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.Util {
    public abstract class OSUtil {
        public static bool IsWindows {
            get {
                var p = (int) Environment.OSVersion.Platform;
                return p != 4 && p != 6 && p != 128;
            }
        }

        public static string SaveFileName(string name) {
            return name.Replace('/', '-')
                .Replace(':', '-')
                .Replace('*', '-')
                .Replace('?', '-')
                .Replace('|', '-')
                .Replace('<', '-')
                .Replace('>', '-')
                .Replace('"', '-')
                .Replace('\\', '-');
        }
        
        public static string GetCurlCommandLocation() {
            if (IsWindows) {
                var curlPath = HumanStoryteller.ContentPack.AssembliesFolder + "curl.exe";
                if (!File.Exists(curlPath)) {
                    Tell.Warn("Not found shipped cURL and on a windows OS, this is no problem for windows 10 users...");
                } else {
                    return curlPath;
                }
            }
            return "curl";
        }
    }
}