using System;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Instructs the <see cref="T:Newtonsoft.Json.JsonSerializer" /> to always serialize the member with the specified name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class JsonPropertyAttribute : Attribute
	{
		internal NullValueHandling? _nullValueHandling;

		internal DefaultValueHandling? _defaultValueHandling;

		internal ReferenceLoopHandling? _referenceLoopHandling;

		internal ObjectCreationHandling? _objectCreationHandling;

		internal TypeNameHandling? _typeNameHandling;

		internal bool? _isReference;

		/// <summary>
		/// Gets or sets the null value handling used when serializing this property.
		/// </summary>
		/// <value>The null value handling.</value>
		public NullValueHandling NullValueHandling
		{
			get
			{
				return _nullValueHandling ?? NullValueHandling.Include;
			}
			set
			{
				_nullValueHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets the default value handling used when serializing this property.
		/// </summary>
		/// <value>The default value handling.</value>
		public DefaultValueHandling DefaultValueHandling
		{
			get
			{
				return _defaultValueHandling ?? DefaultValueHandling.Include;
			}
			set
			{
				_defaultValueHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets the reference loop handling used when serializing this property.
		/// </summary>
		/// <value>The reference loop handling.</value>
		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return _referenceLoopHandling ?? ReferenceLoopHandling.Error;
			}
			set
			{
				_referenceLoopHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets the object creation handling used when deserializing this property.
		/// </summary>
		/// <value>The object creation handling.</value>
		public ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return _objectCreationHandling ?? ObjectCreationHandling.Auto;
			}
			set
			{
				_objectCreationHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets the type name handling used when serializing this property.
		/// </summary>
		/// <value>The type name handling.</value>
		public TypeNameHandling TypeNameHandling
		{
			get
			{
				return _typeNameHandling ?? TypeNameHandling.None;
			}
			set
			{
				_typeNameHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets whether this property's value is serialized as a reference.
		/// </summary>
		/// <value>Whether this property's value is serialized as a reference.</value>
		public bool IsReference
		{
			get
			{
				return _isReference ?? false;
			}
			set
			{
				_isReference = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the property.
		/// </summary>
		/// <value>The name of the property.</value>
		public string PropertyName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this property is required.
		/// </summary>
		/// <value>
		/// 	A value indicating whether this property is required.
		/// </value>
		public Required Required
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" /> class.
		/// </summary>
		public JsonPropertyAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" /> class with the specified name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		public JsonPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}
