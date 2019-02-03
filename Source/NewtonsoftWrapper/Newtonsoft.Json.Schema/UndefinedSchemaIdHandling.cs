namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// Specifies undefined schema Id handling options for the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaGenerator" />.
	/// </summary>
	public enum UndefinedSchemaIdHandling
	{
		/// <summary>
		/// Do not infer a schema Id.
		/// </summary>
		None,
		/// <summary>
		/// Use the .NET type name as the schema Id.
		/// </summary>
		UseTypeName,
		/// <summary>
		/// Use the assembly qualified .NET type name as the schema Id.
		/// </summary>
		UseAssemblyQualifiedName
	}
}
