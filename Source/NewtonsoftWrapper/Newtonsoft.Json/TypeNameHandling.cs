using System;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies type name handling options for the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	[Flags]
	public enum TypeNameHandling
	{
		/// <summary>
		/// Do not include the .NET type name when serializing types.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Include the .NET type name when serializing into a JSON object structure.
		/// </summary>
		Objects = 0x1,
		/// <summary>
		/// Include the .NET type name when serializing into a JSON array structure.
		/// </summary>
		Arrays = 0x2,
		/// <summary>
		/// Include the .NET type name when the type of the object being serialized is not the same as its declared type.
		/// </summary>
		Auto = 0x4,
		/// <summary>
		/// Always include the .NET type name when serializing.
		/// </summary>
		All = 0x3
	}
}
