using System;
using HumanStoryteller.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Nothing : HumanIncidentWorker {
        public const String Name = "Nothing";

        public override void Execute(HumanIncidentParms parms) {
            //What? Did you expect a huge file or something?
            if (parms.Letter != null) {
                if (parms.Letter.Type != null) {
                    Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
                }
            }
        }
    }
}