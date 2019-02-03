using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// Contains the JSON schema extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Determines whether the <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid.
		/// </summary>
		/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
		/// <param name="schema">The schema to test with.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValid(this JToken source, JsonSchema schema)
		{
			bool valid = true;
			source.Validate(schema, delegate
			{
				valid = false;
			});
			return valid;
		}

		/// <summary>
		/// Validates the specified <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
		/// <param name="schema">The schema to test with.</param>
		public static void Validate(this JToken source, JsonSchema schema)
		{
			source.Validate(schema, null);
		}

		/// <summary>
		/// Validates the specified <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
		/// <param name="schema">The schema to test with.</param>
		/// <param name="validationEventHandler">The validation event handler.</param>
		public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
		{
			ValidationUtils.ArgumentNotNull(source, "source");
			ValidationUtils.ArgumentNotNull(schema, "schema");
			using (JsonValidatingReader jsonValidatingReader = new JsonValidatingReader(source.CreateReader()))
			{
				jsonValidatingReader.Schema = schema;
				if (validationEventHandler != null)
				{
					jsonValidatingReader.ValidationEventHandler += validationEventHandler;
				}
				while (jsonValidatingReader.Read())
				{
				}
			}
		}
	}
}
