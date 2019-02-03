using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json.Converters
{
	/// <summary>
	/// Converts a binary value to and from a base 64 string value.
	/// </summary>
	public class BinaryConverter : JsonConverter
	{
		private const string BinaryTypeName = "System.Data.Linq.Binary";

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				byte[] byteArray = GetByteArray(value);
				writer.WriteValue(byteArray);
			}
		}

		private byte[] GetByteArray(object value)
		{
			if (value.GetType().AssignableToTypeName("System.Data.Linq.Binary"))
			{
				IBinary binary = DynamicWrapper.CreateWrapper<IBinary>(value);
				return binary.ToArray();
			}
//			if (value is SqlBinary)
//			{
//				return ((SqlBinary)value).Value;
//			}
			throw new Exception("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
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
			Type type = ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
			if (reader.TokenType == JsonToken.Null)
			{
				if (!ReflectionUtils.IsNullable(objectType))
				{
					throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
				}
				return null;
			}
			if (reader.TokenType != JsonToken.String)
			{
				throw new Exception("Unexpected token parsing binary. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			string s = reader.Value.ToString();
			byte[] array = Convert.FromBase64String(s);
			if (type.AssignableToTypeName("System.Data.Linq.Binary"))
			{
				return Activator.CreateInstance(type, array);
			}
//			if (type == typeof(SqlBinary))
//			{
//				return new SqlBinary(array);
//			}
			throw new Exception("Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
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
			if (objectType.AssignableToTypeName("System.Data.Linq.Binary"))
			{
				return true;
			}
//			if (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?))
//			{
//				return true;
//			}
			return false;
		}
	}
}
