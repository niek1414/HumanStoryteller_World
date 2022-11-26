using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents.Jobs; 
public class LordJob_TravelExact : LordJob {
    private IntVec3 travelDest;

    public LordJob_TravelExact() {
    }

    public LordJob_TravelExact(IntVec3 travelDest) {
        this.travelDest = travelDest;
    }

    public override StateGraph CreateGraph() {
        StateGraph stateGraph = new StateGraph();
        LordToil_TravelExact lordToilTravel = new LordToil_TravelExact(travelDest);
        stateGraph.StartingToil = lordToilTravel;
        LordToil_DefendPoint lordToilDefendPoint = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToilDefendPoint);
        Transition transition1 = new Transition(lordToilTravel, lordToilDefendPoint);
        transition1.AddTrigger(new Trigger_PawnHarmed());
        transition1.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition1.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition1);
        Transition transition2 = new Transition(lordToilDefendPoint, lordToilTravel);
        transition2.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
        stateGraph.AddTransition(transition2);
        return stateGraph;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref travelDest, "travelDest");
    }
}