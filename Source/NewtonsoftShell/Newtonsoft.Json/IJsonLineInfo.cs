namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json
{
	public interface IJsonLineInfo
	{
		int LineNumber
		{
			get;
		}

		int LinePosition
		{
			get;
		}

		bool HasLineInfo();
	}
}
