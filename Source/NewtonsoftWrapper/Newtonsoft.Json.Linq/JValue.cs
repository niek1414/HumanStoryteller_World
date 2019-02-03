using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a value in JSON (string, integer, date, etc).
	/// </summary>
	public class JValue : JToken, IEquatable<JValue>
	{
		private JTokenType _valueType;

		private object _value;

		/// <summary>
		/// Gets a value indicating whether this token has childen tokens.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
		/// </value>
		public override bool HasValues => false;

		/// <summary>
		/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <value>The type.</value>
		public override JTokenType Type => _valueType;

		/// <summary>
		/// Gets or sets the underlying token value.
		/// </summary>
		/// <value>The underlying token value.</value>
		public new object Value
		{
			get
			{
				return _value;
			}
			set
			{
				Type type = (_value != null) ? _value.GetType() : null;
				Type type2 = value?.GetType();
				if (type != type2)
				{
					_valueType = GetValueType(_valueType, value);
				}
				_value = value;
			}
		}

		internal JValue(object value, JTokenType type)
		{
			_value = value;
			_valueType = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class from another <see cref="T:Newtonsoft.Json.Linq.JValue" /> object.
		/// </summary>
		/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JValue" /> object to copy from.</param>
		public JValue(JValue other)
			: this(other.Value, other.Type)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(long value)
			: this(value, JTokenType.Integer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		[CLSCompliant(false)]
		public JValue(ulong value)
			: this(value, JTokenType.Integer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(double value)
			: this(value, JTokenType.Float)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(DateTime value)
			: this(value, JTokenType.Date)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(bool value)
			: this(value, JTokenType.Boolean)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(string value)
			: this(value, JTokenType.String)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JValue" /> class with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		public JValue(object value)
			: this(value, GetValueType(null, value))
		{
		}

		internal override bool DeepEquals(JToken node)
		{
			JValue jValue = node as JValue;
			if (jValue == null)
			{
				return false;
			}
			return ValuesEquals(this, jValue);
		}

		private static bool Compare(JTokenType valueType, object objA, object objB)
		{
			if (objA == null && objB == null)
			{
				return true;
			}
			if (objA != null && objB != null)
			{
				switch (valueType)
				{
				case JTokenType.Integer:
					if (objA is ulong || objB is ulong)
					{
						return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
					}
					return Convert.ToInt64(objA, CultureInfo.InvariantCulture).Equals(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
				case JTokenType.Float:
					return Convert.ToDouble(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDouble(objB, CultureInfo.InvariantCulture));
				case JTokenType.Comment:
				case JTokenType.String:
				case JTokenType.Boolean:
				case JTokenType.Raw:
					return objA.Equals(objB);
				case JTokenType.Date:
					return objA.Equals(objB);
				case JTokenType.Bytes:
				{
					byte[] array = objA as byte[];
					byte[] array2 = objB as byte[];
					if (array == null || array2 == null)
					{
						return false;
					}
					return MiscellaneousUtils.ByteArrayCompare(array, array2);
				}
				default:
					throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
				}
			}
			return false;
		}

		internal override JToken CloneToken()
		{
			return new JValue(this);
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JValue" /> comment with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JValue" /> comment with the given value.</returns>
		public static JValue CreateComment(string value)
		{
			return new JValue(value, JTokenType.Comment);
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JValue" /> string with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JValue" /> string with the given value.</returns>
		public static JValue CreateString(string value)
		{
			return new JValue(value, JTokenType.String);
		}

		private static JTokenType GetValueType(JTokenType? current, object value)
		{
			if (value == null)
			{
				return JTokenType.Null;
			}
			if (value == DBNull.Value)
			{
				return JTokenType.Null;
			}
			if (value is string)
			{
				return GetStringValueType(current);
			}
			if (value is long || value is int || value is short || value is sbyte || value is ulong || value is uint || value is ushort || value is byte)
			{
				return JTokenType.Integer;
			}
			if (value is Enum)
			{
				return JTokenType.Integer;
			}
			if (value is double || value is float || value is decimal)
			{
				return JTokenType.Float;
			}
			if (value is DateTime)
			{
				return JTokenType.Date;
			}
			if (value is DateTimeOffset)
			{
				return JTokenType.Date;
			}
			if (value is byte[])
			{
				return JTokenType.Bytes;
			}
			if (value is bool)
			{
				return JTokenType.Boolean;
			}
			throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
		}

		private static JTokenType GetStringValueType(JTokenType? current)
		{
			if (!current.HasValue)
			{
				return JTokenType.String;
			}
			JTokenType value = current.Value;
			if (value == JTokenType.Comment || value == JTokenType.String || value == JTokenType.Raw)
			{
				return current.Value;
			}
			return JTokenType.String;
		}

		/// <summary>
		/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			switch (_valueType)
			{
			case JTokenType.Comment:
				writer.WriteComment(_value.ToString());
				break;
			case JTokenType.Raw:
				writer.WriteRawValue((_value != null) ? _value.ToString() : null);
				break;
			case JTokenType.Null:
				writer.WriteNull();
				break;
			case JTokenType.Undefined:
				writer.WriteUndefined();
				break;
			default:
			{
				JsonConverter matchingConverter;
				if (_value == null || (matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value.GetType())) == null)
				{
					switch (_valueType)
					{
					case JTokenType.Integer:
						writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
						break;
					case JTokenType.Float:
						writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
						break;
					case JTokenType.String:
						writer.WriteValue((_value != null) ? _value.ToString() : null);
						break;
					case JTokenType.Boolean:
						writer.WriteValue(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
						break;
					case JTokenType.Date:
						if (_value is DateTimeOffset)
						{
							writer.WriteValue((DateTimeOffset)_value);
						}
						else
						{
							writer.WriteValue(Convert.ToDateTime(_value, CultureInfo.InvariantCulture));
						}
						break;
					case JTokenType.Bytes:
						writer.WriteValue((byte[])_value);
						break;
					default:
						throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", _valueType, "Unexpected token type.");
					}
				}
				else
				{
					matchingConverter.WriteJson(writer, _value, new JsonSerializer());
				}
				break;
			}
			}
		}

		internal override int GetDeepHashCode()
		{
			int num = (_value != null) ? _value.GetHashCode() : 0;
			return _valueType.GetHashCode() ^ num;
		}

		private static bool ValuesEquals(JValue v1, JValue v2)
		{
			if (v1 != v2)
			{
				if (v1._valueType == v2._valueType)
				{
					return Compare(v1._valueType, v1._value, v2._value);
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(JValue other)
		{
			if (other == null)
			{
				return false;
			}
			return ValuesEquals(this, other);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj" /> parameter is null.
		/// </exception>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			JValue jValue = obj as JValue;
			if (jValue != null)
			{
				return Equals(jValue);
			}
			return base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object" />.
		/// </returns>
		public override int GetHashCode()
		{
			if (_value == null)
			{
				return 0;
			}
			return _value.GetHashCode();
		}
	}
}
