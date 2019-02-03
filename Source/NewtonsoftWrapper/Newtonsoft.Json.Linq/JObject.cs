using Newtonsoft.Json.Linq.ComponentModel;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a JSON object.
	/// </summary>
	[TypeDescriptionProvider(typeof(JTypeDescriptionProvider))]
	public class JObject : JContainer, IDictionary<string, JToken>, ICollection<KeyValuePair<string, JToken>>, IEnumerable<KeyValuePair<string, JToken>>, IEnumerable, INotifyPropertyChanged, INotifyPropertyChanging
	{
		/// <summary>
		/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <value>The type.</value>
		public override JTokenType Type => JTokenType.Object;

		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
		public override JToken this[object key]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				string text = key as string;
				if (text == null)
				{
					throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
				}
				return this[text];
			}
			set
			{
				ValidationUtils.ArgumentNotNull(key, "o");
				string text = key as string;
				if (text == null)
				{
					throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
				}
				this[text] = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
		/// </summary>
		/// <value></value>
		public JToken this[string propertyName]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
				return Property(propertyName)?.Value;
			}
			set
			{
				JProperty jProperty = Property(propertyName);
				if (jProperty != null)
				{
					jProperty.Value = value;
				}
				else
				{
					OnPropertyChanging(propertyName);
					Add(new JProperty(propertyName, value));
					OnPropertyChanged(propertyName);
				}
			}
		}

		ICollection<string> IDictionary<string, JToken>.Keys
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		ICollection<JToken> IDictionary<string, JToken>.Values
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => Children().Count();

		bool ICollection<KeyValuePair<string, JToken>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when a property value is changing.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class.
		/// </summary>
		public JObject()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class from another <see cref="T:Newtonsoft.Json.Linq.JObject" /> object.
		/// </summary>
		/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JObject" /> object to copy from.</param>
		public JObject(JObject other)
			: base(other)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
		/// </summary>
		/// <param name="content">The contents of the object.</param>
		public JObject(params object[] content)
			: this((object)content)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
		/// </summary>
		/// <param name="content">The contents of the object.</param>
		public JObject(object content)
		{
			Add(content);
		}

		internal override bool DeepEquals(JToken node)
		{
			JObject jObject = node as JObject;
			if (jObject != null)
			{
				return ContentsEqual(jObject);
			}
			return false;
		}

		internal override void ValidateToken(JToken o, JToken existing)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			if (o.Type != JTokenType.Property)
			{
				throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
			}
			JProperty jProperty = (JProperty)o;
			foreach (JProperty item in Children())
			{
				if (item != existing && string.Equals(item.Name, jProperty.Name, StringComparison.Ordinal))
				{
					throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, jProperty.Name, GetType()));
				}
			}
		}

		internal void InternalPropertyChanged(JProperty childProperty)
		{
			OnPropertyChanged(childProperty.Name);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOfItem(childProperty)));
		}

		internal void InternalPropertyChanging(JProperty childProperty)
		{
			OnPropertyChanging(childProperty.Name);
		}

		internal override JToken CloneToken()
		{
			return new JObject(this);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.IEnumerable`1" /> of this object's properties.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of this object's properties.</returns>
		public IEnumerable<JProperty> Properties()
		{
			return Children().Cast<JProperty>();
		}

		/// <summary>
		/// Gets a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> the specified name.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name or null.</returns>
		public JProperty Property(string name)
		{
			return (from p in Properties()
			where string.Equals(p.Name, name, StringComparison.Ordinal)
			select p).SingleOrDefault();
		}

		/// <summary>
		/// Gets an <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of this object's property values.
		/// </summary>
		/// <returns>An <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of this object's property values.</returns>
		public JEnumerable<JToken> PropertyValues()
		{
			return new JEnumerable<JToken>(from p in Properties()
			select p.Value);
		}

		/// <summary>
		/// Loads an <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />. 
		/// </summary>
		/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
		public static JObject Load(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw new Exception("Error reading JObject from JsonReader.");
			}
			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			JObject jObject = new JObject();
			jObject.SetLineInfo(reader as IJsonLineInfo);
			if (!reader.Read())
			{
				throw new Exception("Error reading JObject from JsonReader.");
			}
			jObject.ReadContentFrom(reader);
			return jObject;
		}

		/// <summary>
		/// Load a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a string that contains JSON.
		/// </summary>
		/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> populated from the string that contains JSON.</returns>
		public static JObject Parse(string json)
		{
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return Load(reader);
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from an object.
		/// </summary>
		/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> with the values of the specified object</returns>
		public new static JObject FromObject(object o)
		{
			return FromObject(o, new JsonSerializer());
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Linq.JArray" /> from an object.
		/// </summary>
		/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JArray" />.</param>
		/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used to read the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JArray" /> with the values of the specified object</returns>
		public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
		{
			JToken jToken = JToken.FromObjectInternal(o, jsonSerializer);
			if (jToken != null && jToken.Type != JTokenType.Object)
			{
				throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, jToken.Type));
			}
			return (JObject)jToken;
		}

		/// <summary>
		/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartObject();
			foreach (JProperty item in ChildrenInternal())
			{
				item.WriteTo(writer, converters);
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Adds the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">The value.</param>
		public void Add(string propertyName, JToken value)
		{
			Add(new JProperty(propertyName, value));
		}

		bool IDictionary<string, JToken>.ContainsKey(string key)
		{
			return Property(key) != null;
		}

		/// <summary>
		/// Removes the property with the specified name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>true if item was successfully removed; otherwise, false.</returns>
		public bool Remove(string propertyName)
		{
			JProperty jProperty = Property(propertyName);
			if (jProperty == null)
			{
				return false;
			}
			jProperty.Remove();
			return true;
		}

		/// <summary>
		/// Tries the get value.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">The value.</param>
		/// <returns>true if a value was successfully retrieved; otherwise, false.</returns>
		public bool TryGetValue(string propertyName, out JToken value)
		{
			JProperty jProperty = Property(propertyName);
			if (jProperty == null)
			{
				value = null;
				return false;
			}
			value = jProperty.Value;
			return true;
		}

		void ICollection<KeyValuePair<string, JToken>>.Add(KeyValuePair<string, JToken> item)
		{
			Add(new JProperty(item.Key, item.Value));
		}

		void ICollection<KeyValuePair<string, JToken>>.Clear()
		{
			RemoveAll();
		}

		bool ICollection<KeyValuePair<string, JToken>>.Contains(KeyValuePair<string, JToken> item)
		{
			JProperty jProperty = Property(item.Key);
			if (jProperty == null)
			{
				return false;
			}
			return jProperty.Value == item.Value;
		}

		void ICollection<KeyValuePair<string, JToken>>.CopyTo(KeyValuePair<string, JToken>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
			}
			if (arrayIndex >= array.Length)
			{
				throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
			}
			if (Count > array.Length - arrayIndex)
			{
				throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
			}
			int num = 0;
			foreach (JProperty item in Properties())
			{
				array[arrayIndex + num] = new KeyValuePair<string, JToken>(item.Name, item.Value);
				num++;
			}
		}

		bool ICollection<KeyValuePair<string, JToken>>.Remove(KeyValuePair<string, JToken> item)
		{
			if (!((ICollection<KeyValuePair<string, JToken>>)this).Contains(item))
			{
				return false;
			}
			((IDictionary<string, JToken>)this).Remove(item.Key);
			return true;
		}

		internal override int GetDeepHashCode()
		{
			return ContentsHashCode();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()
		{
			foreach (JProperty item in Properties())
			{
				yield return new KeyValuePair<string, JToken>(item.Name, item.Value);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanged" /> event with the provided arguments.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanging" /> event with the provided arguments.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (this.PropertyChanging != null)
			{
				this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}
	}
}
