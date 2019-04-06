using System;
using System.Collections.Generic;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Research : HumanIncidentWorker {
        public const String Name = "Research";


        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Research)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Research allParams = Tell.AssertNotNull((HumanIncidentParams_Research) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            if (allParams.FinishCurrent) {
                Current.Game.researchManager.FinishProject(Current.Game.researchManager.currentProj);
            }
            
            foreach (var project in allParams.Projects) {
                Current.Game.researchManager.FinishProject(ResearchProjectDef.Named(project));
            }

            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }

                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_Research : HumanIncidentParms {
        public List<string> Projects;
        public bool FinishCurrent;

        public HumanIncidentParams_Research() {
        }

        public HumanIncidentParams_Research(string target, HumanLetter letter, List<string> projects = null, bool finishCurrent = false) : base(target, letter) {
            Projects = projects ?? new List<string>();
            FinishCurrent = finishCurrent;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Projects, "projects", LookMode.Value);
            Scribe_Values.Look(ref FinishCurrent, "finishCurrent");
        }
    }
}