using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class TimeCheck : CheckCondition {
        public const String Name = "Time";
        private List<int> _hours;
        private List<int> _days;
        private List<int> _quadrums;
        private List<int> _years;

        public TimeCheck() {
        }

        public TimeCheck(List<int> hours, List<int> days, List<int> quadrums, List<int> years) {
            _hours = Tell.AssertNotNull(hours, nameof(hours), GetType().Name);
            _days = Tell.AssertNotNull(days, nameof(days), GetType().Name);
            _quadrums = Tell.AssertNotNull(quadrums, nameof(quadrums), GetType().Name);
            _years = Tell.AssertNotNull(years, nameof(years), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Map map = Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
            if (_hours.Count > 0) {
                if (!_hours.Contains(GenLocalDate.HourOfDay(map))) {
                    return false;
                }
            }
            if (_days.Count > 0) {
                if (!_days.Contains(GenLocalDate.DayOfQuadrum(map))) {
                    return false;
                }
            }
            if (_quadrums.Count > 0) {
                if (!_quadrums.Contains(QuadrumToInt(GenDate.Quadrum(GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile).x)))) {
                    return false;
                }
            }
            if (_years.Count > 0) {
                if (!_years.Contains(GenLocalDate.Year(map))) {
                    return false;
                }
            }

            return true;
        }

        private int QuadrumToInt(Quadrum q) {
            switch (q) {
                case Quadrum.Aprimay:
                    return 1;
                case Quadrum.Jugust:
                    return 2;
                case Quadrum.Septober:
                    return 3;
                case Quadrum.Decembary:
                    return 4;
                case Quadrum.Undefined:
                    return 1;
                default:
                    Tell.Err("Unknown quadrum encountered", q);
                    return 0;
            }
        }

        public override string ToString() {
            return $"Hours: {_hours}, Days: {_days}, Quadrums: {_quadrums}, Years: {_years}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref _hours, "hours");
            Scribe_Collections.Look(ref _days, "days");
            Scribe_Collections.Look(ref _quadrums, "quadrums");
            Scribe_Collections.Look(ref _years, "years");
        }
    }
}