using System;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public class JsonObjectContract : JsonContract
	{
		/// <summary>
		/// Gets or sets the object member serialization.
		/// </summary>
		/// <value>The member object serialization.</value>
		public MemberSerialization MemberSerialization
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the object's properties.
		/// </summary>
		/// <value>The object's properties.</value>
		public JsonPropertyCollection Properties
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the parametrized constructor used to create the object.
		/// </summary>
		/// <value>The parametrized constructor.</value>
		public ConstructorInfo ParametrizedConstructor
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" /> class.
		/// </summary>
		/// <param name="underlyingType">The underlying type for the contract.</param>
		public JsonObjectContract(Type underlyingType)
			: base(underlyingType)
		{
			Properties = new JsonPropertyCollection(this);
		}
	}
}
