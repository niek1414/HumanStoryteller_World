using System;
using HumanStoryteller.Model.StoryPart;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Nothing : HumanIncidentWorker {
    public const String Name = "Nothing";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        //What? Did you expect a huge file or something?
        
        SendLetter(@params);
        return ir;
    }
}