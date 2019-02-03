using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a JSON property.
	/// </summary>
	public class JProperty : JContainer
	{
		private readonly string _name;

		/// <summary>
		/// Gets the property name.
		/// </summary>
		/// <value>The property name.</value>
		public string Name
		{
			[DebuggerStepThrough]
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Gets or sets the property value.
		/// </summary>
		/// <value>The property value.</value>
		public new JToken Value
		{
			[DebuggerStepThrough]
			get
			{
				return base.Content;
			}
			set
			{
				CheckReentrancy();
				JToken jToken = value ?? new JValue((object)null);
				if (base.Content == null)
				{
					jToken = (base.Content = EnsureParentToken(jToken));
					base.Content.Parent = this;
					base.Content.Next = base.Content;
				}
				else
				{
					base.Content.Replace(jToken);
				}
			}
		}

		/// <summary>
		/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <value>The type.</value>
		public override JTokenType Type
		{
			[DebuggerStepThrough]
			get
			{
				return JTokenType.Property;
			}
		}

		internal override void ReplaceItem(JToken existing, JToken replacement)
		{
			if (!JContainer.IsTokenUnchanged(existing, replacement))
			{
				if (base.Parent != null)
				{
					((JObject)base.Parent).InternalPropertyChanging(this);
				}
				base.ReplaceItem(existing, replacement);
				if (base.Parent != null)
				{
					((JObject)base.Parent).InternalPropertyChanged(this);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class from another <see cref="T:Newtonsoft.Json.Linq.JProperty" /> object.
		/// </summary>
		/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> object to copy from.</param>
		public JProperty(JProperty other)
			: base(other)
		{
			_name = other.Name;
		}

		internal override void AddItem(bool isLast, JToken previous, JToken item)
		{
			if (Value != null)
			{
				throw new Exception("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
			}
			Value = item;
		}

		internal override JToken GetItem(int index)
		{
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Value;
		}

		internal override void SetItem(int index, JToken item)
		{
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			Value = item;
		}

		internal override bool RemoveItem(JToken item)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
		}

		internal override void RemoveItemAt(int index)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
		}

		internal override void InsertItem(int index, JToken item)
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
		}

		internal override bool ContainsItem(JToken item)
		{
			return Value == item;
		}

		internal override void ClearItems()
		{
			throw new Exception("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
		}

		/// <summary>
		/// Returns a collection of the child tokens of this token, in document order.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.
		/// </returns>
		public override JEnumerable<JToken> Children()
		{
			return new JEnumerable<JToken>(GetValueEnumerable());
		}

		private IEnumerable<JToken> GetValueEnumerable()
		{
			yield return Value;
		}

		internal override bool DeepEquals(JToken node)
		{
			JProperty jProperty = node as JProperty;
			if (jProperty != null && _name == jProperty.Name)
			{
				return ContentsEqual(jProperty);
			}
			return false;
		}

		internal override JToken CloneToken()
		{
			return new JProperty(this);
		}

		internal JProperty(string name)
		{
			ValidationUtils.ArgumentNotNull(name, "name");
			_name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="content">The property content.</param>
		public JProperty(string name, params object[] content)
			: this(name, (object)content)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="content">The property content.</param>
		public JProperty(string name, object content)
		{
			ValidationUtils.ArgumentNotNull(name, "name");
			_name = name;
			Value = (IsMultiContent(content) ? new JArray(content) : CreateFromContent(content));
		}

		/// <summary>
		/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WritePropertyName(_name);
			Value.WriteTo(writer, converters);
		}

		internal override int GetDeepHashCode()
		{
			return _name.GetHashCode() ^ ((Value != null) ? Value.GetDeepHashCode() : 0);
		}

		/// <summary>
		/// Loads an <see cref="T:Newtonsoft.Json.Linq.JProperty" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />. 
		/// </summary>
		/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JProperty" />.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
		public static JProperty Load(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JProperty from JsonReader.");
			}
			if (reader.TokenType != JsonToken.PropertyName)
			{
				throw new Exception("Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			JProperty jProperty = new JProperty((string)reader.Value);
			jProperty.SetLineInfo(reader as IJsonLineInfo);
			if (!reader.Read())
			{
				throw new Exception("Error reading JProperty from JsonReader.");
			}
			jProperty.ReadContentFrom(reader);
			return jProperty;
		}
	}
}
