using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents an abstract JSON token.
	/// </summary>
	public abstract class JToken : IJEnumerable<JToken>, IEnumerable<JToken>, IEnumerable, IJsonLineInfo
	{
		private JContainer _parent;

		internal JToken _next;

		private static JTokenEqualityComparer _equalityComparer;

		private int? _lineNumber;

		private int? _linePosition;

		/// <summary>
		/// Gets a comparer that can compare two tokens for value equality.
		/// </summary>
		/// <value>A <see cref="T:Newtonsoft.Json.Linq.JTokenEqualityComparer" /> that can compare two nodes for value equality.</value>
		public static JTokenEqualityComparer EqualityComparer
		{
			get
			{
				if (_equalityComparer == null)
				{
					_equalityComparer = new JTokenEqualityComparer();
				}
				return _equalityComparer;
			}
		}

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public JContainer Parent
		{
			[DebuggerStepThrough]
			get
			{
				return _parent;
			}
			internal set
			{
				_parent = value;
			}
		}

		/// <summary>
		/// Gets the root <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <value>The root <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
		public JToken Root
		{
			get
			{
				JContainer parent = Parent;
				if (parent == null)
				{
					return this;
				}
				while (parent.Parent != null)
				{
					parent = parent.Parent;
				}
				return parent;
			}
		}

		/// <summary>
		/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <value>The type.</value>
		public abstract JTokenType Type
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this token has childen tokens.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
		/// </value>
		public abstract bool HasValues
		{
			get;
		}

		/// <summary>
		/// Gets the next sibling token of this node.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the next sibling token.</value>
		public JToken Next
		{
			get
			{
				if (_parent != null && _next != _parent.First)
				{
					return _next;
				}
				return null;
			}
			internal set
			{
				_next = value;
			}
		}

		/// <summary>
		/// Gets the previous sibling token of this node.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the previous sibling token.</value>
		public JToken Previous
		{
			get
			{
				if (_parent == null)
				{
					return null;
				}
				JToken next = _parent.Content._next;
				JToken result = null;
				while (next != this)
				{
					result = next;
					next = next.Next;
				}
				return result;
			}
		}

		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
		public virtual JToken this[object key]
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
			set
			{
				throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		/// <summary>
		/// Get the first child token of this token.
		/// </summary>
		/// <value>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the first child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
		public virtual JToken First
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		/// <summary>
		/// Get the last child token of this token.
		/// </summary>
		/// <value>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the last child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
		public virtual JToken Last
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		IJEnumerable<JToken> IJEnumerable<JToken>.this[object key]
		{
			get
			{
				return this[key];
			}
		}

		int IJsonLineInfo.LineNumber
		{
			get
			{
				return _lineNumber ?? 0;
			}
		}

		int IJsonLineInfo.LinePosition
		{
			get
			{
				return _linePosition ?? 0;
			}
		}

		internal abstract JToken CloneToken();

		internal abstract bool DeepEquals(JToken node);

		/// <summary>
		/// Compares the values of two tokens, including the values of all descendant tokens.
		/// </summary>
		/// <param name="t1">The first <see cref="T:Newtonsoft.Json.Linq.JToken" /> to compare.</param>
		/// <param name="t2">The second <see cref="T:Newtonsoft.Json.Linq.JToken" /> to compare.</param>
		/// <returns>true if the tokens are equal; otherwise false.</returns>
		public static bool DeepEquals(JToken t1, JToken t2)
		{
			if (t1 != t2)
			{
				if (t1 != null && t2 != null)
				{
					return t1.DeepEquals(t2);
				}
				return false;
			}
			return true;
		}

		internal JToken()
		{
		}

		/// <summary>
		/// Adds the specified content immediately after this token.
		/// </summary>
		/// <param name="content">A content object that contains simple content or a collection of content objects to be added after this token.</param>
		public void AddAfterSelf(object content)
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			_parent.AddInternal(Next == null, this, content);
		}

		/// <summary>
		/// Adds the specified content immediately before this token.
		/// </summary>
		/// <param name="content">A content object that contains simple content or a collection of content objects to be added before this token.</param>
		public void AddBeforeSelf(object content)
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			JToken jToken = Previous;
			if (jToken == null)
			{
				jToken = _parent.Last;
			}
			_parent.AddInternal(isLast: false, previous: jToken, content: content);
		}

		/// <summary>
		/// Returns a collection of the ancestor tokens of this token.
		/// </summary>
		/// <returns>A collection of the ancestor tokens of this token.</returns>
		public IEnumerable<JToken> Ancestors()
		{
			for (JToken parent = Parent; parent != null; parent = parent.Parent)
			{
				yield return parent;
			}
		}

		/// <summary>
		/// Returns a collection of the sibling tokens after this token, in document order.
		/// </summary>
		/// <returns>A collection of the sibling tokens after this tokens, in document order.</returns>
		public IEnumerable<JToken> AfterSelf()
		{
			if (Parent != null)
			{
				for (JToken o = Next; o != null; o = o.Next)
				{
					yield return o;
				}
			}
		}

		/// <summary>
		/// Returns a collection of the sibling tokens before this token, in document order.
		/// </summary>
		/// <returns>A collection of the sibling tokens before this token, in document order.</returns>
		public IEnumerable<JToken> BeforeSelf()
		{
			for (JToken o = Parent.First; o != this; o = o.Next)
			{
				yield return o;
			}
		}

		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key converted to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the token to.</typeparam>
		/// <param name="key">The token key.</param>
		/// <returns>The converted token value.</returns>
		public virtual T Value<T>(object key)
		{
			JToken token = this[key];
			return token.Convert<JToken, T>();
		}

		/// <summary>
		/// Returns a collection of the child tokens of this token, in document order.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
		public virtual JEnumerable<JToken> Children()
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
		}

		/// <summary>
		/// Returns a collection of the child tokens of this token, in document order, filtered by the specified type.
		/// </summary>
		/// <typeparam name="T">The type to filter the child tokens on.</typeparam>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
		public JEnumerable<T> Children<T>() where T : JToken
		{
			return new JEnumerable<T>(Children().OfType<T>());
		}

		/// <summary>
		/// Returns a collection of the child values of this token, in document order.
		/// </summary>
		/// <typeparam name="T">The type to convert the values to.</typeparam>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing the child values of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
		public virtual IEnumerable<T> Values<T>()
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
		}

		/// <summary>
		/// Removes this token from its parent.
		/// </summary>
		public void Remove()
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			_parent.RemoveItem(this);
		}

		/// <summary>
		/// Replaces this token with the specified token.
		/// </summary>
		/// <param name="value">The value.</param>
		public void Replace(JToken value)
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			_parent.ReplaceItem(this, value);
		}

		/// <summary>
		/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
		public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

		/// <summary>
		/// Returns the indented JSON for this token.
		/// </summary>
		/// <returns>
		/// The indented JSON for this token.
		/// </returns>
		public override string ToString()
		{
			return ToString(Formatting.Indented);
		}

		/// <summary>
		/// Returns the JSON for this token using the given formatting and converters.
		/// </summary>
		/// <param name="formatting">Indicates how the output is formatted.</param>
		/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
		/// <returns>The JSON for this token using the given formatting and converters.</returns>
		public string ToString(Formatting formatting, params JsonConverter[] converters)
		{
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
				jsonTextWriter.Formatting = formatting;
				WriteTo(jsonTextWriter, converters);
				return stringWriter.ToString();
			}
		}

		private static JValue EnsureValue(JToken value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value is JProperty)
			{
				value = ((JProperty)value).Value;
			}
			return value as JValue;
		}

		private static string GetType(JToken token)
		{
			ValidationUtils.ArgumentNotNull(token, "token");
			if (token is JProperty)
			{
				token = ((JProperty)token).Value;
			}
			return token.Type.ToString();
		}

		private static bool IsNullable(JToken o)
		{
			if (o.Type != JTokenType.Undefined)
			{
				return o.Type == JTokenType.Null;
			}
			return true;
		}

		private static bool ValidateFloat(JToken o, bool nullable)
		{
			if (o.Type != JTokenType.Float && o.Type != JTokenType.Integer)
			{
				if (nullable)
				{
					return IsNullable(o);
				}
				return false;
			}
			return true;
		}

		private static bool ValidateInteger(JToken o, bool nullable)
		{
			if (o.Type != JTokenType.Integer)
			{
				if (nullable)
				{
					return IsNullable(o);
				}
				return false;
			}
			return true;
		}

		private static bool ValidateDate(JToken o, bool nullable)
		{
			if (o.Type != JTokenType.Date)
			{
				if (nullable)
				{
					return IsNullable(o);
				}
				return false;
			}
			return true;
		}

		private static bool ValidateBoolean(JToken o, bool nullable)
		{
			if (o.Type != JTokenType.Boolean)
			{
				if (nullable)
				{
					return IsNullable(o);
				}
				return false;
			}
			return true;
		}

		private static bool ValidateString(JToken o)
		{
			if (o.Type != JTokenType.String && o.Type != JTokenType.Comment && o.Type != JTokenType.Raw)
			{
				return IsNullable(o);
			}
			return true;
		}

		private static bool ValidateBytes(JToken o)
		{
			if (o.Type != JTokenType.Bytes)
			{
				return IsNullable(o);
			}
			return true;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Boolean" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator bool(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateBoolean(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (bool)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.DateTimeOffset" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator DateTimeOffset(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateDate(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (DateTimeOffset)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator bool?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateBoolean(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (bool?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Int64" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator long(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (long)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator DateTime?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateDate(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (DateTime?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator DateTimeOffset?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateDate(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (DateTimeOffset?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator decimal?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator double?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (double?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Int32" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator int(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator int?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.DateTime" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator DateTime(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateDate(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (DateTime)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator long?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (long?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator float?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Decimal" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator decimal(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator uint?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (uint?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator ulong?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: true))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (ulong?)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Double" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator double(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (double)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Single" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator float(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateFloat(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.String" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator string(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateString(jValue))
			{
				throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (string)jValue.Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.UInt32" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator uint(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToUInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.UInt64" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator ulong(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateInteger(jValue, nullable: false))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToUInt64(jValue.Value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Byte[]" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator byte[](JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateBytes(jValue))
			{
				throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return (byte[])jValue.Value;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Boolean" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(bool value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.DateTimeOffset" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(DateTimeOffset value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(bool? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(long value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(DateTime? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(DateTimeOffset? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(decimal? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(double? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.UInt16" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(ushort value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Int32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(int value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(int? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.DateTime" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(DateTime value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(long? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(float? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Decimal" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(decimal value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(ushort? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(uint? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(ulong? value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Double" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(double value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Single" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(float value)
		{
			return new JValue((double)value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.String" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(string value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.UInt32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(uint value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.UInt64" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(ulong value)
		{
			return new JValue(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Byte[]" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
		public static implicit operator JToken(byte[] value)
		{
			return new JValue(value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<JToken>)this).GetEnumerator();
		}

		IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
		{
			return Children().GetEnumerator();
		}

		internal abstract int GetDeepHashCode();

		/// <summary>
		/// Creates an <see cref="T:Newtonsoft.Json.JsonReader" /> for this token.
		/// </summary>
		/// <returns>An <see cref="T:Newtonsoft.Json.JsonReader" /> that can be used to read this token and its descendants.</returns>
		public JsonReader CreateReader()
		{
			return new JTokenReader(this);
		}

		internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				jsonSerializer.Serialize(jTokenWriter, o);
				return jTokenWriter.Token;
			}
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from an object.
		/// </summary>
		/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the value of the specified object</returns>
		public static JToken FromObject(object o)
		{
			return FromObjectInternal(o, new JsonSerializer());
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from an object using the specified <see cref="T:Newtonsoft.Json.JsonSerializer" />.
		/// </summary>
		/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
		/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used when reading the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the value of the specified object</returns>
		public static JToken FromObject(object o, JsonSerializer jsonSerializer)
		{
			return FromObjectInternal(o, jsonSerializer);
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
		/// </summary>
		/// <param name="reader">An <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
		/// <returns>
		/// An <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
		/// that were read from the reader. The runtime type of the token is determined
		/// by the token type of the first token encountered in the reader.
		/// </returns>
		public static JToken ReadFrom(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JToken from JsonReader.");
			}
			if (reader.TokenType == JsonToken.StartObject)
			{
				return JObject.Load(reader);
			}
			if (reader.TokenType == JsonToken.StartArray)
			{
				return JArray.Load(reader);
			}
			if (reader.TokenType == JsonToken.PropertyName)
			{
				return JProperty.Load(reader);
			}
			if (reader.TokenType == JsonToken.StartConstructor)
			{
				return JConstructor.Load(reader);
			}
			if (!JsonReader.IsStartToken(reader.TokenType))
			{
				return new JValue(reader.Value);
			}
			throw new Exception("Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}

		internal void SetLineInfo(IJsonLineInfo lineInfo)
		{
			if (lineInfo != null && lineInfo.HasLineInfo())
			{
				SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
			}
		}

		internal void SetLineInfo(int lineNumber, int linePosition)
		{
			_lineNumber = lineNumber;
			_linePosition = linePosition;
		}

		bool IJsonLineInfo.HasLineInfo()
		{
			if (_lineNumber.HasValue)
			{
				return _linePosition.HasValue;
			}
			return false;
		}

		/// <summary>
		/// Selects the token that matches the object path.
		/// </summary>
		/// <param name="path">
		/// The object path from the current <see cref="T:Newtonsoft.Json.Linq.JToken" /> to the <see cref="T:Newtonsoft.Json.Linq.JToken" />
		/// to be returned. This must be a string of property names or array indexes separated
		/// by periods, such as <code>Tables[0].DefaultView[0].Price</code> in C# or
		/// <code>Tables(0).DefaultView(0).Price</code> in Visual Basic.
		/// </param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that matches the object path or a null reference if no matching token is found.</returns>
		public JToken SelectToken(string path)
		{
			return SelectToken(path, errorWhenNoMatch: false);
		}

		/// <summary>
		/// Selects the token that matches the object path.
		/// </summary>
		/// <param name="path">
		/// The object path from the current <see cref="T:Newtonsoft.Json.Linq.JToken" /> to the <see cref="T:Newtonsoft.Json.Linq.JToken" />
		/// to be returned. This must be a string of property names or array indexes separated
		/// by periods, such as <code>Tables[0].DefaultView[0].Price</code> in C# or
		/// <code>Tables(0).DefaultView(0).Price</code> in Visual Basic.
		/// </param>
		/// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no token is found.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that matches the object path.</returns>
		public JToken SelectToken(string path, bool errorWhenNoMatch)
		{
			JPath jPath = new JPath(path);
			return jPath.Evaluate(this, errorWhenNoMatch);
		}
	}
}
