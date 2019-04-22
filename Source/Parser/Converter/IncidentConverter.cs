using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimWorld;

namespace HumanStoryteller.Parser.Converter {
   public class IncidentConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        private String defaultIncidentTarget = "FirstOfPlayer";
        private HumanLetter defaultLetter = null;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(FiringHumanIncident);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            FiringHumanIncident incident;
            String type = jsonObject["type"].Value<string>();
            if (type == null) {
                Parser.LogParseError("incident", type);
                return LetterDefOf.NeutralEvent;
            }

            switch (type) {
                case HumanIncidentWorker_Root.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Root(),
                        new HumanIncidentParams_Root(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Nothing.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Nothing(),
                        new HumanIncidentParms(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Alphabeavers.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Alphabeavers(),
                        new HumanIncidentParams_Alphabeavers(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_FarmAnimalsWanderIn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_FarmAnimalsWanderIn(),
                        new HumanIncidentParams_FarmAnimalsWanderIn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_PsychicSoothe.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PsychicSoothe(),
                        new HumanIncidentParams_PsychicSoothe(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_RaidEnemy.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RaidEnemy(),
                        new HumanIncidentParams_RaidEnemy(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Dialog.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Dialog(),
                        new HumanIncidentParams_Dialog(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_AmbrosiaSprout.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AmbrosiaSprout(),
                        new HumanIncidentParams_AmbrosiaSprout(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_AnimalInsanityMass.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AnimalInsanityMass(),
                        new HumanIncidentParams_AnimalInsanityMass(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_AnimalInsanitySingle.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AnimalInsanitySingle(),
                        new HumanIncidentParams_AnimalInsanitySingle(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Aurora.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Aurora(),
                        new HumanIncidentParams_Aurora(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Difficulty.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Difficulty(),
                        new HumanIncidentParams_Difficulty(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_CropBlight.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CropBlight(),
                        new HumanIncidentParams_CropBlight(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Disease.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Disease(),
                        new HumanIncidentParams_Disease(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_TempFlux.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TempFlux(),
                        new HumanIncidentParams_TempFlux(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Eclipse.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Eclipse(),
                        new HumanIncidentParams_Eclipse(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Flashstorm.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Flashstorm(),
                        new HumanIncidentParams_Flashstorm(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_HerdMigration.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_HerdMigration(),
                        new HumanIncidentParams_HerdMigration(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Infestation.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Infestation(),
                        new HumanIncidentParams_Infestation(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_ManhunterPack.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ManhunterPack(),
                        new HumanIncidentParams_ManhunterPack(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_MeteoriteImpact.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_MeteoriteImpact(),
                        new HumanIncidentParams_MeteoriteImpact(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_TraderArrival.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TraderArrival(),
                        new HumanIncidentParams_TraderArrival(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_PsychicDrone.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PsychicDrone(),
                        new HumanIncidentParams_PsychicDrone(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_RefugeeChased.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RefugeeChased(),
                        new HumanIncidentParams_RefugeeChased(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_ShipPartCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ShipPartCrash(),
                        new HumanIncidentParams_ShipPartCrash(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_RefugeePodCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RefugeePodCrash(),
                        new HumanIncidentParams_RefugeePodCrash(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_ResourcePodCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ResourcePodCrash(),
                        new HumanIncidentParams_ResourcePodCrash(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_SelfTame.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SelfTame(),
                        new HumanIncidentParams_SelfTame(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_ShortCircuit.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ShortCircuit(),
                        new HumanIncidentParams_ShortCircuit(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_VisitorGroup.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_VisitorGroup(),
                        new HumanIncidentParams_VisitorGroup(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_WildManWandersIn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_WildManWandersIn(),
                        new HumanIncidentParams_WildManWandersIn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_ToxicFallout.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ToxicFallout(),
                        new HumanIncidentParams_ToxicFallout(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_VolcanicWinter.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_VolcanicWinter(),
                        new HumanIncidentParams_VolcanicWinter(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Planetkiller.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Planetkiller(),
                        new HumanIncidentParams_Planetkiller(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_CreatePawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CreatePawn(),
                        new HumanIncidentParams_CreatePawn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_KillPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_KillPawn(),
                        new HumanIncidentParams_KillPawn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_TimeTravel.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TimeTravel(),
                        new HumanIncidentParams_TimeTravel(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_SetRelation.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SetRelation(),
                        new HumanIncidentParams_SetRelation(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_HealPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_HealPawn(),
                        new HumanIncidentParams_HealPawn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_PawnHealth.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PawnHealth(),
                        new HumanIncidentParams_PawnHealth(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_GiveThought.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_GiveThought(),
                        new HumanIncidentParams_GiveThought(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_RenamePawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RenamePawn(),
                        new HumanIncidentParams_RenamePawn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_PlayAudio.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PlayAudio(),
                        new HumanIncidentParams_PlayAudio(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_SolarFlare.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SolarFlare(),
                        new HumanIncidentParams_SolarFlare(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_EditPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_EditPawn(),
                        new HumanIncidentParams_EditPawn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Rules.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Rules(),
                        new HumanIncidentParams_Rules(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Research.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Research(),
                        new HumanIncidentParams_Research(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_DeleteItems.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_DeleteItems(),
                        new HumanIncidentParams_DeleteItems(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_OrbitalStrike.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_OrbitalStrike(),
                        new HumanIncidentParams_OrbitalStrike(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_MentalBreak.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_MentalBreak(),
                        new HumanIncidentParams_MentalBreak(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Quest.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Quest(),
                        new HumanIncidentParams_Quest(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_TradeRequest.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TradeRequest(),
                        new HumanIncidentParams_TradeRequest(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_CreateSettlement.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CreateSettlement(),
                        new HumanIncidentParams_CreateSettlement(defaultIncidentTarget, defaultLetter));
                    break;
                default:
                    Parser.LogParseError("incident", type);
                    return new FiringHumanIncident(null);
            }

            serializer.Populate(jsonObject.CreateReader(), incident.Parms);
            return incident;
        }
    }
}