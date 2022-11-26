using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_LongEntry : HumanIncidentWorker {
    public const String Name = "LongEntry";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        Tell.Err("LongEntry object should NEVER be executed!");
        throw new InvalidOperationException("HS_ LongEntry object should NEVER be executed!");
    }
}

public class HumanIncidentParams_LongEntry : HumanIncidentParams {
    public bool OverrideMapGen;
    public bool OverrideMapLoc;
    public string Seed = "";
    public RichText Opening = new RichText();
    public string StartSeason = "";
    public Number PawnAmount = new Number();
    public Number Coverage = new Number();
    public Number Rainfall = new Number();
    public Number Temperature = new Number();
    public Number Population = new Number();
    public Number Site = new Number();
    public Number MapSize = new Number();
    
    public HumanIncidentParams_LongEntry() {
    }

    public override string ToString() {
        return $"{base.ToString()}, OverrideMapGen: [{OverrideMapGen}], OverrideMapLoc: [{OverrideMapLoc}], Seed: [{Seed}], Opening: [{Opening}], StartSeason: [{StartSeason}], PawnAmount: [{PawnAmount}], Coverage: [{Coverage}], Rainfall: [{Rainfall}], Temperature: [{Temperature}], Population: [{Population}], Site: [{Site}], MapSize: [{MapSize}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref OverrideMapGen, "overrideMapGen");
        Scribe_Values.Look(ref Seed, "seed");
        Scribe_Deep.Look(ref Opening, "opening");
        Scribe_Values.Look(ref StartSeason, "startSeason");
        Scribe_Deep.Look(ref PawnAmount, "pawnAmount");
        Scribe_Deep.Look(ref Coverage, "coverage");
        Scribe_Deep.Look(ref Rainfall, "rainfall");
        Scribe_Deep.Look(ref Temperature, "temperature");
        Scribe_Deep.Look(ref Population, "population");
        Scribe_Deep.Look(ref Site, "site");
        Scribe_Deep.Look(ref MapSize, "mapSize");
    }
}