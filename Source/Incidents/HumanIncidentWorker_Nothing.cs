using System;
using HumanStoryteller.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Nothing : HumanIncidentWorker {
        public const String Name = "Nothing";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            //What? Did you expect a huge file or something?
            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }
}