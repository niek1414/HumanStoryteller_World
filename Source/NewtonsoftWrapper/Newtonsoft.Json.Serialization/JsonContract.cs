using Newtonsoft.Json.Utilities;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public abstract class JsonContract
	{
		/// <summary>
		/// Gets the underlying type for the contract.
		/// </summary>
		/// <value>The underlying type for the contract.</value>
		public Type UnderlyingType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the type created during deserialization.
		/// </summary>
		/// <value>The type created during deserialization.</value>
		public Type CreatedType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether this type contract is serialized as a reference.
		/// </summary>
		/// <value>Whether this type contract is serialized as a reference.</value>
		public bool? IsReference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default <see cref="T:Newtonsoft.Json.JsonConverter" /> for this contract.
		/// </summary>
		/// <value>The converter.</value>
		public JsonConverter Converter
		{
			get;
			set;
		}

		internal JsonConverter InternalConverter
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the method called immediately after deserialization of the object.
		/// </summary>
		/// <value>The method called immediately after deserialization of the object.</value>
		public MethodInfo OnDeserialized
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the method called during deserialization of the object.
		/// </summary>
		/// <value>The method called during deserialization of the object.</value>
		public MethodInfo OnDeserializing
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the method called after serialization of the object graph.
		/// </summary>
		/// <value>The method called after serialization of the object graph.</value>
		public MethodInfo OnSerialized
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the method called before serialization of the object.
		/// </summary>
		/// <value>The method called before serialization of the object.</value>
		public MethodInfo OnSerializing
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default creator method used to create the object.
		/// </summary>
		/// <value>The default creator method used to create the object.</value>
		public Func<object> DefaultCreator
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [default creator non public].
		/// </summary>
		/// <value><c>true</c> if the default object creator is non-public; otherwise, <c>false</c>.</value>
		public bool DefaultCreatorNonPublic
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the method called when an error is thrown during the serialization of the object.
		/// </summary>
		/// <value>The method called when an error is thrown during the serialization of the object.</value>
		public MethodInfo OnError
		{
			get;
			set;
		}

		internal void InvokeOnSerializing(object o, StreamingContext context)
		{
			if (OnSerializing != null)
			{
				OnSerializing.Invoke(o, new object[1]
				{
					context
				});
			}
		}

		internal void InvokeOnSerialized(object o, StreamingContext context)
		{
			if (OnSerialized != null)
			{
				OnSerialized.Invoke(o, new object[1]
				{
					context
				});
			}
		}

		internal void InvokeOnDeserializing(object o, StreamingContext context)
		{
			if (OnDeserializing != null)
			{
				OnDeserializing.Invoke(o, new object[1]
				{
					context
				});
			}
		}

		internal void InvokeOnDeserialized(object o, StreamingContext context)
		{
			if (OnDeserialized != null)
			{
				OnDeserialized.Invoke(o, new object[1]
				{
					context
				});
			}
		}

		internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
		{
			if (OnError != null)
			{
				OnError.Invoke(o, new object[2]
				{
					context,
					errorContext
				});
			}
		}

		internal JsonContract(Type underlyingType)
		{
			ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
			UnderlyingType = underlyingType;
			CreatedType = underlyingType;
		}
	}
}
