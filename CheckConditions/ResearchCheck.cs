using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions; 
public class ResearchCheck : CheckCondition {
    public const String Name = "Research";
    private ResearchProjectDef _researchProject;

    public ResearchCheck() {
    }

    public ResearchCheck(ResearchProjectDef researchProject) {
        _researchProject = Tell.AssertNotNull(researchProject, nameof(researchProject), GetType().Name);
    }

    public override bool Check(IncidentResult result, int checkPosition) {
        return _researchProject.IsFinished;
    }

    public override string ToString() {
        return $"Research: [{_researchProject}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref _researchProject, "researchProject");
    }
}