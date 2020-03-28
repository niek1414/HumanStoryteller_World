using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SpeedControl : HumanIncidentWorker {
        public const String Name = "SpeedControl";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_SpeedControl)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_SpeedControl
                allParams = Tell.AssertNotNull((HumanIncidentParams_SpeedControl) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var sc = HumanStoryteller.StoryComponent;
            sc.StoryStatus.DisableSpeedControls = allParams.LockControls;
            if (allParams.Speed.Equals("Slow")) {
                sc.StoryStatus.SignalForceSlowMotion();
            } else if (allParams.Speed != "") {
                TimeSpeed speed = TimeSpeed.Normal;
                bool notFound = false;
                switch (allParams.Speed) {
                    case "Paused":
                        speed = TimeSpeed.Paused;
                        break;
                    case "Normal":
                        speed = TimeSpeed.Normal;
                        break;
                    case "Fast":
                        speed = TimeSpeed.Fast;
                        break;
                    case "Superfast":
                        speed = TimeSpeed.Superfast;
                        break;
                    case "Ultrafast":
                        speed = TimeSpeed.Ultrafast;
                        break;
                    default:
                        Tell.Warn("TimeSpeed not known: " + allParams.Speed);
                        notFound = true;
                        break;
                }

                if (!notFound) {
                    Find.TickManager.CurTimeSpeed = speed;
                }
            }
            
            if (Find.TickManager.CurTimeSpeed == TimeSpeed.Paused && sc.StoryStatus.DisableSpeedControls) {
                Tell.Warn("Prevented time control deadlock!");
                sc.StoryStatus.DisableSpeedControls = false;
            }
            
            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_SpeedControl : HumanIncidentParms {
        public bool LockControls;
        public string Speed = "";

        public HumanIncidentParams_SpeedControl() {
        }

        public HumanIncidentParams_SpeedControl(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, LockControls: [{LockControls}], Speed: [{Speed}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref LockControls, "lockControls");
            Scribe_Values.Look(ref Speed, "speed");
        }
    }
}