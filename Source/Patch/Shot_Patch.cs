using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Patch {
    public class Shot_Patch {
        public static bool TryingToCastShot;

        public Shot_Patch() {
        }

        public static void Patch(Harmony harmony) {
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

                ShotReport tempReport = new ShotReport();
                var traverse = Traverse.Create(tempReport);
                var traverseOld = Traverse.Create(__result);
                
                switch (type.Value) {
                    case ShotReportUtil.HitResponseType.AlwaysHit:
                        traverse.Field("factorFromShooterAndDist").SetValue(1f);
                        traverse.Field("factorFromEquipment").SetValue(1f);
                        traverse.Field("factorFromTargetSize").SetValue(1f);
                        traverse.Field("factorFromWeather").SetValue(1f);
                        traverse.Field("forcedMissRadius").SetValue(0f);
                        traverse.Field("coversOverallBlockChance").SetValue(0f);
                        traverse.Field("coveringGas").SetValue(null);
                        
                        traverse.Field("target").SetValue(traverseOld.Field("target").GetValue());
                        traverse.Field("distance").SetValue(traverseOld.Field("distance").GetValue());
                        traverse.Field("covers").SetValue(traverseOld.Field("covers").GetValue());
                        traverse.Field("shootLine").SetValue(__result.ShootLine);
                        __result = (ShotReport) Traverse.Create(traverse).Field("_root").GetValue();
                        break;
                    case ShotReportUtil.HitResponseType.AlwaysMis:
                        traverse.Field("factorFromShooterAndDist").SetValue(0f);
                        traverse.Field("factorFromEquipment").SetValue(0f);
                        traverse.Field("factorFromTargetSize").SetValue(0f);
                        traverse.Field("factorFromWeather").SetValue(0f);
                        traverse.Field("coversOverallBlockChance").SetValue(1f);
                        traverse.Field("forcedMissRadius").SetValue(0f);
                        
                        traverse.Field("coveringGas").SetValue(traverseOld.Field("coveringGas").GetValue());
                        traverse.Field("target").SetValue(traverseOld.Field("target").GetValue());
                        traverse.Field("distance").SetValue(traverseOld.Field("distance").GetValue());
                        traverse.Field("covers").SetValue((List<CoverInfo>)traverseOld.Field("covers").GetValue());
                        traverse.Field("shootLine").SetValue(__result.ShootLine);
                        __result = (ShotReport) Traverse.Create(traverse).Field("_root").GetValue();
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