using System.Collections.Generic;
using System.Globalization;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq.JsonPath;

	
	
	internal class FieldFilter : PathFilter
	{
		internal string Name;

		public FieldFilter(string name)
		{
			Name = name;
		}

		
		public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
		{
			foreach (JToken item in current)
			{
				JObject jObject = item as JObject;
				if (jObject != null)
				{
					if (Name != null)
					{
						JToken jToken = jObject[Name];
						if (jToken != null)
						{
							yield return jToken;
						}
						else if (errorWhenNoMatch)
						{
							throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, Name));
						}
					}
					else
					{
						foreach (KeyValuePair<string, JToken> item2 in jObject)
						{
							yield return item2.Value;
						}
					}
				}
				else if (errorWhenNoMatch)
				{
					throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, Name ?? "*", item.GetType().Name));
				}
			}
		}
	}
