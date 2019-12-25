using System;
using System.Reflection;
using Harmony;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Patch {
    public class Shot_Patch {
        public static bool TryingToCastShot;

        public Shot_Patch() {
        }

        public static void Patch(HarmonyInstance harmony) {
            MethodInfo targetTryCastShot = AccessTools.Method(typeof(Verb_LaunchProjectile), "TryCastShot");
            HarmonyMethod patchPreTryCastShot = new HarmonyMethod(typeof(Shot_Patch).GetMethod("TryCastShotPre"));
            HarmonyMethod patchPostTryCastShot = new HarmonyMethod(typeof(Shot_Patch).GetMethod("TryCastShotPost"));
            harmony.Patch(targetTryCastShot, patchPreTryCastShot, patchPostTryCastShot);
            
            MethodInfo targetHitReport = AccessTools.Method(typeof(ShotReport), "HitReportFor");
            HarmonyMethod patchHitReportForPost = new HarmonyMethod(typeof(Shot_Patch).GetMethod("HitReportForPost"));
            harmony.Patch(targetHitReport, null, patchHitReportForPost);
        }
        
        /**
         * Single thread depended! 
         */
        public static void TryCastShotPre() {
            TryingToCastShot = true;
        }
        
        /**
         * Single thread depended! 
         */
        public static void TryCastShotPost() {
            TryingToCastShot = false;
        }

        public static void HitReportForPost(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target) {
            if (!TryingToCastShot || Main_Patch.ShouldNotMessWithGame()) {
                return;
            }

            if (!target.HasThing) {
                return;
            }

            if (!(target.Thing is Pawn receiver) || !(caster is Pawn sender)) {
                return;
            }

            if (ShotReportUtil.GetShotReport(sender, receiver, out var type)) {
                if (!type.HasValue) {
                    Tell.Err("GetShotReport returned no HitResponseType but did return 'true'!");
                    return;
                }

                switch (type.Value) {
                    case ShotReportUtil.HitResponseType.AlwaysHit:
                        Traverse.Create(__result).Field("factorFromShooterAndDist").SetValue(1f);
                        Traverse.Create(__result).Field("factorFromEquipment").SetValue(1f);
                        Traverse.Create(__result).Field("factorFromTargetSize").SetValue(1f);
                        Traverse.Create(__result).Field("factorFromWeather").SetValue(1f);
                        Traverse.Create(__result).Field("forcedMissRadius").SetValue(1f);
                        Traverse.Create(__result).Field("coversOverallBlockChance").SetValue(0f);
                        Traverse.Create(__result).Field("coveringGas").SetValue(null);
                        break;
                    case ShotReportUtil.HitResponseType.AlwaysMis:
                        Traverse.Create(__result).Field("factorFromShooterAndDist").SetValue(0f);
                        Traverse.Create(__result).Field("factorFromEquipment").SetValue(0f);
                        Traverse.Create(__result).Field("factorFromTargetSize").SetValue(0f);
                        Traverse.Create(__result).Field("factorFromWeather").SetValue(0f);
                        Traverse.Create(__result).Field("coversOverallBlockChance").SetValue(1f);
                        Traverse.Create(__result).Field("forcedMissRadius").SetValue(0f);
                        break;
                    case ShotReportUtil.HitResponseType.Unaltered:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown HitResponseType: " + type.Value);
                }
                
                Tell.Log("Triggered OnHit event, type: " + type.Value + "\n" + __result.GetTextReadout());
            }
        }
    }
}