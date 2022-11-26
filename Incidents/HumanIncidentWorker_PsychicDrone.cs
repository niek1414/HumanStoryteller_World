using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_PsychicDrone : HumanIncidentWorker {
    public const String Name = "PsychicDrone";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_PsychicDrone)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_PsychicDrone
            allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicDrone) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        var paramsDuration = allParams.Duration.GetValue();
        int duration = Mathf.RoundToInt(paramsDuration != -1
            ? paramsDuration * 60000f
            : IncidentDef.Named("PsychicDrone").durationDays.RandomInRange * 60000f);
        GameCondition_PsychicEmanation gameCondition_PsychicEmanation =
            (GameCondition_PsychicEmanation) GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration);

        PsychicDroneLevel l;
        switch (allParams.PsyLevel) {
            case "LOW":
                l = PsychicDroneLevel.BadLow;
                break;
            case "MEDIUM":
                l = PsychicDroneLevel.BadMedium;
                break;
            case "HIGH":
                l = PsychicDroneLevel.BadHigh;
                break;
            case "EXTREME":
                l = PsychicDroneLevel.BadExtreme;
                break;
            default:
                l = PsychicDroneLevel.BadMedium;
                break;
        }

        gameCondition_PsychicEmanation.level = l;

        Gender g = PawnUtil.GetGender(allParams.Gender);

        gameCondition_PsychicEmanation.gender = g != Gender.None ? g : map.mapPawns.FreeColonists.RandomElement().gender;
        map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
        string text = "LetterIncidentPsychicDrone".Translate(g.ToString().Translate().ToLower(), l.GetLabel());
        SendLetter(allParams, "LetterLabelPsychicDrone".Translate(), text, LetterDefOf.NegativeEvent, null);

        return ir;
    }
}

public class HumanIncidentParams_PsychicDrone : HumanIncidentParams {
    public Number Duration = new Number();
    public string Gender = "";
    public string PsyLevel = "";

    public HumanIncidentParams_PsychicDrone() {
    }

    public HumanIncidentParams_PsychicDrone(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Duration: [{Duration}], Gender: [{Gender}], PsyLevel: [{PsyLevel}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Duration, "duration");
        Scribe_Values.Look(ref Gender, "gender");
        Scribe_Values.Look(ref PsyLevel, "psyLevel");
    }
}