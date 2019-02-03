using System;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public class JsonPrimitiveContract : JsonContract
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonPrimitiveContract" /> class.
		/// </summary>
		/// <param name="underlyingType">The underlying type for the contract.</param>
		public JsonPrimitiveContract(Type underlyingType)
			: base(underlyingType)
		{
		}
	}
}
