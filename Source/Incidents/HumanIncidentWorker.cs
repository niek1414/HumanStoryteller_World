using System;
using HumanStoryteller.Model;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    public abstract class HumanIncidentWorker : IExposable {
        public abstract IncidentResult Execute(HumanIncidentParms parms);

        protected void SendLetter(HumanIncidentParms parms, String title, String message, LetterDef type, LookTargets target,
            Faction relatedFaction = null, string debugInfo = null) {
            if (parms.Letter != null) {
                Letter l;
                if (parms.Letter.Type == null) {
                    l = LetterMaker.MakeLetter(title, message, type, target, relatedFaction);
                } else {
                    l = LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type);
                }

                Find.LetterStack.ReceiveLetter(l, debugInfo);
            }
        }

        public virtual void ExposeData() {}
    }

    public abstract class IncidentResult : IExposable, ILoadReferenceable {
        private int _id;

        protected IncidentResult() {
            _id = Rand.Int;
        }

        public virtual void ExposeData() {
            Scribe_Values.Look(ref _id, "id");
        }
        public string GetUniqueLoadID() {
            return $"IncidentResult_{_id}";
        }
    }
}