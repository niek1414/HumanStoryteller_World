using System;

namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// The value types allowed by the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" />.
	/// </summary>
	[Flags]
	public enum JsonSchemaType
	{
		/// <summary>
		/// No type specified.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// String type.
		/// </summary>
		String = 0x1,
		/// <summary>
		/// Float type.
		/// </summary>
		Float = 0x2,
		/// <summary>
		/// Integer type.
		/// </summary>
		Integer = 0x4,
		/// <summary>
		/// Boolean type.
		/// </summary>
		Boolean = 0x8,
		/// <summary>
		/// Object type.
		/// </summary>
		Object = 0x10,
		/// <summary>
		/// Array type.
		/// </summary>
		Array = 0x20,
		/// <summary>
		/// Null type.
		/// </summary>
		Null = 0x40,
		/// <summary>
		/// Any type.
		/// </summary>
		Any = 0x7F
	}
}
