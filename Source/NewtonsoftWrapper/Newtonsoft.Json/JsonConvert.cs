using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Provides methods for converting between common language runtime types and JSON types.
	/// </summary>
	public static class JsonConvert
	{
		/// <summary>
		/// Represents JavaScript's boolean value true as a string. This field is read-only.
		/// </summary>
		public static readonly string True = "true";

		/// <summary>
		/// Represents JavaScript's boolean value false as a string. This field is read-only.
		/// </summary>
		public static readonly string False = "false";

		/// <summary>
		/// Represents JavaScript's null as a string. This field is read-only.
		/// </summary>
		public static readonly string Null = "null";

		/// <summary>
		/// Represents JavaScript's undefined as a string. This field is read-only.
		/// </summary>
		public static readonly string Undefined = "undefined";

		/// <summary>
		/// Represents JavaScript's positive infinity as a string. This field is read-only.
		/// </summary>
		public static readonly string PositiveInfinity = "Infinity";

		/// <summary>
		/// Represents JavaScript's negative infinity as a string. This field is read-only.
		/// </summary>
		public static readonly string NegativeInfinity = "-Infinity";

		/// <summary>
		/// Represents JavaScript's NaN as a string. This field is read-only.
		/// </summary>
		public static readonly string NaN = "NaN";

		internal static readonly long InitialJavaScriptDateTicks = 621355968000000000L;

		/// <summary>
		/// Converts the <see cref="T:System.DateTime" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.DateTime" />.</returns>
		public static string ToString(DateTime value)
		{
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				WriteDateTimeString(stringWriter, value, GetUtcOffset(value), value.Kind);
				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// Converts the <see cref="T:System.DateTimeOffset" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.DateTimeOffset" />.</returns>
		public static string ToString(DateTimeOffset value)
		{
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				WriteDateTimeString(stringWriter, value.UtcDateTime, value.Offset, DateTimeKind.Local);
				return stringWriter.ToString();
			}
		}

		private static TimeSpan GetUtcOffset(DateTime dateTime)
		{
			return TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
		}

		internal static void WriteDateTimeString(TextWriter writer, DateTime value)
		{
			WriteDateTimeString(writer, value, GetUtcOffset(value), value.Kind);
		}

		internal static void WriteDateTimeString(TextWriter writer, DateTime value, TimeSpan offset, DateTimeKind kind)
		{
			long value2 = ConvertDateTimeToJavaScriptTicks(value, offset);
			writer.Write("\"\\/Date(");
			writer.Write(value2);
			switch (kind)
			{
			case DateTimeKind.Unspecified:
			case DateTimeKind.Local:
			{
				writer.Write((offset.Ticks >= 0) ? "+" : "-");
				int num = Math.Abs(offset.Hours);
				if (num < 10)
				{
					writer.Write(0);
				}
				writer.Write(num);
				int num2 = Math.Abs(offset.Minutes);
				if (num2 < 10)
				{
					writer.Write(0);
				}
				writer.Write(num2);
				break;
			}
			}
			writer.Write(")\\/\"");
		}

		private static long ToUniversalTicks(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				return dateTime.Ticks;
			}
			return ToUniversalTicks(dateTime, GetUtcOffset(dateTime));
		}

		private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				return dateTime.Ticks;
			}
			long num = dateTime.Ticks - offset.Ticks;
			if (num > 3155378975999999999L)
			{
				return 3155378975999999999L;
			}
			if (num < 0)
			{
				return 0L;
			}
			return num;
		}

		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
		{
			long universialTicks = ToUniversalTicks(dateTime, offset);
			return UniversialTicksToJavaScriptTicks(universialTicks);
		}

		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
		{
			return ConvertDateTimeToJavaScriptTicks(dateTime, convertToUtc: true);
		}

		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
		{
			long universialTicks = convertToUtc ? ToUniversalTicks(dateTime) : dateTime.Ticks;
			return UniversialTicksToJavaScriptTicks(universialTicks);
		}

		private static long UniversialTicksToJavaScriptTicks(long universialTicks)
		{
			return (universialTicks - InitialJavaScriptDateTicks) / 10000;
		}

		internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
		{
			return new DateTime(javaScriptTicks * 10000 + InitialJavaScriptDateTicks, DateTimeKind.Utc);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Boolean" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Boolean" />.</returns>
		public static string ToString(bool value)
		{
			if (!value)
			{
				return False;
			}
			return True;
		}

		/// <summary>
		/// Converts the <see cref="T:System.Char" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Char" />.</returns>
		public static string ToString(char value)
		{
			return ToString(char.ToString(value));
		}

		/// <summary>
		/// Converts the <see cref="T:System.Enum" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Enum" />.</returns>
		public static string ToString(Enum value)
		{
			return value.ToString("D");
		}

		/// <summary>
		/// Converts the <see cref="T:System.Int32" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Int32" />.</returns>
		public static string ToString(int value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Int16" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Int16" />.</returns>
		public static string ToString(short value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.UInt16" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.UInt16" />.</returns>
		[CLSCompliant(false)]
		public static string ToString(ushort value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.UInt32" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.UInt32" />.</returns>
		[CLSCompliant(false)]
		public static string ToString(uint value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Int64" />  to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Int64" />.</returns>
		public static string ToString(long value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.UInt64" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.UInt64" />.</returns>
		[CLSCompliant(false)]
		public static string ToString(ulong value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Single" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Single" />.</returns>
		public static string ToString(float value)
		{
			return EnsureDecimalPlace((double)value, value.ToString("R", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Converts the <see cref="T:System.Double" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Double" />.</returns>
		public static string ToString(double value)
		{
			return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
		}

		private static string EnsureDecimalPlace(double value, string text)
		{
			if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		private static string EnsureDecimalPlace(string text)
		{
			if (text.IndexOf('.') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		/// <summary>
		/// Converts the <see cref="T:System.Byte" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Byte" />.</returns>
		public static string ToString(byte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.SByte" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.SByte" />.</returns>
		[CLSCompliant(false)]
		public static string ToString(sbyte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Decimal" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.SByte" />.</returns>
		public static string ToString(decimal value)
		{
			return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Converts the <see cref="T:System.Guid" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Guid" />.</returns>
		public static string ToString(Guid value)
		{
			return '"' + value.ToString("D", CultureInfo.InvariantCulture) + '"';
		}

		/// <summary>
		/// Converts the <see cref="T:System.String" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
		public static string ToString(string value)
		{
			return ToString(value, '"');
		}

		/// <summary>
		/// Converts the <see cref="T:System.String" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="delimter">The string delimiter character.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
		public static string ToString(string value, char delimter)
		{
			return JavaScriptUtils.ToEscapedJavaScriptString(value, delimter, appendDelimiters: true);
		}

		/// <summary>
		/// Converts the <see cref="T:System.Object" /> to its JSON string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A JSON string representation of the <see cref="T:System.Object" />.</returns>
		public static string ToString(object value)
		{
			if (value == null)
			{
				return Null;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				switch (convertible.GetTypeCode())
				{
				case TypeCode.String:
					return ToString(convertible.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Char:
					return ToString(convertible.ToChar(CultureInfo.InvariantCulture));
				case TypeCode.Boolean:
					return ToString(convertible.ToBoolean(CultureInfo.InvariantCulture));
				case TypeCode.SByte:
					return ToString(convertible.ToSByte(CultureInfo.InvariantCulture));
				case TypeCode.Int16:
					return ToString(convertible.ToInt16(CultureInfo.InvariantCulture));
				case TypeCode.UInt16:
					return ToString(convertible.ToUInt16(CultureInfo.InvariantCulture));
				case TypeCode.Int32:
					return ToString(convertible.ToInt32(CultureInfo.InvariantCulture));
				case TypeCode.Byte:
					return ToString(convertible.ToByte(CultureInfo.InvariantCulture));
				case TypeCode.UInt32:
					return ToString(convertible.ToUInt32(CultureInfo.InvariantCulture));
				case TypeCode.Int64:
					return ToString(convertible.ToInt64(CultureInfo.InvariantCulture));
				case TypeCode.UInt64:
					return ToString(convertible.ToUInt64(CultureInfo.InvariantCulture));
				case TypeCode.Single:
					return ToString(convertible.ToSingle(CultureInfo.InvariantCulture));
				case TypeCode.Double:
					return ToString(convertible.ToDouble(CultureInfo.InvariantCulture));
				case TypeCode.DateTime:
					return ToString(convertible.ToDateTime(CultureInfo.InvariantCulture));
				case TypeCode.Decimal:
					return ToString(convertible.ToDecimal(CultureInfo.InvariantCulture));
				case TypeCode.DBNull:
					return Null;
				}
			}
			else if (value is DateTimeOffset)
			{
				return ToString((DateTimeOffset)value);
			}
			throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
		}

		private static bool IsJsonPrimitiveTypeCode(TypeCode typeCode)
		{
			switch (typeCode)
			{
			case TypeCode.DBNull:
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
			case TypeCode.DateTime:
			case TypeCode.String:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsJsonPrimitiveType(Type type)
		{
			if (ReflectionUtils.IsNullableType(type))
			{
				type = Nullable.GetUnderlyingType(type);
			}
			if (type == typeof(DateTimeOffset))
			{
				return true;
			}
			if (type == typeof(byte[]))
			{
				return true;
			}
			return IsJsonPrimitiveTypeCode(Type.GetTypeCode(type));
		}

		internal static bool IsJsonPrimitive(object value)
		{
			if (value == null)
			{
				return true;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				return IsJsonPrimitiveTypeCode(convertible.GetTypeCode());
			}
			if (value is DateTimeOffset)
			{
				return true;
			}
			if (value is byte[])
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Serializes the specified object to a JSON string.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <returns>A JSON string representation of the object.</returns>
		public static string SerializeObject(object value)
		{
			return SerializeObject(value, Formatting.None, (JsonSerializerSettings)null);
		}

		/// <summary>
		/// Serializes the specified object to a JSON string.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <returns>
		/// A JSON string representation of the object.
		/// </returns>
		public static string SerializeObject(object value, Formatting formatting)
		{
			return SerializeObject(value, formatting, (JsonSerializerSettings)null);
		}

		/// <summary>
		/// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="converters">A collection converters used while serializing.</param>
		/// <returns>A JSON string representation of the object.</returns>
		public static string SerializeObject(object value, params JsonConverter[] converters)
		{
			return SerializeObject(value, Formatting.None, converters);
		}

		/// <summary>
		/// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <param name="converters">A collection converters used while serializing.</param>
		/// <returns>A JSON string representation of the object.</returns>
		public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
		{
			object obj;
			if (converters == null || converters.Length <= 0)
			{
				obj = null;
			}
			else
			{
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
				jsonSerializerSettings.Converters = converters;
				obj = jsonSerializerSettings;
			}
			JsonSerializerSettings settings = (JsonSerializerSettings)obj;
			return SerializeObject(value, formatting, settings);
		}

		/// <summary>
		/// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
		/// If this is null, default serialization settings will be is used.</param>
		/// <returns>
		/// A JSON string representation of the object.
		/// </returns>
		public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			StringBuilder sb = new StringBuilder(128);
			StringWriter stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture);
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
			{
				jsonTextWriter.Formatting = formatting;
				jsonSerializer.Serialize(jsonTextWriter, value);
			}
			return stringWriter.ToString();
		}

		/// <summary>
		/// Deserializes the specified object to a Json object.
		/// </summary>
		/// <param name="value">The object to deserialize.</param>
		/// <returns>The deserialized object from the Json string.</returns>
		public static object DeserializeObject(string value)
		{
			return DeserializeObject(value, (Type)null, (JsonSerializerSettings)null);
		}

		/// <summary>
		/// Deserializes the specified object to a Json object.
		/// </summary>
		/// <param name="value">The object to deserialize.</param>
		/// <param name="type">The <see cref="T:System.Type" /> of object being deserialized.</param>
		/// <returns>The deserialized object from the Json string.</returns>
		public static object DeserializeObject(string value, Type type)
		{
			return DeserializeObject(value, type, (JsonSerializerSettings)null);
		}

		/// <summary>
		/// Deserializes the specified object to a Json object.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize.</typeparam>
		/// <param name="value">The object to deserialize.</param>
		/// <returns>The deserialized object from the Json string.</returns>
		public static T DeserializeObject<T>(string value)
		{
			return DeserializeObject<T>(value, (JsonSerializerSettings)null);
		}

		/// <summary>
		/// Deserializes the specified JSON to the given anonymous type.
		/// </summary>
		/// <typeparam name="T">
		/// The anonymous type to deserialize to. This can't be specified
		/// traditionally and must be infered from the anonymous type passed
		/// as a parameter.
		/// </typeparam>
		/// <param name="value">The object to deserialize.</param>
		/// <param name="anonymousTypeObject">The anonymous type object.</param>
		/// <returns>The deserialized anonymous type from the JSON string.</returns>
		public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
		{
			return DeserializeObject<T>(value);
		}

		/// <summary>
		/// Deserializes the JSON string to the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize.</typeparam>
		/// <param name="value">The object to deserialize.</param>
		/// <param name="converters">Converters to use while deserializing.</param>
		/// <returns>The deserialized object from the JSON string.</returns>
		public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
		{
			return (T)DeserializeObject(value, typeof(T), converters);
		}

		/// <summary>
		/// Deserializes the JSON string to the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize.</typeparam>
		/// <param name="value">The object to deserialize.</param>
		/// <param name="settings">
		/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
		/// If this is null, default serialization settings will be is used.
		/// </param>
		/// <returns>The deserialized object from the JSON string.</returns>
		public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
		{
			return (T)DeserializeObject(value, typeof(T), settings);
		}

		/// <summary>
		/// Deserializes the JSON string to the specified type.
		/// </summary>
		/// <param name="value">The object to deserialize.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <param name="converters">Converters to use while deserializing.</param>
		/// <returns>The deserialized object from the JSON string.</returns>
		public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
		{
			object obj;
			if (converters == null || converters.Length <= 0)
			{
				obj = null;
			}
			else
			{
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
				jsonSerializerSettings.Converters = converters;
				obj = jsonSerializerSettings;
			}
			JsonSerializerSettings settings = (JsonSerializerSettings)obj;
			return DeserializeObject(value, type, settings);
		}

		/// <summary>
		/// Deserializes the JSON string to the specified type.
		/// </summary>
		/// <param name="value">The JSON to deserialize.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <param name="settings">
		/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
		/// If this is null, default serialization settings will be is used.
		/// </param>
		/// <returns>The deserialized object from the JSON string.</returns>
		public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
		{
			StringReader reader = new StringReader(value);
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			using (JsonReader jsonReader = new JsonTextReader(reader))
			{
				object result = jsonSerializer.Deserialize(jsonReader, type);
				if (!jsonReader.Read())
				{
					return result;
				}
				if (jsonReader.TokenType != JsonToken.Comment)
				{
					throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
				}
				return result;
			}
		}

		/// <summary>
		/// Populates the object with values from the JSON string.
		/// </summary>
		/// <param name="value">The JSON to populate values from.</param>
		/// <param name="target">The target object to populate values onto.</param>
		public static void PopulateObject(string value, object target)
		{
			PopulateObject(value, target, null);
		}

		/// <summary>
		/// Populates the object with values from the JSON string.
		/// </summary>
		/// <param name="value">The JSON to populate values from.</param>
		/// <param name="target">The target object to populate values onto.</param>
		/// <param name="settings">
		/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
		/// If this is null, default serialization settings will be is used.
		/// </param>
		public static void PopulateObject(string value, object target, JsonSerializerSettings settings)
		{
			StringReader reader = new StringReader(value);
			JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
			using (JsonReader jsonReader = new JsonTextReader(reader))
			{
				jsonSerializer.Populate(jsonReader, target);
				if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
				{
					throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
				}
			}
		}

		/// <summary>
		/// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string.
		/// </summary>
		/// <param name="node">The node to convert to JSON.</param>
		/// <returns>A JSON string of the XNode.</returns>
		public static string SerializeXNode(XObject node)
		{
			return SerializeXNode(node, Formatting.None);
		}

		/// <summary>
		/// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string.
		/// </summary>
		/// <param name="node">The node to convert to JSON.</param>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <returns>A JSON string of the XNode.</returns>
		public static string SerializeXNode(XObject node, Formatting formatting)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			return SerializeObject(node, formatting, xmlNodeConverter);
		}

		/// <summary>
		/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string.
		/// </summary>
		/// <param name="value">The JSON string.</param>
		/// <returns>The deserialized XNode</returns>
		public static XDocument DeserializeXNode(string value)
		{
			return DeserializeXNode(value, null);
		}

		/// <summary>
		/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root elment.
		/// </summary>
		/// <param name="value">The JSON string.</param>
		/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
		/// <returns>The deserialized XNode</returns>
		public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
			return (XDocument)DeserializeObject(value, typeof(XDocument), xmlNodeConverter);
		}

		/// <summary>
		/// Serializes the XML node to a JSON string.
		/// </summary>
		/// <param name="node">The node to serialize.</param>
		/// <returns>A JSON string of the XmlNode.</returns>
		public static string SerializeXmlNode(XmlNode node)
		{
			return SerializeXmlNode(node, Formatting.None);
		}

		/// <summary>
		/// Serializes the XML node to a JSON string.
		/// </summary>
		/// <param name="node">The node to serialize.</param>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <returns>A JSON string of the XmlNode.</returns>
		public static string SerializeXmlNode(XmlNode node, Formatting formatting)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			return SerializeObject(node, formatting, xmlNodeConverter);
		}

		/// <summary>
		/// Deserializes the XmlNode from a JSON string.
		/// </summary>
		/// <param name="value">The JSON string.</param>
		/// <returns>The deserialized XmlNode</returns>
		public static XmlDocument DeserializeXmlNode(string value)
		{
			return DeserializeXmlNode(value, null);
		}

		/// <summary>
		/// Deserializes the XmlNode from a JSON string nested in a root elment.
		/// </summary>
		/// <param name="value">The JSON string.</param>
		/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
		/// <returns>The deserialized XmlNode</returns>
		public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
		{
			XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
			xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
			return (XmlDocument)DeserializeObject(value, typeof(XmlDocument), xmlNodeConverter);
		}
	}
}
