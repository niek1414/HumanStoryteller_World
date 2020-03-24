using System;
using System.IO;
using System.Threading;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using HumanStoryteller.Web;
using RestSharp;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShowImage : HumanIncidentWorker {
        public const String Name = "ShowImage";

        private static String CachePath = (OSUtil.IsWindows ? "C:\\" : "/") +
                                          Path.Combine(Path.Combine("tmp", "RimWorld"), Path.Combine("HumanStoryteller", "image"));

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ShowImage)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ShowImage
                allParams = Tell.AssertNotNull((HumanIncidentParams_ShowImage) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var sc = HumanStoryteller.StoryComponent;
            if (allParams.RemoveAll) {
                sc.StoryOverlay.NotifyEnd<ShowImage>();
            } else {
                sc.StoryOverlay.AddItem(new ShowImage(
                    Path.Combine(CachePath, OSUtil.SaveFileName(allParams.Url))
                ));
            }

            SendLetter(allParams);
            return ir;
        }

        public override void PreLoad(HumanIncidentParms parms) {
            if (!(parms is HumanIncidentParams_ShowImage)) {
                Tell.Err("Tried to preload " + GetType() + " but param type was " + parms.GetType());
                return;
            }

            HumanIncidentParams_ShowImage
                allParams = Tell.AssertNotNull((HumanIncidentParams_ShowImage) parms, nameof(parms), GetType().Name);
            Tell.Log($"Preloading event {Name} with:{allParams}");

            if (allParams.RemoveAll) return;

            Interlocked.Increment(ref HumanStoryteller.ConcurrentActions);

            var thread = new Thread(() => {
                try {
                    Directory.CreateDirectory(CachePath);
                    var tmpPath = Path.Combine(CachePath, OSUtil.SaveFileName(allParams.Url));
                    Client.DownloadFile(tmpPath, allParams.Url);
                } catch (Exception e) {
                    Tell.Err("Exception in preloading image " + allParams.Url, e);
                }

                Interlocked.Decrement(ref HumanStoryteller.ConcurrentActions);
            }) {Name = "Preload image " + allParams.Url};
            thread.Start();
        }
    }

    public class HumanIncidentParams_ShowImage : HumanIncidentParms {
        public bool RemoveAll;
        public string Url;

        public HumanIncidentParams_ShowImage() {
        }

        public HumanIncidentParams_ShowImage(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, RemoveAll: [{RemoveAll}], Url: [{Url}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref RemoveAll, "removeAll");
            Scribe_Values.Look(ref Url, "url");
        }
    }
}