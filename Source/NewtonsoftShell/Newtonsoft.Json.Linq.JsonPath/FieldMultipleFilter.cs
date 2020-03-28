using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Newtonsoft.Json.Linq.JsonPath
{
	
	
	internal class FieldMultipleFilter : PathFilter
	{
		internal List<string> Names;

		public FieldMultipleFilter(List<string> names)
		{
			Names = names;
		}

		public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
		{
			foreach (JToken item in current)
			{
				JObject o = item as JObject;
				if (o != null)
				{
					foreach (string name in Names)
					{
						JToken jToken = o[name];
						if (jToken != null)
						{
							yield return jToken;
						}
						if (errorWhenNoMatch)
						{
							throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, name));
						}
					}
				}
				else if (errorWhenNoMatch)
				{
					throw new JsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", from n in Names
					select "'" + n + "'"), item.GetType().Name));
				}
			}
		}
	}
}
