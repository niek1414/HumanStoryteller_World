using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization;

	public class SnakeCaseNamingStrategy : NamingStrategy
	{
		public SnakeCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
		{
			base.ProcessDictionaryKeys = processDictionaryKeys;
			base.OverrideSpecifiedNames = overrideSpecifiedNames;
		}

		public SnakeCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
			: this(processDictionaryKeys, overrideSpecifiedNames)
		{
			base.ProcessExtensionDataNames = processExtensionDataNames;
		}

		public SnakeCaseNamingStrategy()
		{
		}

		
		protected override string ResolvePropertyName(string name)
		{
			return StringUtils.ToSnakeCase(name);
		}
	}
