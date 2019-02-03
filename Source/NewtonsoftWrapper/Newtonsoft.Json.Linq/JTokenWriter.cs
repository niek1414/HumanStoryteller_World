using Newtonsoft.Json.Utilities;
using System;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
	/// </summary>
	public class JTokenWriter : JsonWriter
	{
		private JContainer _token;

		private JContainer _parent;

		private JValue _value;

		/// <summary>
		/// Gets the token being writen.
		/// </summary>
		/// <value>The token being writen.</value>
		public JToken Token
		{
			get
			{
				if (_token != null)
				{
					return _token;
				}
				return _value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenWriter" /> class writing to the given <see cref="T:Newtonsoft.Json.Linq.JContainer" />.
		/// </summary>
		/// <param name="container">The container being written to.</param>
		public JTokenWriter(JContainer container)
		{
			ValidationUtils.ArgumentNotNull(container, "container");
			_token = container;
			_parent = container;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenWriter" /> class.
		/// </summary>
		public JTokenWriter()
		{
		}

		/// <summary>
		/// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
		/// </summary>
		public override void Flush()
		{
		}

		/// <summary>
		/// Closes this stream and the underlying stream.
		/// </summary>
		public override void Close()
		{
			base.Close();
		}

		/// <summary>
		/// Writes the beginning of a Json object.
		/// </summary>
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			AddParent(new JObject());
		}

		private void AddParent(JContainer container)
		{
			if (_parent == null)
			{
				_token = container;
			}
			else
			{
				_parent.Add(container);
			}
			_parent = container;
		}

		private void RemoveParent()
		{
			_parent = _parent.Parent;
			if (_parent != null && _parent.Type == JTokenType.Property)
			{
				_parent = _parent.Parent;
			}
		}

		/// <summary>
		/// Writes the beginning of a Json array.
		/// </summary>
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			AddParent(new JArray());
		}

		/// <summary>
		/// Writes the start of a constructor with the given name.
		/// </summary>
		/// <param name="name">The name of the constructor.</param>
		public override void WriteStartConstructor(string name)
		{
			base.WriteStartConstructor(name);
			AddParent(new JConstructor(name));
		}

		/// <summary>
		/// Writes the end.
		/// </summary>
		/// <param name="token">The token.</param>
		protected override void WriteEnd(JsonToken token)
		{
			RemoveParent();
		}

		/// <summary>
		/// Writes the property name of a name/value pair on a Json object.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			AddParent(new JProperty(name));
		}

		private void AddValue(object value, JsonToken token)
		{
			AddValue(new JValue(value), token);
		}

		internal void AddValue(JValue value, JsonToken token)
		{
			if (_parent != null)
			{
				_parent.Add(value);
				if (_parent.Type == JTokenType.Property)
				{
					_parent = _parent.Parent;
				}
			}
			else
			{
				_value = value;
			}
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		public override void WriteNull()
		{
			base.WriteNull();
			AddValue(null, JsonToken.Null);
		}

		/// <summary>
		/// Writes an undefined value.
		/// </summary>
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			AddValue(null, JsonToken.Undefined);
		}

		/// <summary>
		/// Writes raw JSON.
		/// </summary>
		/// <param name="json">The raw JSON to write.</param>
		public override void WriteRaw(string json)
		{
			base.WriteRaw(json);
			AddValue(new JRaw(json), JsonToken.Raw);
		}

		/// <summary>
		/// Writes out a comment <code>/*...*/</code> containing the specified text.
		/// </summary>
		/// <param name="text">Text to place inside the comment.</param>
		public override void WriteComment(string text)
		{
			base.WriteComment(text);
			AddValue(JValue.CreateComment(text), JsonToken.Comment);
		}

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			AddValue(value ?? string.Empty, JsonToken.String);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int32" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt32" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt64" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Boolean);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt16" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			AddValue(value.ToString(), JsonToken.String);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.SByte" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.DateTime" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Date);
		}

		/// <summary>
		/// Writes a <see cref="T:System.DateTimeOffset" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Date);
		}

		/// <summary>
		/// Writes a <see cref="T:Byte[]" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:Byte[]" /> value to write.</param>
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Bytes);
		}
	}
}
