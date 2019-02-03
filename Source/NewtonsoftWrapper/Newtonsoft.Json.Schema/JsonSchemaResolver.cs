using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// Resolves <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from an id.
	/// </summary>
	public class JsonSchemaResolver
	{
		/// <summary>
		/// Gets or sets the loaded schemas.
		/// </summary>
		/// <value>The loaded schemas.</value>
		public IList<JsonSchema> LoadedSchemas
		{
			get;
			protected set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" /> class.
		/// </summary>
		public JsonSchemaResolver()
		{
			LoadedSchemas = new List<JsonSchema>();
		}

		/// <summary>
		/// Gets a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> for the specified id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> for the specified id.</returns>
		public virtual JsonSchema GetSchema(string id)
		{
			return LoadedSchemas.SingleOrDefault((JsonSchema s) => s.Id == id);
		}
	}
}
