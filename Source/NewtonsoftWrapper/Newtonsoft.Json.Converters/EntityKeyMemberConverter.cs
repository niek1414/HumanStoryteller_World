using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json.Converters
{
	/// <summary>
	/// Converts an Entity Framework EntityKey to and from JSON.
	/// </summary>
	public class EntityKeyMemberConverter : JsonConverter
	{
		private const string EntityKeyMemberFullTypeName = "System.Data.EntityKeyMember";

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(value);
			Type type = (entityKeyMember.Value != null) ? entityKeyMember.Value.GetType() : null;
			writer.WriteStartObject();
			writer.WritePropertyName("Key");
			writer.WriteValue(entityKeyMember.Key);
			writer.WritePropertyName("Type");
			writer.WriteValue(type?.FullName);
			writer.WritePropertyName("Value");
			if (type != null)
			{
				if (JsonSerializerInternalWriter.TryConvertToString(entityKeyMember.Value, type, out string s))
				{
					writer.WriteValue(s);
				}
				else
				{
					writer.WriteValue(entityKeyMember.Value);
				}
			}
			else
			{
				writer.WriteNull();
			}
			writer.WriteEndObject();
		}

		private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
		{
			ReadAndAssert(reader);
			if (reader.TokenType != JsonToken.PropertyName || reader.Value.ToString() != propertyName)
			{
				throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
			}
		}

		private static void ReadAndAssert(JsonReader reader)
		{
			if (!reader.Read())
			{
				throw new JsonSerializationException("Unexpected end.");
			}
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
			IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(Activator.CreateInstance(objectType));
			ReadAndAssertProperty(reader, "Key");
			ReadAndAssert(reader);
			entityKeyMember.Key = reader.Value.ToString();
			ReadAndAssertProperty(reader, "Type");
			ReadAndAssert(reader);
			string typeName = reader.Value.ToString();
			Type type = Type.GetType(typeName);
			ReadAndAssertProperty(reader, "Value");
			ReadAndAssert(reader);
			entityKeyMember.Value = serializer.Deserialize(reader, type);
			ReadAndAssert(reader);
			return DynamicWrapper.GetUnderlyingObject(entityKeyMember);
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
			return objectType.AssignableToTypeName("System.Data.EntityKeyMember");
		}
	}
}
