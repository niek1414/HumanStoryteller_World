namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies null value handling options for the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public enum NullValueHandling
	{
		/// <summary>
		/// Include null values when serializing and deserializing objects.
		/// </summary>
		Include,
		/// <summary>
		/// Ignore null values when serializing and deserializing objects.
		/// </summary>
		Ignore
	}
}
