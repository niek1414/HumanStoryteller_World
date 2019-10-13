using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Helper.IntentHelper {
    public class LordUtil {
        public static Lord MakeNewLord(Faction faction, LordJob lordJob, Map map, IncidentResult_Traveled ir, IEnumerable<Pawn> startingPawns = null)
        {
            if (map == null)
            {
                Log.Warning("Tried to create a lord with null map.", false);
                return null;
            }
            Lord newLord = new LordWithMemory(ir);
            newLord.loadID = Find.UniqueIDsManager.GetNextLordID();
            newLord.faction = faction;
            map.lordManager.AddLord(newLord);
            newLord.SetJob(lordJob);
            newLord.GotoToil(newLord.Graph.StartingToil);
            if (startingPawns != null)
            {
                foreach (Pawn startingPawn in startingPawns)
                    newLord.AddPawn(startingPawn);
            }
            return newLord;
        }
    }
}