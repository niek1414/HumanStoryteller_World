using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// An in-memory representation of a JSON Schema.
	/// </summary>
	public class JsonSchema
	{
		private readonly string _internalId = Guid.NewGuid().ToString("N");

		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		public string Id
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the object is optional.
		/// </summary>
		public bool? Optional
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the object is read only.
		/// </summary>
		public bool? ReadOnly
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the object is visible to users.
		/// </summary>
		public bool? Hidden
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the object is transient.
		/// </summary>
		public bool? Transient
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the description of the object.
		/// </summary>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the types of values allowed by the object.
		/// </summary>
		/// <value>The type.</value>
		public JsonSchemaType? Type
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the pattern.
		/// </summary>
		/// <value>The pattern.</value>
		public string Pattern
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum length.
		/// </summary>
		/// <value>The minimum length.</value>
		public int? MinimumLength
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum length.
		/// </summary>
		/// <value>The maximum length.</value>
		public int? MaximumLength
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum decimals.
		/// </summary>
		/// <value>The maximum decimals.</value>
		public int? MaximumDecimals
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>The minimum.</value>
		public double? Minimum
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>The maximum.</value>
		public double? Maximum
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum number of items.
		/// </summary>
		/// <value>The minimum number of items.</value>
		public int? MinimumItems
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum number of items.
		/// </summary>
		/// <value>The maximum number of items.</value>
		public int? MaximumItems
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of items.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of items.</value>
		public IList<JsonSchema> Items
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of properties.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of properties.</value>
		public IDictionary<string, JsonSchema> Properties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of additional properties.
		/// </summary>
		/// <value>The <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> of additional properties.</value>
		public JsonSchema AdditionalProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether additional properties are allowed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if additional properties are allowed; otherwise, <c>false</c>.
		/// </value>
		public bool AllowAdditionalProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the required property if this property is present.
		/// </summary>
		/// <value>The required property if this property is present.</value>
		public string Requires
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identity.
		/// </summary>
		/// <value>The identity.</value>
		public IList<string> Identity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the a collection of valid enum values allowed.
		/// </summary>
		/// <value>A collection of valid enum values allowed.</value>
		public IList<JToken> Enum
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a collection of options.
		/// </summary>
		/// <value>A collection of options.</value>
		public IDictionary<JToken, string> Options
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets disallowed types.
		/// </summary>
		/// <value>The disallow types.</value>
		public JsonSchemaType? Disallow
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default value.
		/// </summary>
		/// <value>The default value.</value>
		public JToken Default
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the extend <see cref="T:Newtonsoft.Json.Schema.JsonSchema" />.
		/// </summary>
		/// <value>The extended <see cref="T:Newtonsoft.Json.Schema.JsonSchema" />.</value>
		public JsonSchema Extends
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the format.
		/// </summary>
		/// <value>The format.</value>
		public string Format
		{
			get;
			set;
		}

		internal string InternalId => _internalId;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> class.
		/// </summary>
		public JsonSchema()
		{
			AllowAdditionalProperties = true;
		}

		/// <summary>
		/// Reads a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the JSON Schema to read.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> object representing the JSON Schema.</returns>
		public static JsonSchema Read(JsonReader reader)
		{
			return Read(reader, new JsonSchemaResolver());
		}

		/// <summary>
		/// Reads a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the JSON Schema to read.</param>
		/// <param name="resolver">The <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" /> to use when resolving schema references.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> object representing the JSON Schema.</returns>
		public static JsonSchema Read(JsonReader reader, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			ValidationUtils.ArgumentNotNull(resolver, "resolver");
			JsonSchemaBuilder jsonSchemaBuilder = new JsonSchemaBuilder(resolver);
			return jsonSchemaBuilder.Parse(reader);
		}

		/// <summary>
		/// Load a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from a string that contains schema JSON.
		/// </summary>
		/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> populated from the string that contains JSON.</returns>
		public static JsonSchema Parse(string json)
		{
			return Parse(json, new JsonSchemaResolver());
		}

		/// <summary>
		/// Parses the specified json.
		/// </summary>
		/// <param name="json">The json.</param>
		/// <param name="resolver">The resolver.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> populated from the string that contains JSON.</returns>
		public static JsonSchema Parse(string json, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(json, "json");
			JsonReader reader = new JsonTextReader(new StringReader(json));
			return Read(reader, resolver);
		}

		/// <summary>
		/// Writes this schema to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		public void WriteTo(JsonWriter writer)
		{
			WriteTo(writer, new JsonSchemaResolver());
		}

		/// <summary>
		/// Writes this schema to a <see cref="T:Newtonsoft.Json.JsonWriter" /> using the specified <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" />.
		/// </summary>
		/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
		/// <param name="resolver">The resolver used.</param>
		public void WriteTo(JsonWriter writer, JsonSchemaResolver resolver)
		{
			ValidationUtils.ArgumentNotNull(writer, "writer");
			ValidationUtils.ArgumentNotNull(resolver, "resolver");
			JsonSchemaWriter jsonSchemaWriter = new JsonSchemaWriter(writer, resolver);
			jsonSchemaWriter.WriteSchema(this);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		/// </returns>
		public override string ToString()
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
			jsonTextWriter.Formatting = Formatting.Indented;
			WriteTo(jsonTextWriter);
			return stringWriter.ToString();
		}
	}
}
