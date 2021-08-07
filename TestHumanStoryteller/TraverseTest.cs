using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Harmony;
using Xunit;

namespace TestHumanStoryteller {
    public class TraverseTest {
        // [Fact]
        // public void Reflection() {
        //     ShotReport shotReport = new ShotReport();
        //     var coverInfos = new List<CoverInfo>{CoverInfo.Invalid};
        //     var traverse = Traverse.Create(shotReport);
        //     traverse.Field("distance").SetValue((float)10);
        //     traverse.Field("covers").SetValue(coverInfos);
        //     var newValue = Traverse.Create(traverse).Field("_root").GetValue();
        //     
        //     Assert.Equal(coverInfos, Traverse.Create(newValue).Field("covers").GetValue());
        //     Assert.Equal((float)10, Traverse.Create(newValue).Field("distance").GetValue());
        // }
        //
        
        [Fact]
        public void Zip() {
            String ziped = "H4sIAIK/b14A/2XMsQ2AMAwF0V1cp4DY/pZoWQKBmCTK7lBAddVJV7xhu23XsMO2Vb3Z+Xbps33HcQIncYTzyw7ZITtkh+yQA3JADsgBOSAn5ISckBNyQhZkQRZkQRbkglyQC3JBrnnPB5ta2fkjAgAA";
            Assert.Equal("", Unzip(ziped));
        }
        
        private static string Unzip(string str) {
            if (str.StartsWith("{") || str.StartsWith("[")) {
                return str;
            }
            using (var msi = new MemoryStream(Convert.FromBase64String(str)))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        
        private static void CopyTo(Stream src, Stream dest) {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}