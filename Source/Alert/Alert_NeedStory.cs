using Verse;

namespace HumanStoryteller.Alert
{
	public class Alert_NeedStory : RimWorld.Alert
	{
		public Alert_NeedStory()
		{
			defaultLabel = Translator.Translate("AlertNeedStorySource");
			defaultExplanation = Translator.Translate("AlertNeedStoryDesc");
			defaultPriority = RimWorld.AlertPriority.Critical;
		}

		public override RimWorld.AlertReport GetReport()
		{
			return HumanStoryteller.IsNoStory && HumanStoryteller.HumanStorytellerGame;
		}
	}
}
