using System;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies reference handling options for the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	[Flags]
	public enum PreserveReferencesHandling
	{
		/// <summary>
		/// Do not preserve references when serializing types.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Preserve references when serializing into a JSON object structure.
		/// </summary>
		Objects = 0x1,
		/// <summary>
		/// Preserve references when serializing into a JSON array structure.
		/// </summary>
		Arrays = 0x2,
		/// <summary>
		/// Preserve references when serializing.
		/// </summary>
		All = 0x3
	}
}
