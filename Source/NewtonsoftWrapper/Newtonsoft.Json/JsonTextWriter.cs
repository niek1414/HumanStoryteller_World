using Newtonsoft.Json.Utilities;
using System;
using System.IO;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
	/// </summary>
	public class JsonTextWriter : JsonWriter
	{
		private readonly TextWriter _writer;

		private Base64Encoder _base64Encoder;

		private char _indentChar;

		private int _indentation;

		private char _quoteChar;

		private bool _quoteName;

		private Base64Encoder Base64Encoder
		{
			get
			{
				if (_base64Encoder == null)
				{
					_base64Encoder = new Base64Encoder(_writer);
				}
				return _base64Encoder;
			}
		}

		/// <summary>
		/// Gets or sets how many IndentChars to write for each level in the hierarchy when <paramref name="Formatting" /> is set to <c>Formatting.Indented</c>.
		/// </summary>
		public int Indentation
		{
			get
			{
				return _indentation;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Indentation value must be greater than 0.");
				}
				_indentation = value;
			}
		}

		/// <summary>
		/// Gets or sets which character to use to quote attribute values.
		/// </summary>
		public char QuoteChar
		{
			get
			{
				return _quoteChar;
			}
			set
			{
				if (value != '"' && value != '\'')
				{
					throw new ArgumentException("Invalid JavaScript string quote character. Valid quote characters are ' and \".");
				}
				_quoteChar = value;
			}
		}

		/// <summary>
		/// Gets or sets which character to use for indenting when <paramref name="Formatting" /> is set to <c>Formatting.Indented</c>.
		/// </summary>
		public char IndentChar
		{
			get
			{
				return _indentChar;
			}
			set
			{
				_indentChar = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether object names will be surrounded with quotes.
		/// </summary>
		public bool QuoteName
		{
			get
			{
				return _quoteName;
			}
			set
			{
				_quoteName = value;
			}
		}

		/// <summary>
		/// Creates an instance of the <c>JsonWriter</c> class using the specified <see cref="T:System.IO.TextWriter" />. 
		/// </summary>
		/// <param name="textWriter">The <c>TextWriter</c> to write to.</param>
		public JsonTextWriter(TextWriter textWriter)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException("textWriter");
			}
			_writer = textWriter;
			_quoteChar = '"';
			_quoteName = true;
			_indentChar = ' ';
			_indentation = 2;
		}

		/// <summary>
		/// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
		/// </summary>
		public override void Flush()
		{
			_writer.Flush();
		}

		/// <summary>
		/// Closes this stream and the underlying stream.
		/// </summary>
		public override void Close()
		{
			base.Close();
			_writer.Close();
		}

		/// <summary>
		/// Writes the beginning of a Json object.
		/// </summary>
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			_writer.Write("{");
		}

		/// <summary>
		/// Writes the beginning of a Json array.
		/// </summary>
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			_writer.Write("[");
		}

		/// <summary>
		/// Writes the start of a constructor with the given name.
		/// </summary>
		/// <param name="name">The name of the constructor.</param>
		public override void WriteStartConstructor(string name)
		{
			base.WriteStartConstructor(name);
			_writer.Write("new ");
			_writer.Write(name);
			_writer.Write("(");
		}

		/// <summary>
		/// Writes the specified end token.
		/// </summary>
		/// <param name="token">The end token to write.</param>
		protected override void WriteEnd(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
				_writer.Write("}");
				break;
			case JsonToken.EndArray:
				_writer.Write("]");
				break;
			case JsonToken.EndConstructor:
				_writer.Write(")");
				break;
			default:
				throw new JsonWriterException("Invalid JsonToken: " + token);
			}
		}

		/// <summary>
		/// Writes the property name of a name/value pair on a Json object.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			JavaScriptUtils.WriteEscapedJavaScriptString(_writer, name, _quoteChar, _quoteName);
			_writer.Write(':');
		}

		/// <summary>
		/// Writes indent characters.
		/// </summary>
		protected override void WriteIndent()
		{
			if (base.Formatting == Formatting.Indented)
			{
				_writer.Write(Environment.NewLine);
				int num = base.Top * _indentation;
				for (int i = 0; i < num; i++)
				{
					_writer.Write(_indentChar);
				}
			}
		}

		/// <summary>
		/// Writes the JSON value delimiter.
		/// </summary>
		protected override void WriteValueDelimiter()
		{
			_writer.Write(',');
		}

		/// <summary>
		/// Writes an indent space.
		/// </summary>
		protected override void WriteIndentSpace()
		{
			_writer.Write(' ');
		}

		private void WriteValueInternal(string value, JsonToken token)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		public override void WriteNull()
		{
			base.WriteNull();
			WriteValueInternal(JsonConvert.Null, JsonToken.Null);
		}

		/// <summary>
		/// Writes an undefined value.
		/// </summary>
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
		}

		/// <summary>
		/// Writes raw JSON.
		/// </summary>
		/// <param name="json">The raw JSON to write.</param>
		public override void WriteRaw(string json)
		{
			base.WriteRaw(json);
			_writer.Write(json);
		}

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			if (value == null)
			{
				WriteValueInternal(JsonConvert.Null, JsonToken.Null);
			}
			else
			{
				JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, appendDelimiters: true);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int32" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt32" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt64" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.UInt16" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.SByte" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
		}

		/// <summary>
		/// Writes a <see cref="T:System.DateTime" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			JsonConvert.WriteDateTimeString(_writer, value);
		}

		/// <summary>
		/// Writes a <see cref="T:Byte[]" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:Byte[]" /> value to write.</param>
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			if (value != null)
			{
				_writer.Write(_quoteChar);
				Base64Encoder.Encode(value, 0, value.Length);
				Base64Encoder.Flush();
				_writer.Write(_quoteChar);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.DateTimeOffset" /> value.
		/// </summary>
		/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			WriteValueInternal(JsonConvert.ToString(value), JsonToken.Date);
		}

		/// <summary>
		/// Writes out a comment <code>/*...*/</code> containing the specified text. 
		/// </summary>
		/// <param name="text">Text to place inside the comment.</param>
		public override void WriteComment(string text)
		{
			base.WriteComment(text);
			_writer.Write("/*");
			_writer.Write(text);
			_writer.Write("*/");
		}

		/// <summary>
		/// Writes out the given white space.
		/// </summary>
		/// <param name="ws">The string of white space characters.</param>
		public override void WriteWhitespace(string ws)
		{
			base.WriteWhitespace(ws);
			_writer.Write(ws);
		}
	}
}
