using System;
using HumanStoryteller.Model;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    public abstract class HumanIncidentWorker : IExposable {//TODO make also ILoadReferenceable
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

    public abstract class IncidentResult : IExposable {
        public virtual void ExposeData() {}
    }
}