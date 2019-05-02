using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Root : HumanIncidentWorker {
        public const String Name = "Root";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            Tell.Err("Root object should NEVER be executed!");
            throw new InvalidOperationException("HS_ Root object should NEVER be executed!");
        }
    }

    public class HumanIncidentParams_Root : HumanIncidentParms {
        public bool OverrideMapGen;
        public bool OverrideMapLoc;
        public string Seed;
        public string Opening;
        public Number PawnAmount;
        public Number Coverage;
        public Number Rainfall;
        public Number Temperature;
        public Number Site;
        
        public HumanIncidentParams_Root() {
        }


        public HumanIncidentParams_Root(string target, HumanLetter letter, bool overrideMapGen, bool overrideMapLoc, string seed, string opening, Number coverage, Number rainfall, Number temperature, Number site, Number pawnAmount) : base(target, letter) {
            OverrideMapGen = overrideMapGen;
            OverrideMapLoc = overrideMapLoc;
            Seed = seed;
            Opening = opening;
            PawnAmount = pawnAmount;
            Coverage = coverage;
            Rainfall = rainfall;
            Temperature = temperature;
            Site = site;
        }

        public HumanIncidentParams_Root(String target, HumanLetter letter, bool overrideMapGen = false, bool overrideMapLoc = false, string seed = "", string opening = "") : this(target, letter, overrideMapGen, overrideMapLoc, seed, opening, new Number(), new Number(), new Number(), new Number(), new Number())  {
        }

        public override string ToString() {
            return $"{base.ToString()}, OverrideMapGen: {OverrideMapGen}, OverrideMapLoc: {OverrideMapLoc}, Seed: {Seed}, Opening: {Opening}, PawnAmount: {PawnAmount}, Coverage: {Coverage}, Rainfall: {Rainfall}, Temperature: {Temperature}, Site: {Site}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref OverrideMapGen, "overrideMapGen");
            Scribe_Values.Look(ref Seed, "seed");
            Scribe_Values.Look(ref Opening, "opening");
            Scribe_Deep.Look(ref PawnAmount, "pawnAmount");
            Scribe_Deep.Look(ref Coverage, "coverage");
            Scribe_Deep.Look(ref Rainfall, "rainfall");
            Scribe_Deep.Look(ref Temperature, "temperature");
        }
    }
}