using System;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public class JsonISerializableContract : JsonContract
	{
		/// <summary>
		/// Gets or sets the ISerializable object constructor.
		/// </summary>
		/// <value>The ISerializable object constructor.</value>
		public ObjectConstructor<object> ISerializableCreator
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonISerializableContract" /> class.
		/// </summary>
		/// <param name="underlyingType">The underlying type for the contract.</param>
		public JsonISerializableContract(Type underlyingType)
			: base(underlyingType)
		{
		}
	}
}
