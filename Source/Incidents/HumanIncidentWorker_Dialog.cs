using System;
using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Dialog : HumanIncidentWorker {
        public const String Name = "Dialog";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Dialog)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Dialog
                allParams = Tell.AssertNotNull((HumanIncidentParams_Dialog) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            string title = "todo (more info inside)";
            string message = "This event should be customized in the mail tab of the event";
            LetterDef type = LetterDefOf.NeutralEvent;

            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }
                title = parms.Letter.Title;
                message = parms.Letter.Message;
                type = parms.Letter.Type;
            }

            Letter l = LetterMaker.MakeLetter(title, message, type);
            ChoiceLetter_Dialog choiceLetter_Dialog = new ChoiceLetter_Dialog {
                ID = l.ID,
                def = l.def,
                label = l.label,
                lookTargets = l.lookTargets,
                relatedFaction = l.relatedFaction,
                arrivalTick = l.arrivalTick,
                arrivalTime = l.arrivalTime,
                debugInfo = l.debugInfo,
                text = message,
                title = title,
                radioMode = true,
                map = map,
                fee = Mathf.RoundToInt(allParams.Silver.GetValue())
            };
            choiceLetter_Dialog.report = new IncidentResult_Dialog(choiceLetter_Dialog);
            var duration = allParams.Duration.GetValue();
            if (duration > 0) {
                choiceLetter_Dialog.StartTimeout(Mathf.RoundToInt(60000 * duration));
            }

            Find.LetterStack.ReceiveLetter(choiceLetter_Dialog);

            return choiceLetter_Dialog.report;
        }
    }

    public class HumanIncidentParams_Dialog : HumanIncidentParms {
        public Number Silver;
        public Number Duration;

        public HumanIncidentParams_Dialog() {
        }

        public HumanIncidentParams_Dialog(String target, HumanLetter letter, Number silver, Number duration) : base(target, letter) {
            Silver = silver;
            Duration = duration;
        }
        
        public HumanIncidentParams_Dialog(String target, HumanLetter letter) : this(target, letter, new Number(0), new Number(1)) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Silver: {Silver}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Silver, "silver");
            Scribe_Deep.Look(ref Duration, "duration");
        }
    }
    
    public class ChoiceLetter_Dialog : ChoiceLetter {
        public Map map;
        public int fee;
        public IncidentResult_Dialog report;

        public override IEnumerable<DiaOption> Choices {
            get {
                if (!ArchivedOnly) {
                    DiaOption accept =
                        new DiaOption("RansomDemand_Accept".Translate() + (fee == 0 ? "" : " (" + fee + " " + ThingDefOf.Silver.label + ")")) {
                            action = delegate {
                                report.LetterAnswer = DialogResponse.Accepted;
                                TradeUtility.LaunchSilver(map, fee);
                                Find.LetterStack.RemoveLetter(this);
                            },
                            resolveTree = true
                        };
                    if (!TradeUtility.ColonyHasEnoughSilver(map, fee)) {
                        accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
                    }

                    yield return accept;
                }

                yield return new DiaOption("Close".Translate()) {
                    action = () => {
                        report.LetterAnswer = DialogResponse.Denied;
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };

                yield return Option_Postpone;
            }
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(map);

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref report, "report");
            Scribe_Values.Look(ref fee, "fee");
        }
    }
}