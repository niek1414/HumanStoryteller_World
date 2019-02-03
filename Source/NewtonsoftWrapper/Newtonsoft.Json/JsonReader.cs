using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
	/// </summary>
	public abstract class JsonReader : IDisposable
	{
		/// <summary>
		/// Specifies the state of the reader.
		/// </summary>
		protected enum State
		{
			/// <summary>
			/// The Read method has not been called.
			/// </summary>
			Start,
			/// <summary>
			/// The end of the file has been reached successfully.
			/// </summary>
			Complete,
			/// <summary>
			/// Reader is at a property.
			/// </summary>
			Property,
			/// <summary>
			/// Reader is at the start of an object.
			/// </summary>
			ObjectStart,
			/// <summary>
			/// Reader is in an object.
			/// </summary>
			Object,
			/// <summary>
			/// Reader is at the start of an array.
			/// </summary>
			ArrayStart,
			/// <summary>
			/// Reader is in an array.
			/// </summary>
			Array,
			/// <summary>
			/// The Close method has been called.
			/// </summary>
			Closed,
			/// <summary>
			/// Reader has just read a value.
			/// </summary>
			PostValue,
			/// <summary>
			/// Reader is at the start of a constructor.
			/// </summary>
			ConstructorStart,
			/// <summary>
			/// Reader in a constructor.
			/// </summary>
			Constructor,
			/// <summary>
			/// An error occurred that prevents the read operation from continuing.
			/// </summary>
			Error,
			/// <summary>
			/// The end of the file has been reached successfully.
			/// </summary>
			Finished
		}

		private JsonToken _token;

		private object _value;

		private Type _valueType;

		private char _quoteChar;

		private State _currentState;

		private JTokenType _currentTypeContext;

		private int _top;

		private readonly List<JTokenType> _stack;

		/// <summary>
		/// Gets the current reader state.
		/// </summary>
		/// <value>The current reader state.</value>
		protected State CurrentState => _currentState;

		/// <summary>
		/// Gets the quotation mark character used to enclose the value of a string.
		/// </summary>
		public virtual char QuoteChar
		{
			get
			{
				return _quoteChar;
			}
			protected internal set
			{
				_quoteChar = value;
			}
		}

		/// <summary>
		/// Gets the type of the current Json token. 
		/// </summary>
		public virtual JsonToken TokenType => _token;

		/// <summary>
		/// Gets the text value of the current Json token.
		/// </summary>
		public virtual object Value => _value;

		/// <summary>
		/// Gets The Common Language Runtime (CLR) type for the current Json token.
		/// </summary>
		public virtual Type ValueType => _valueType;

		/// <summary>
		/// Gets the depth of the current token in the JSON document.
		/// </summary>
		/// <value>The depth of the current token in the JSON document.</value>
		public virtual int Depth
		{
			get
			{
				int num = _top - 1;
				if (IsStartToken(TokenType))
				{
					return num - 1;
				}
				return num;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonReader" /> class with the specified <see cref="T:System.IO.TextReader" />.
		/// </summary>
		protected JsonReader()
		{
			_currentState = State.Start;
			_stack = new List<JTokenType>();
			Push(JTokenType.None);
		}

		private void Push(JTokenType value)
		{
			_stack.Add(value);
			_top++;
			_currentTypeContext = value;
		}

		private JTokenType Pop()
		{
			JTokenType result = Peek();
			_stack.RemoveAt(_stack.Count - 1);
			_top--;
			_currentTypeContext = _stack[_top - 1];
			return result;
		}

		private JTokenType Peek()
		{
			return _currentTypeContext;
		}

		/// <summary>
		/// Reads the next JSON token from the stream.
		/// </summary>
		/// <returns>true if the next token was read successfully; false if there are no more tokens to read.</returns>
		public abstract bool Read();

		/// <summary>
		/// Reads the next JSON token from the stream as a <see cref="T:Byte[]" />.
		/// </summary>
		/// <returns>A <see cref="T:Byte[]" /> or a null reference if the next JSON token is null.</returns>
		public abstract byte[] ReadAsBytes();

		/// <summary>
		/// Skips the children of the current token.
		/// </summary>
		public void Skip()
		{
			if (IsStartToken(TokenType))
			{
				int depth = Depth;
				while (Read() && depth < Depth)
				{
				}
			}
		}

		/// <summary>
		/// Sets the current token.
		/// </summary>
		/// <param name="newToken">The new token.</param>
		protected void SetToken(JsonToken newToken)
		{
			SetToken(newToken, null);
		}

		/// <summary>
		/// Sets the current token and value.
		/// </summary>
		/// <param name="newToken">The new token.</param>
		/// <param name="value">The value.</param>
		protected virtual void SetToken(JsonToken newToken, object value)
		{
			_token = newToken;
			switch (newToken)
			{
			case JsonToken.StartObject:
				_currentState = State.ObjectStart;
				Push(JTokenType.Object);
				break;
			case JsonToken.StartArray:
				_currentState = State.ArrayStart;
				Push(JTokenType.Array);
				break;
			case JsonToken.StartConstructor:
				_currentState = State.ConstructorStart;
				Push(JTokenType.Constructor);
				break;
			case JsonToken.EndObject:
				ValidateEnd(JsonToken.EndObject);
				_currentState = State.PostValue;
				break;
			case JsonToken.EndArray:
				ValidateEnd(JsonToken.EndArray);
				_currentState = State.PostValue;
				break;
			case JsonToken.EndConstructor:
				ValidateEnd(JsonToken.EndConstructor);
				_currentState = State.PostValue;
				break;
			case JsonToken.PropertyName:
				_currentState = State.Property;
				Push(JTokenType.Property);
				break;
			case JsonToken.Raw:
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				_currentState = State.PostValue;
				break;
			}
			JTokenType jTokenType = Peek();
			if (jTokenType == JTokenType.Property && _currentState == State.PostValue)
			{
				Pop();
			}
			if (value != null)
			{
				_value = value;
				_valueType = value.GetType();
			}
			else
			{
				_value = null;
				_valueType = null;
			}
		}

		private void ValidateEnd(JsonToken endToken)
		{
			JTokenType jTokenType = Pop();
			if (GetTypeForCloseToken(endToken) != jTokenType)
			{
				throw new JsonReaderException("JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, jTokenType));
			}
		}

		/// <summary>
		/// Sets the state based on current token type.
		/// </summary>
		protected void SetStateBasedOnCurrent()
		{
			JTokenType jTokenType = Peek();
			switch (jTokenType)
			{
			case JTokenType.Object:
				_currentState = State.Object;
				break;
			case JTokenType.Array:
				_currentState = State.Array;
				break;
			case JTokenType.Constructor:
				_currentState = State.Constructor;
				break;
			case JTokenType.None:
				_currentState = State.Finished;
				break;
			default:
				throw new JsonReaderException("While setting the reader state back to current object an unexpected JsonType was encountered: " + jTokenType);
			}
		}

		internal static bool IsPrimitiveToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsStartToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.StartObject:
			case JsonToken.StartArray:
			case JsonToken.StartConstructor:
			case JsonToken.PropertyName:
				return true;
			case JsonToken.None:
			case JsonToken.Comment:
			case JsonToken.Raw:
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.EndObject:
			case JsonToken.EndArray:
			case JsonToken.EndConstructor:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return false;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected JsonToken value.");
			}
		}

		private JTokenType GetTypeForCloseToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
				return JTokenType.Object;
			case JsonToken.EndArray:
				return JTokenType.Array;
			case JsonToken.EndConstructor:
				return JTokenType.Constructor;
			default:
				throw new JsonReaderException("Not a valid close JsonToken: " + token);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_currentState != State.Closed && disposing)
			{
				Close();
			}
		}

		/// <summary>
		/// Changes the <see cref="T:Newtonsoft.Json.JsonReader.State" /> to Closed. 
		/// </summary>
		public virtual void Close()
		{
			_currentState = State.Closed;
			_token = JsonToken.None;
			_value = null;
			_valueType = null;
		}
	}
}
