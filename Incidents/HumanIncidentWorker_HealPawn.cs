using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents;
class HumanIncidentWorker_HealPawn : HumanIncidentWorker
{
    public const String Name = "HealPawn";

    protected override IncidentResult Execute(HumanIncidentParams @params)
    {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_HealPawn))
        {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_HealPawn allParams =
            Tell.AssertNotNull((HumanIncidentParams_HealPawn)@params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map)allParams.GetTarget();

        foreach (var pawn in allParams.Pawns.Filter(map))
        {
            if (pawn == null)
            {
                continue;
            }

            IEnumerable<Hediff_Injury> injuries;
            if (allParams.Miracle)
            {
                injuries = from x in
#if v1_4
                           pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
#else
                               pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
#endif
                    select x;
            }
            else
            {
                injuries = from x in
#if v1_4
                           pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
#else
                               pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
#endif
                           where x.CanHealNaturally() || x.CanHealFromTending()
                           select x;
            }
            foreach (var injury in injuries)
            {
                injury.Heal(injury.Severity);
            }
        }

        SendLetter(@params);

        return ir;
    }
}

public class HumanIncidentParams_HealPawn : HumanIncidentParams
{
    public PawnGroupSelector Pawns = new PawnGroupSelector();
    public bool Miracle;

    public HumanIncidentParams_HealPawn()
    {
    }

    public HumanIncidentParams_HealPawn(Target target, HumanLetter letter) : base(target, letter)
    {
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Pawns: [{Pawns}], Miracle: [{Miracle}]";
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref Pawns, "names");
        Scribe_Values.Look(ref Miracle, "miracle");
    }
}