using HumanStoryteller.CheckConditions;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Helper.IntentHelper {
    public class LordWithMemory : Lord {
        public IncidentResult_Traveled TraveledIR;

        public LordWithMemory() {
            
        }
        
        public LordWithMemory(IncidentResult_Traveled traveledIR) {
            TraveledIR = traveledIR;
        }

        public new void ExposeData() {
            Scribe_References.Look(ref TraveledIR, "traveledIR");
        }
    }
}