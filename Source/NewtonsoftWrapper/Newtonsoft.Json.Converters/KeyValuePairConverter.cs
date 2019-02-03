using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Converters
{
	/// <summary>
	/// Converts a <see cref="T:System.Collections.Generic.KeyValuePair`2" /> to and from JSON.
	/// </summary>
	public class KeyValuePairConverter : JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Type type = value.GetType();
			PropertyInfo property = type.GetProperty("Key");
			PropertyInfo property2 = type.GetProperty("Value");
			writer.WriteStartObject();
			writer.WritePropertyName("Key");
			serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property, value));
			writer.WritePropertyName("Value");
			serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property2, value));
			writer.WriteEndObject();
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			IList<Type> genericArguments = objectType.GetGenericArguments();
			Type objectType2 = genericArguments[0];
			Type objectType3 = genericArguments[1];
			reader.Read();
			reader.Read();
			object obj = serializer.Deserialize(reader, objectType2);
			reader.Read();
			reader.Read();
			object obj2 = serializer.Deserialize(reader, objectType3);
			reader.Read();
			return ReflectionUtils.CreateInstance(objectType, obj, obj2);
		}

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type objectType)
		{
			if (objectType.IsValueType && objectType.IsGenericType)
			{
				return objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<, >);
			}
			return false;
		}
	}
}
