using System;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;

	public abstract class DateTimeConverterBase : JsonConverter
	{
		
		public override bool CanConvert(Type objectType)
		{
			if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
			{
				return true;
			}
			if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
			{
				return true;
			}
			return false;
		}
	}
