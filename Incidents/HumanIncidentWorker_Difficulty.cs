using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Difficulty : HumanIncidentWorker {
    public const String Name = "Difficulty";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_Difficulty)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_Difficulty
            allParams = Tell.AssertNotNull((HumanIncidentParams_Difficulty) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Find.Storyteller.difficultyDef = DefDatabase<DifficultyDef>.GetNamed(allParams.Difficulty, false) ?? Find.Storyteller.difficultyDef;

        SendLetter(@params);

        return ir;
    }
}

public class HumanIncidentParams_Difficulty : HumanIncidentParams {
    public string Difficulty = "";

    public HumanIncidentParams_Difficulty() {
    }

    public HumanIncidentParams_Difficulty(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Difficulty: [{Difficulty}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Difficulty, "difficulty");
    }
}