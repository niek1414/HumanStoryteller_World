using Verse;

namespace HumanStoryteller.Util {
    public class Number : IExposable {
        private float _value;
        private string _variable;
        private bool isVariable;

        public Number() {
            _value = -1;
            isVariable = false;
        }

        public Number(float value = -1) {
            _value = value;
            isVariable = false;
        }

        public Number(string variable) {
            _variable = variable;
            isVariable = true;
        }

        public float GetValue() {
            return isVariable ? DataBank.GetValueFromVariable(_variable) : _value;
        }

        public override string ToString() {
            return "Nmbr: " + GetValue() + (isVariable ? "var: " + _variable + " " : " ");
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _value, "value");
            Scribe_Values.Look(ref _variable, "variable");
            Scribe_Values.Look(ref isVariable, "isVariable");
        }
    }
}