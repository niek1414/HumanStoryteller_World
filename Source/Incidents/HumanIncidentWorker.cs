using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    public abstract class HumanIncidentWorker : IExposable {
        private void PreExecute(HumanIncidentParms parms) {
            HumanStoryteller.StoryComponent.SameAsLastEvent = (Map) parms.GetTarget();
        }
        
        private void PostExecute(HumanIncidentParms parms, IncidentResult incidentResult) {
            incidentResult.Target = parms.Target;
        }

        public IncidentResult ExecuteIncident(HumanIncidentParms parms) {
            PreExecute(parms);
            IncidentResult ir = Execute(parms);
            PostExecute(parms, ir);
            return ir;
        }
        
        protected abstract IncidentResult Execute(HumanIncidentParms parms);

        protected void SendLetter(HumanIncidentParms parms, String title, String message, LetterDef type, LookTargets target,
            Faction relatedFaction = null, string debugInfo = null) {
            if (parms.Letter != null) {
                Letter l;
                if (parms.Letter.Type == null) {
                    l = LetterMaker.MakeLetter(title, message, type, target, relatedFaction);
                } else {
                    if (parms.Letter.Shake) {
                        Find.CameraDriver.shaker.DoShake(4f);
                    }

                    l = LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type, target, relatedFaction);
                    if (parms.Letter.Force) {
                        l.OpenLetter();
                    }
                }

                Find.LetterStack.ReceiveLetter(l, debugInfo);
            }
        }

        protected void SendLetter(HumanIncidentParms parms, LookTargets target = null, Faction relatedFaction = null, string debugInfo = null) {
            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }

                var letter = LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type, target, relatedFaction);
                if (parms.Letter.Force) {
                    letter.OpenLetter();
                }

                Find.LetterStack.ReceiveLetter(letter, debugInfo);
            }
        }

        public virtual void ExposeData() {
        }
    }

    public class IncidentResult : IExposable, ILoadReferenceable {
        private int _id;
        public string Target;

        public IncidentResult() {
            _id = Rand.Int;
        }

        public int Id => _id;

        public Map GetTarget() {
            return MapUtil.GetTarget(Target);
        }
        
        public virtual void ExposeData() {
            Scribe_Values.Look(ref _id, "id");
            Scribe_Values.Look(ref Target, "target");
        }

        public string GetUniqueLoadID() {
            return $"IncidentResult_{_id}";
        }
    }
}