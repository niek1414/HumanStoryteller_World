using System.Collections.Generic;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq.JsonPath;

	
	
	internal class QueryScanFilter : PathFilter
	{
		internal QueryExpression Expression;

		public QueryScanFilter(QueryExpression expression)
		{
			Expression = expression;
		}

		public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
		{
			foreach (JToken item in current)
			{
				JContainer jContainer = item as JContainer;
				if (jContainer != null)
				{
					foreach (JToken item2 in jContainer.DescendantsAndSelf())
					{
						if (Expression.IsMatch(root, item2))
						{
							yield return item2;
						}
					}
				}
				else if (Expression.IsMatch(root, item))
				{
					yield return item;
				}
			}
		}
	}
