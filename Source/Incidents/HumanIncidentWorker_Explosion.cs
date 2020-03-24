using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Explosion : HumanIncidentWorker {
        public const String Name = "Explosion";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Explosion)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Explosion allParams =
                Tell.AssertNotNull((HumanIncidentParams_Explosion) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            DamageDef damageDef = DefDatabase<DamageDef>.GetNamedSilentFail(allParams.ExplosionType);
            if (damageDef == null) {
                Tell.Warn("Could not find damageDef of type " + allParams.ExplosionType + " defaulting to bomb");
                damageDef = DamageDefOf.Bomb;
            }

            GenExplosion.DoExplosion(allParams.Location.GetSingleCell(map), map, allParams.Radius.GetValue(), damageDef, null);

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_Explosion : HumanIncidentParms {
        public string ExplosionType = "";
        public Number Radius = new Number(3.9f);
        public Location Location = new Location();

        public HumanIncidentParams_Explosion() {
        }

        public HumanIncidentParams_Explosion(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, ExplosionType: [{ExplosionType}], Location: [{Location}], Radius: [{Radius}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref ExplosionType, "explosionType");
            Scribe_Deep.Look(ref Radius, "radius");
            Scribe_Deep.Look(ref Location, "location");
        }
    }
}