using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.DebugConnection.Outgoing; 
public class DataBanks : Message {
    public List<Variable> Variables { get; set; }
    public List<Map> Maps { get; set; }
    public List<PawnGroup> PawnGroups { get; set; }
    public List<PawnObj> Pawns { get; set; }

    public DataBanks() : base(MessageType.DataBanks) {
    }

    public DataBanks(StoryComponent sc) : this() {
        Variables = sc.VariableBank.Select(pair => new Variable(pair.Key, pair.Value)).ToList();
        Variables.Add(new Variable("_DAYS", DataBankUtil.GetDaysPassed()));
        Variables.Add(new Variable("_TREAT_POINTS", DataBankUtil.GetThreatPoints()));
        Variables.Add(new Variable("_WEALTH", DataBankUtil.GetWealth()));

        Maps = sc.MapBank.Select(pair => new Map(pair.Key, pair.Value.Parent.Tile)).ToList();
        PawnGroups = sc.PawnGroupBank.Select(pair => new PawnGroup(pair.Key, pair.Value.Pawns.Select(p => p.LabelShort).ToList())).ToList();
        Pawns = sc.PawnBank.Select(pair => new PawnObj(pair.Key, pair.Value.LabelShort)).ToList();
    }

    public override string ToString() {
        return
            $"Variables: [{Variables.ToStringSafeEnumerable()}], Maps: [{Maps.ToStringSafeEnumerable()}], PawnGroups: [{PawnGroups.ToStringSafeEnumerable()}]";
    }

    public class PawnObj {
        public string Name;
        public string Label;

        public PawnObj(string name, string label) {
            Name = name;
            Label = label;
        }

        public override string ToString() {
            return $"Name: [{Name}], Label: [{Label}]";
        }
    }

    public class PawnGroup {
        public string Name;
        public List<string> Pawns;

        public PawnGroup(string name, List<string> pawns) {
            Name = name;
            Pawns = pawns;
        }

        public override string ToString() {
            return $"Name: [{Name}], Pawns: [{Pawns.ToStringSafeEnumerable()}]";
        }
    }

    public class Map {
        public string Name;
        public int Tile;

        public Map(string name, int tile) {
            Name = name;
            Tile = tile;
        }

        public override string ToString() {
            return $"Name: [{Name}], Tile: [{Tile}]";
        }
    }

    public class Variable {
        public string Name;
        public float Value;

        public Variable(string name, float value) {
            Name = name;
            Value = value;
        }

        public override string ToString() {
            return $"Name: [{Name}], Value: [{Value}]";
        }
    }
}