using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
	
	
	internal class ScanMultipleFilter : PathFilter
	{
		private List<string> _names;

		public ScanMultipleFilter(List<string> names)
		{
			_names = names;
		}

		public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
		{
			foreach (JToken item in current)
			{
				JToken value = item;
				while (true)
				{
					JContainer container = value as JContainer;
					value = PathFilter.GetNextScanValue(item, container, value);
					if (value == null)
					{
						break;
					}
					JProperty property = value as JProperty;
					if (property != null)
					{
						foreach (string name in _names)
						{
							if (property.Name == name)
							{
								yield return property.Value;
							}
						}
					}
				}
			}
		}
	}
}
