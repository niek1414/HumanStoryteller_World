using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class DialogCheck : CheckCondition {
        public const String Name = "Dialog";
        
        private DialogResponse _response;
        
        public static readonly Dictionary<string, DialogResponse> dict = new Dictionary<string, DialogResponse>
        {
            {"Accepted", DialogResponse.Accepted},
            {"Denied", DialogResponse.Denied},
            {"Pending", DialogResponse.Pending}
        };

        public DialogCheck() {
        }

        public DialogCheck(DialogResponse response) {
            _response = Tell.AssertNotNull(response, nameof(response), GetType().Name);
        }

        public override bool Check(IncidentResult result) {
            if (!(result is IncidentResult_Dialog)) {
                Tell.Err("Tried to check " + GetType() + " but result type was " + result.GetType());
                return false;
            }

            IncidentResult_Dialog resultDialog = (IncidentResult_Dialog) result;
            return resultDialog.LetterAnswer == _response;//TODO AND ALSO: add to parser
        }

        public override string ToString() {
            return $"Response: {_response}";
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
}