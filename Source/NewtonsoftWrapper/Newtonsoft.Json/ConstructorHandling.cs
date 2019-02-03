namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies how constructors are used when initializing objects during deserialization by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public enum ConstructorHandling
	{
		/// <summary>
		/// First attempt to use the public default constructor then fall back to single paramatized constructor.
		/// </summary>
		Default,
		/// <summary>
		/// Allow Json.NET to use a non-public default constructor.
		/// </summary>
		AllowNonPublicDefaultConstructor
	}
}
