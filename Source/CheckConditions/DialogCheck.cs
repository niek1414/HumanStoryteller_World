using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class DialogCheck : CheckCondition {
        public const String Name = "Dialog";

        private DialogResponse _response;

        public static readonly Dictionary<string, DialogResponse> dict = new Dictionary<string, DialogResponse> {
            {"Accepted", DialogResponse.Accepted},
            {"Denied", DialogResponse.Denied},
            {"Pending", DialogResponse.Pending}
        };

        public DialogCheck() {
        }

        public DialogCheck(DialogResponse response) {
            _response = Tell.AssertNotNull(response, nameof(response), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Err($"Tried to check {GetType()} but result type was null." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            if (!(result is IncidentResult_Dialog)) {
                Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_Dialog resultDialog = (IncidentResult_Dialog) result;
            return resultDialog.LetterAnswer == _response;
        }

        public override string ToString() {
            return $"Response: [{_response}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _response, "response");
        }
    }

    public enum DialogResponse {
        Accepted,
        Denied,
        Pending
    }
    
    public class IncidentResult_Dialog : IncidentResult {
        public ChoiceLetter_Dialog Letter;
        public DialogResponse LetterAnswer;

        public IncidentResult_Dialog() {
        }

        public IncidentResult_Dialog(ChoiceLetter_Dialog letter, DialogResponse letterAnswer = DialogResponse.Pending) {
            Letter = letter;
            LetterAnswer = letterAnswer;
        }

        public override string ToString() {
            return $"{base.ToString()}, Letter title: {Letter.title}, LetterAnswer: {LetterAnswer.ToString()}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref Letter, "letter");
            Scribe_Values.Look(ref LetterAnswer, "response");
        }
    }
}