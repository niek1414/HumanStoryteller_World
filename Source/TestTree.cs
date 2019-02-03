using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using RimWorld;
using Verse;

namespace HumanStoryteller {
    public static class TestTree {
        public static StoryGraph CreateTree(StorytellerComp storyteller) {
            StoryNode c = new StoryNode(new StoryEvent("c", "Alphabeavers", CreateAlphabeavers()));
            StoryNode b2 = new StoryNode(new StoryEvent("b2", "RaidWithNames", CreateRaidWithNames(storyteller)), new Connection(30, c));
            StoryNode b1 = new StoryNode(new StoryEvent("b1", "PsychicSoothe", CreatePsychicSoothe()), new Connection(30, c));
            StoryNode a = new StoryNode(new StoryEvent("a", "AnimalName", CreateJoinAnimalName()),  new Connection(30, b1),  new Connection(0, b2),
                new List<CheckCondition> {new PawnHealthCheck("Mike's pet", HealthCondition.Healthy)});

            StoryNode root = new StoryNode(null, new Connection(30, a));
            return new StoryGraph(root);
        }

        private static FiringHumanIncident CreateAlphabeavers() {
            return new FiringHumanIncident(new HumanIncidentWorker_Alphabeavers(),
                new HumanIncidentParams_Alphabeavers("OfPlayer", null, 100));
        }

        private static FiringHumanIncident CreateJoinAnimalName() {
            IncidentParms parms = new IncidentParms();
            parms.pawnGroups = new Dictionary<Pawn, int>();
            parms.target = Find.Maps.Find(x => x.ParentFaction.IsPlayer);
            Pawn pawn = new Pawn();
            pawn.Name = new NameSingle("Mike's pet");
            parms.pawnGroups.Add(pawn, 0);
            return new FiringHumanIncident(new HumanIncidentWorker_FarmAnimalsWanderIn(),
                new HumanIncidentParams_FarmAnimalsWanderIn(
                    "OfPlayer",
                    null,
                    -1,
                    new List<string> {"Mike's pet"}
                ));
        }

        private static FiringHumanIncident CreatePsychicSoothe() {
            IncidentParms parms = new IncidentParms();
            parms.target = Find.Maps.Find(x => x.ParentFaction.IsPlayer);
            return new FiringHumanIncident(new HumanIncidentWorker_PsychicSoothe(),
                new HumanIncidentParams_PsychicSoothe(
                    "OfPlayer",
                    null
                ));
        }


        private static FiringHumanIncident CreateRaidWithNames(StorytellerComp storyteller) {
            return new FiringHumanIncident(new HumanIncidentWorker_RaidEnemy(),
                new HumanIncidentParams_RaidEnemy(
                    "OfPlayer",
                    null,
                    -1,
                    "",
                    "",
                    "",
                    new List<string>()
                ));
        }
    }
}