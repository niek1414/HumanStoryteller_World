using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Serializes and deserializes objects into and from the JSON format.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> enables you to control how objects are encoded into JSON.
	/// </summary>
	public class JsonSerializer
	{
		private TypeNameHandling _typeNameHandling;

		private FormatterAssemblyStyle _typeNameAssemblyFormat;

		private PreserveReferencesHandling _preserveReferencesHandling;

		private ReferenceLoopHandling _referenceLoopHandling;

		private MissingMemberHandling _missingMemberHandling;

		private ObjectCreationHandling _objectCreationHandling;

		private NullValueHandling _nullValueHandling;

		private DefaultValueHandling _defaultValueHandling;

		private ConstructorHandling _constructorHandling;

		private JsonConverterCollection _converters;

		private IContractResolver _contractResolver;

		private IReferenceResolver _referenceResolver;

		private SerializationBinder _binder;

		private StreamingContext _context;

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.IReferenceResolver" /> used by the serializer when resolving references.
		/// </summary>
		public virtual IReferenceResolver ReferenceResolver
		{
			get
			{
				if (_referenceResolver == null)
				{
					_referenceResolver = new DefaultReferenceResolver();
				}
				return _referenceResolver;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Reference resolver cannot be null.");
				}
				_referenceResolver = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:System.Runtime.Serialization.SerializationBinder" /> used by the serializer when resolving type names.
		/// </summary>
		public virtual SerializationBinder Binder
		{
			get
			{
				return _binder;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Serialization binder cannot be null.");
				}
				_binder = value;
			}
		}

		/// <summary>
		/// Gets or sets how type name writing and reading is handled by the serializer.
		/// </summary>
		public virtual TypeNameHandling TypeNameHandling
		{
			get
			{
				return _typeNameHandling;
			}
			set
			{
				if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_typeNameHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets how a type name assembly is written and resolved by the serializer.
		/// </summary>
		/// <value>The type name assembly format.</value>
		public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			get
			{
				return _typeNameAssemblyFormat;
			}
			set
			{
				if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_typeNameAssemblyFormat = value;
			}
		}

		/// <summary>
		/// Gets or sets how object references are preserved by the serializer.
		/// </summary>
		public virtual PreserveReferencesHandling PreserveReferencesHandling
		{
			get
			{
				return _preserveReferencesHandling;
			}
			set
			{
				if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_preserveReferencesHandling = value;
			}
		}

		/// <summary>
		/// Get or set how reference loops (e.g. a class referencing itself) is handled.
		/// </summary>
		public virtual ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return _referenceLoopHandling;
			}
			set
			{
				if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_referenceLoopHandling = value;
			}
		}

		/// <summary>
		/// Get or set how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
		/// </summary>
		public virtual MissingMemberHandling MissingMemberHandling
		{
			get
			{
				return _missingMemberHandling;
			}
			set
			{
				if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_missingMemberHandling = value;
			}
		}

		/// <summary>
		/// Get or set how null values are handled during serialization and deserialization.
		/// </summary>
		public virtual NullValueHandling NullValueHandling
		{
			get
			{
				return _nullValueHandling;
			}
			set
			{
				if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_nullValueHandling = value;
			}
		}

		/// <summary>
		/// Get or set how null default are handled during serialization and deserialization.
		/// </summary>
		public virtual DefaultValueHandling DefaultValueHandling
		{
			get
			{
				return _defaultValueHandling;
			}
			set
			{
				if (value < DefaultValueHandling.Include || value > DefaultValueHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_defaultValueHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets how objects are created during deserialization.
		/// </summary>
		/// <value>The object creation handling.</value>
		public virtual ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return _objectCreationHandling;
			}
			set
			{
				if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_objectCreationHandling = value;
			}
		}

		/// <summary>
		/// Gets or sets how constructors are used during deserialization.
		/// </summary>
		/// <value>The constructor handling.</value>
		public virtual ConstructorHandling ConstructorHandling
		{
			get
			{
				return _constructorHandling;
			}
			set
			{
				if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_constructorHandling = value;
			}
		}

		/// <summary>
		/// Gets a collection <see cref="T:Newtonsoft.Json.JsonConverter" /> that will be used during serialization.
		/// </summary>
		/// <value>Collection <see cref="T:Newtonsoft.Json.JsonConverter" /> that will be used during serialization.</value>
		public virtual JsonConverterCollection Converters
		{
			get
			{
				if (_converters == null)
				{
					_converters = new JsonConverterCollection();
				}
				return _converters;
			}
		}

		/// <summary>
		/// Gets or sets the contract resolver used by the serializer when
		/// serializing .NET objects to JSON and vice versa.
		/// </summary>
		public virtual IContractResolver ContractResolver
		{
			get
			{
				if (_contractResolver == null)
				{
					_contractResolver = DefaultContractResolver.Instance;
				}
				return _contractResolver;
			}
			set
			{
				_contractResolver = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:System.Runtime.Serialization.StreamingContext" /> used by the serializer when invoking serialization callback methods.
		/// </summary>
		/// <value>The context.</value>
		public virtual StreamingContext Context
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		/// <summary>
		/// Occurs when the <see cref="T:Newtonsoft.Json.JsonSerializer" /> errors during serialization and deserialization.
		/// </summary>
		public virtual event EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> Error;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializer" /> class.
		/// </summary>
		public JsonSerializer()
		{
			_referenceLoopHandling = ReferenceLoopHandling.Error;
			_missingMemberHandling = MissingMemberHandling.Ignore;
			_nullValueHandling = NullValueHandling.Include;
			_defaultValueHandling = DefaultValueHandling.Include;
			_objectCreationHandling = ObjectCreationHandling.Auto;
			_preserveReferencesHandling = PreserveReferencesHandling.None;
			_constructorHandling = ConstructorHandling.Default;
			_typeNameHandling = TypeNameHandling.None;
			_context = JsonSerializerSettings.DefaultContext;
			_binder = DefaultSerializationBinder.Instance;
		}

		/// <summary>
		/// Creates a new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
		/// </summary>
		/// <param name="settings">The settings to be applied to the <see cref="T:Newtonsoft.Json.JsonSerializer" />.</param>
		/// <returns>A new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.</returns>
		public static JsonSerializer Create(JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			if (settings != null)
			{
				if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
				{
					jsonSerializer.Converters.AddRange(settings.Converters);
				}
				jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
				jsonSerializer.TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat;
				jsonSerializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
				jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
				jsonSerializer.MissingMemberHandling = settings.MissingMemberHandling;
				jsonSerializer.ObjectCreationHandling = settings.ObjectCreationHandling;
				jsonSerializer.NullValueHandling = settings.NullValueHandling;
				jsonSerializer.DefaultValueHandling = settings.DefaultValueHandling;
				jsonSerializer.ConstructorHandling = settings.ConstructorHandling;
				jsonSerializer.Context = settings.Context;
				if (settings.Error != null)
				{
					JsonSerializer jsonSerializer2 = jsonSerializer;
					jsonSerializer2.Error = (EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>)Delegate.Combine(jsonSerializer2.Error, settings.Error);
				}
				if (settings.ContractResolver != null)
				{
					jsonSerializer.ContractResolver = settings.ContractResolver;
				}
				if (settings.ReferenceResolver != null)
				{
					jsonSerializer.ReferenceResolver = settings.ReferenceResolver;
				}
				if (settings.Binder != null)
				{
					jsonSerializer.Binder = settings.Binder;
				}
			}
			return jsonSerializer;
		}

		/// <summary>
		/// Populates the JSON values onto the target object.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.IO.TextReader" /> that contains the JSON structure to reader values from.</param>
		/// <param name="target">The target object to populate values onto.</param>
		public void Populate(TextReader reader, object target)
		{
			Populate(new JsonTextReader(reader), target);
		}

		/// <summary>
		/// Populates the JSON values onto the target object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> that contains the JSON structure to reader values from.</param>
		/// <param name="target">The target object to populate values onto.</param>
		public void Populate(JsonReader reader, object target)
		{
			PopulateInternal(reader, target);
		}

		internal virtual void PopulateInternal(JsonReader reader, object target)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			ValidationUtils.ArgumentNotNull(target, "target");
			JsonSerializerInternalReader jsonSerializerInternalReader = new JsonSerializerInternalReader(this);
			jsonSerializerInternalReader.Populate(reader, target);
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> that contains the JSON structure to deserialize.</param>
		/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
		public object Deserialize(JsonReader reader)
		{
			return Deserialize(reader, null);
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="T:System.IO.StringReader" />
		/// into an instance of the specified type.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.IO.TextReader" /> containing the object.</param>
		/// <param name="objectType">The <see cref="T:System.Type" /> of object being deserialized.</param>
		/// <returns>The instance of <paramref name="objectType" /> being deserialized.</returns>
		public object Deserialize(TextReader reader, Type objectType)
		{
			return Deserialize(new JsonTextReader(reader), objectType);
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />
		/// into an instance of the specified type.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the object.</param>
		/// <typeparam name="T">The type of the object to deserialize.</typeparam>
		/// <returns>The instance of <typeparamref name="T" /> being deserialized.</returns>
		public T Deserialize<T>(JsonReader reader)
		{
			return (T)Deserialize(reader, typeof(T));
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />
		/// into an instance of the specified type.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the object.</param>
		/// <param name="objectType">The <see cref="T:System.Type" /> of object being deserialized.</param>
		/// <returns>The instance of <paramref name="objectType" /> being deserialized.</returns>
		public object Deserialize(JsonReader reader, Type objectType)
		{
			return DeserializeInternal(reader, objectType);
		}

		internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			JsonSerializerInternalReader jsonSerializerInternalReader = new JsonSerializerInternalReader(this);
			return jsonSerializerInternalReader.Deserialize(reader, objectType);
		}

		/// <summary>
		/// Serializes the specified <see cref="T:System.Object" /> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="T:System.IO.TextWriter" />. 
		/// </summary>
		/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> used to write the Json structure.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
		public void Serialize(TextWriter textWriter, object value)
		{
			Serialize(new JsonTextWriter(textWriter), value);
		}

		/// <summary>
		/// Serializes the specified <see cref="T:System.Object" /> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="T:Newtonsoft.Json.JsonWriter" />. 
		/// </summary>
		/// <param name="jsonWriter">The <see cref="T:Newtonsoft.Json.JsonWriter" /> used to write the Json structure.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
		public void Serialize(JsonWriter jsonWriter, object value)
		{
			SerializeInternal(jsonWriter, value);
		}

		internal virtual void SerializeInternal(JsonWriter jsonWriter, object value)
		{
			ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");
			JsonSerializerInternalWriter jsonSerializerInternalWriter = new JsonSerializerInternalWriter(this);
			jsonSerializerInternalWriter.Serialize(jsonWriter, value);
		}

		internal JsonConverter GetMatchingConverter(Type type)
		{
			return GetMatchingConverter(_converters, type);
		}

		internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(objectType, "objectType");
			if (converters != null)
			{
				for (int i = 0; i < converters.Count; i++)
				{
					JsonConverter jsonConverter = converters[i];
					if (jsonConverter.CanConvert(objectType))
					{
						return jsonConverter;
					}
				}
			}
			return null;
		}

		internal void OnError(Newtonsoft.Json.Serialization.ErrorEventArgs e)
		{
			this.Error?.Invoke(this, e);
		}
	}
}
