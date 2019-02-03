using System;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Maps a JSON property to a .NET member.
	/// </summary>
	public class JsonProperty
	{
		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		/// <value>The name of the property.</value>
		public string PropertyName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> that will get and set the <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> during serialization.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> that will get and set the <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> during serialization.</value>
		public IValueProvider ValueProvider
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type of the property.
		/// </summary>
		/// <value>The type of the property.</value>
		public Type PropertyType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.JsonConverter" /> for the property.
		/// If set this converter takes presidence over the contract converter for the property type.
		/// </summary>
		/// <value>The converter.</value>
		public JsonConverter Converter
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is ignored.
		/// </summary>
		/// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
		public bool Ignored
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is readable.
		/// </summary>
		/// <value><c>true</c> if readable; otherwise, <c>false</c>.</value>
		public bool Readable
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is writable.
		/// </summary>
		/// <value><c>true</c> if writable; otherwise, <c>false</c>.</value>
		public bool Writable
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the member converter.
		/// </summary>
		/// <value>The member converter.</value>
		public JsonConverter MemberConverter
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the default value.
		/// </summary>
		/// <value>The default value.</value>
		public object DefaultValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is required.
		/// </summary>
		/// <value>A value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is required.</value>
		public Required Required
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this property preserves object references.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is reference; otherwise, <c>false</c>.
		/// </value>
		public bool? IsReference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the property null value handling.
		/// </summary>
		/// <value>The null value handling.</value>
		public NullValueHandling? NullValueHandling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the property default value handling.
		/// </summary>
		/// <value>The default value handling.</value>
		public DefaultValueHandling? DefaultValueHandling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the property reference loop handling.
		/// </summary>
		/// <value>The reference loop handling.</value>
		public ReferenceLoopHandling? ReferenceLoopHandling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the property object creation handling.
		/// </summary>
		/// <value>The object creation handling.</value>
		public ObjectCreationHandling? ObjectCreationHandling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type name handling.
		/// </summary>
		/// <value>The type name handling.</value>
		public TypeNameHandling? TypeNameHandling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a predicate used to determine whether the property should be serialize.
		/// </summary>
		/// <value>A predicate used to determine whether the property should be serialize.</value>
		public Predicate<object> ShouldSerialize
		{
			get;
			set;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return PropertyName;
		}
	}
}
