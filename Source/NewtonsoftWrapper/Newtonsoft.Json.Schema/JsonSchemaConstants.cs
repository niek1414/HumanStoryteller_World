using System.Collections.Generic;

namespace Newtonsoft.Json.Schema
{
	internal static class JsonSchemaConstants
	{
		public const string TypePropertyName = "type";

		public const string PropertiesPropertyName = "properties";

		public const string ItemsPropertyName = "items";

		public const string OptionalPropertyName = "optional";

		public const string AdditionalPropertiesPropertyName = "additionalProperties";

		public const string RequiresPropertyName = "requires";

		public const string IdentityPropertyName = "identity";

		public const string MinimumPropertyName = "minimum";

		public const string MaximumPropertyName = "maximum";

		public const string MinimumItemsPropertyName = "minItems";

		public const string MaximumItemsPropertyName = "maxItems";

		public const string PatternPropertyName = "pattern";

		public const string MaximumLengthPropertyName = "maxLength";

		public const string MinimumLengthPropertyName = "minLength";

		public const string EnumPropertyName = "enum";

		public const string OptionsPropertyName = "options";

		public const string ReadOnlyPropertyName = "readonly";

		public const string TitlePropertyName = "title";

		public const string DescriptionPropertyName = "description";

		public const string FormatPropertyName = "format";

		public const string DefaultPropertyName = "default";

		public const string TransientPropertyName = "transient";

		public const string MaximumDecimalsPropertyName = "maxDecimal";

		public const string HiddenPropertyName = "hidden";

		public const string DisallowPropertyName = "disallow";

		public const string ExtendsPropertyName = "extends";

		public const string IdPropertyName = "id";

		public const string OptionValuePropertyName = "value";

		public const string OptionLabelPropertyName = "label";

		public const string ReferencePropertyName = "$ref";

		public static readonly IDictionary<string, JsonSchemaType> JsonSchemaTypeMapping = new Dictionary<string, JsonSchemaType>
		{
			{
				"string",
				JsonSchemaType.String
			},
			{
				"object",
				JsonSchemaType.Object
			},
			{
				"integer",
				JsonSchemaType.Integer
			},
			{
				"number",
				JsonSchemaType.Float
			},
			{
				"null",
				JsonSchemaType.Null
			},
			{
				"boolean",
				JsonSchemaType.Boolean
			},
			{
				"array",
				JsonSchemaType.Array
			},
			{
				"any",
				JsonSchemaType.Any
			}
		};
	}
}
