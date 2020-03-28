using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HumanStoryteller.Parser.Converter {
    public class IncidentConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

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
                return new FiringHumanIncident(null);;
            }

            switch (type) {
                case HumanIncidentWorker_Root.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Root(), new HumanIncidentParams_Root());
                    break;
                case HumanIncidentWorker_Nothing.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Nothing(), new HumanIncidentParms());
                    break;
                case HumanIncidentWorker_Alphabeavers.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Alphabeavers(), new HumanIncidentParams_Alphabeavers());
                    break;
                case HumanIncidentWorker_FarmAnimalsWanderIn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_FarmAnimalsWanderIn(), new HumanIncidentParams_FarmAnimalsWanderIn());
                    break;
                case HumanIncidentWorker_PsychicSoothe.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PsychicSoothe(), new HumanIncidentParams_PsychicSoothe());
                    break;
                case HumanIncidentWorker_RaidEnemy.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RaidEnemy(), new HumanIncidentParams_RaidEnemy());
                    break;
                case HumanIncidentWorker_Dialog.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Dialog(), new HumanIncidentParams_Dialog());
                    break;
                case HumanIncidentWorker_AmbrosiaSprout.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AmbrosiaSprout(), new HumanIncidentParams_AmbrosiaSprout());
                    break;
                case HumanIncidentWorker_AnimalInsanityMass.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AnimalInsanityMass(), new HumanIncidentParams_AnimalInsanityMass());
                    break;
                case HumanIncidentWorker_AnimalInsanitySingle.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_AnimalInsanitySingle(), new HumanIncidentParams_AnimalInsanitySingle());
                    break;
                case HumanIncidentWorker_Aurora.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Aurora(), new HumanIncidentParams_Aurora());
                    break;
                case HumanIncidentWorker_Difficulty.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Difficulty(), new HumanIncidentParams_Difficulty());
                    break;
                case HumanIncidentWorker_CropBlight.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CropBlight(), new HumanIncidentParams_CropBlight());
                    break;
                case HumanIncidentWorker_Disease.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Disease(), new HumanIncidentParams_Disease());
                    break;
                case HumanIncidentWorker_TempFlux.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TempFlux(), new HumanIncidentParams_TempFlux());
                    break;
                case HumanIncidentWorker_Eclipse.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Eclipse(), new HumanIncidentParams_Eclipse());
                    break;
                case HumanIncidentWorker_Flashstorm.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Flashstorm(), new HumanIncidentParams_Flashstorm());
                    break;
                case HumanIncidentWorker_HerdMigration.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_HerdMigration(), new HumanIncidentParams_HerdMigration());
                    break;
                case HumanIncidentWorker_Infestation.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Infestation(), new HumanIncidentParams_Infestation());
                    break;
                case HumanIncidentWorker_ManhunterPack.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ManhunterPack(), new HumanIncidentParams_ManhunterPack());
                    break;
                case HumanIncidentWorker_MeteoriteImpact.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_MeteoriteImpact(), new HumanIncidentParams_MeteoriteImpact());
                    break;
                case HumanIncidentWorker_TraderArrival.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TraderArrival(), new HumanIncidentParams_TraderArrival());
                    break;
                case HumanIncidentWorker_PsychicDrone.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PsychicDrone(), new HumanIncidentParams_PsychicDrone());
                    break;
                case HumanIncidentWorker_RefugeeChased.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RefugeeChased(), new HumanIncidentParams_RefugeeChased());
                    break;
                case HumanIncidentWorker_ShipPartCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ShipPartCrash(), new HumanIncidentParams_ShipPartCrash());
                    break;
                case HumanIncidentWorker_RefugeePodCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RefugeePodCrash(), new HumanIncidentParams_RefugeePodCrash());
                    break;
                case HumanIncidentWorker_ResourcePodCrash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ResourcePodCrash(), new HumanIncidentParams_ResourcePodCrash());
                    break;
                case HumanIncidentWorker_SelfTame.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SelfTame(), new HumanIncidentParams_SelfTame());
                    break;
                case HumanIncidentWorker_ShortCircuit.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ShortCircuit(), new HumanIncidentParams_ShortCircuit());
                    break;
                case HumanIncidentWorker_VisitorGroup.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_VisitorGroup(), new HumanIncidentParams_VisitorGroup());
                    break;
                case HumanIncidentWorker_WildManWandersIn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_WildManWandersIn(), new HumanIncidentParams_WildManWandersIn());
                    break;
                case HumanIncidentWorker_ToxicFallout.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ToxicFallout(), new HumanIncidentParams_ToxicFallout());
                    break;
                case HumanIncidentWorker_VolcanicWinter.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_VolcanicWinter(), new HumanIncidentParams_VolcanicWinter());
                    break;
                case HumanIncidentWorker_Planetkiller.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Planetkiller(), new HumanIncidentParams_Planetkiller());
                    break;
                case HumanIncidentWorker_CreatePawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CreatePawn(), new HumanIncidentParams_CreatePawn());
                    break;
                case HumanIncidentWorker_KillPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_KillPawn(), new HumanIncidentParams_KillPawn());
                    break;
                case HumanIncidentWorker_TimeTravel.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TimeTravel(), new HumanIncidentParams_TimeTravel());
                    break;
                case HumanIncidentWorker_SetRelation.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SetRelation(), new HumanIncidentParams_SetRelation());
                    break;
                case HumanIncidentWorker_HealPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_HealPawn(), new HumanIncidentParams_HealPawn());
                    break;
                case HumanIncidentWorker_PawnHealth.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PawnHealth(), new HumanIncidentParams_PawnHealth());
                    break;
                case HumanIncidentWorker_GiveThought.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_GiveThought(), new HumanIncidentParams_GiveThought());
                    break;
                case HumanIncidentWorker_RenamePawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RenamePawn(), new HumanIncidentParams_RenamePawn());
                    break;
                case HumanIncidentWorker_PlayAudio.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PlayAudio(), new HumanIncidentParams_PlayAudio());
                    break;
                case HumanIncidentWorker_SolarFlare.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SolarFlare(), new HumanIncidentParams_SolarFlare());
                    break;
                case HumanIncidentWorker_EditPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_EditPawn(), new HumanIncidentParams_EditPawn());
                    break;
                case HumanIncidentWorker_Rules.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Rules(), new HumanIncidentParams_Rules());
                    break;
                case HumanIncidentWorker_Research.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Research(), new HumanIncidentParams_Research());
                    break;
                case HumanIncidentWorker_DeleteItems.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_DeleteItems(), new HumanIncidentParams_DeleteItems());
                    break;
                case HumanIncidentWorker_OrbitalStrike.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_OrbitalStrike(), new HumanIncidentParams_OrbitalStrike());
                    break;
                case HumanIncidentWorker_MentalBreak.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_MentalBreak(), new HumanIncidentParams_MentalBreak());
                    break;
                case HumanIncidentWorker_CreateSettlement.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CreateSettlement(), new HumanIncidentParams_CreateSettlement());
                    break;
                case HumanIncidentWorker_IntentGiver.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_IntentGiver(), new HumanIncidentParams_IntentGiver());
                    break;
                case HumanIncidentWorker_CreateStructure.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CreateStructure(), new HumanIncidentParams_CreateStructure());
                    break;
                case HumanIncidentWorker_DestroyPosition.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_DestroyPosition(), new HumanIncidentParams_DestroyPosition());
                    break;
                case HumanIncidentWorker_ChapterSplash.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ChapterSplash(), new HumanIncidentParams_ChapterSplash());
                    break;
                case HumanIncidentWorker_RadioMessage.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RadioMessage(), new HumanIncidentParams_RadioMessage());
                    break;
                case HumanIncidentWorker_SpeedControl.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SpeedControl(), new HumanIncidentParams_SpeedControl());
                    break;
                case HumanIncidentWorker_ControlCamera.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ControlCamera(), new HumanIncidentParams_ControlCamera());
                    break;
                case HumanIncidentWorker_MovieMode.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_MovieMode(), new HumanIncidentParams_MovieMode());
                    break;
                case HumanIncidentWorker_RenameMap.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RenameMap(), new HumanIncidentParams_RenameMap());
                    break;
                case HumanIncidentWorker_CoupleDecouple.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_CoupleDecouple(), new HumanIncidentParams_CoupleDecouple());
                    break;
                case HumanIncidentWorker_FadeBlack.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_FadeBlack(), new HumanIncidentParams_FadeBlack());
                    break;
                case HumanIncidentWorker_SavePawnGroup.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_SavePawnGroup(), new HumanIncidentParams_SavePawnGroup());
                    break;
                case HumanIncidentWorker_ShowImage.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_ShowImage(), new HumanIncidentParams_ShowImage());
                    break;
                case HumanIncidentWorker_PointTo.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PointTo(), new HumanIncidentParams_PointTo());
                    break;
                case HumanIncidentWorker_TransferPawn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_TransferPawn(), new HumanIncidentParams_TransferPawn());
                    break;
                case HumanIncidentWorker_BubbleMessage.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_BubbleMessage(), new HumanIncidentParams_BubbleMessage());
                    break;
                case HumanIncidentWorker_Unfog.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Unfog(), new HumanIncidentParams_Unfog());
                    break;
                case HumanIncidentWorker_OnHit.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_OnHit(), new HumanIncidentParams_OnHit());
                    break;
                case HumanIncidentWorker_DisableStoryEvent.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_DisableStoryEvent(), new HumanIncidentParams_DisableStoryEvent());
                    break;
                case HumanIncidentWorker_Explosion.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Explosion(), new HumanIncidentParams_Explosion());
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