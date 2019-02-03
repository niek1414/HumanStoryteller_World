namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies the member serialization options for the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public enum MemberSerialization
	{
		/// <summary>
		/// All members are serialized by default. Members can be excluded using the <see cref="T:Newtonsoft.Json.JsonIgnoreAttribute" />.
		/// </summary>
		OptOut,
		/// <summary>
		/// Only members must be marked with the <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" /> are serialized.
		/// </summary>
		OptIn
	}
}
