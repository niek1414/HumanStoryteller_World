using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json.Converters
{
	/// <summary>
	/// Converts a <see cref="T:System.DateTime" /> to and from a JavaScript date constructor (e.g. new Date(52231943)).
	/// </summary>
	public class JavaScriptDateTimeConverter : DateTimeConverterBase
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			long value2;
			if (value is DateTime)
			{
				DateTime dateTime = ((DateTime)value).ToUniversalTime();
				value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime);
			}
			else
			{
				if (!(value is DateTimeOffset))
				{
					throw new Exception("Expected date object value.");
				}
				value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(((DateTimeOffset)value).ToUniversalTime().UtcDateTime);
			}
			writer.WriteStartConstructor("Date");
			writer.WriteValue(value2);
			writer.WriteEndConstructor();
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type type = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
			if (reader.TokenType == JsonToken.Null)
			{
				if (!ReflectionUtils.IsNullableType(objectType))
				{
					throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
				}
				return null;
			}
			if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
			{
				throw new Exception("Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
			}
			reader.Read();
			if (reader.TokenType != JsonToken.Integer)
			{
				throw new Exception("Unexpected token parsing date. Expected Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			long javaScriptTicks = (long)reader.Value;
			DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
			reader.Read();
			if (reader.TokenType != JsonToken.EndConstructor)
			{
				throw new Exception("Unexpected token parsing date. Expected EndConstructor, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			if (type == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(dateTime);
			}
			return dateTime;
		}
	}
}
