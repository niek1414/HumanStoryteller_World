using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Resolves member mappings for a type, camel casing property names.
	/// </summary>
	public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver" /> class.
		/// </summary>
		public CamelCasePropertyNamesContractResolver()
			: base(shareCache: true)
		{
		}

		/// <summary>
		/// Resolves the name of the property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>The property name camel cased.</returns>
		protected override string ResolvePropertyName(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				return propertyName;
			}
			if (!char.IsUpper(propertyName[0]))
			{
				return propertyName;
			}
			string text = char.ToLower(propertyName[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
			if (propertyName.Length > 1)
			{
				text += propertyName.Substring(1);
			}
			return text;
		}
	}
}
