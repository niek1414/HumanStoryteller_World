using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// A collection of <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> objects.
	/// </summary>
	public class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
	{
		private readonly JsonObjectContract _contract;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonPropertyCollection" /> class.
		/// </summary>
		/// <param name="contract">The contract.</param>
		public JsonPropertyCollection(JsonObjectContract contract)
		{
			ValidationUtils.ArgumentNotNull(contract, "contract");
			_contract = contract;
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(JsonProperty item)
		{
			return item.PropertyName;
		}

		/// <summary>
		/// Adds a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> object.
		/// </summary>
		/// <param name="property">The property to add to the collection.</param>
		public void AddProperty(JsonProperty property)
		{
			if (Contains(property.PropertyName))
			{
				if (property.Ignored)
				{
					return;
				}
				JsonProperty jsonProperty = base[property.PropertyName];
				if (!jsonProperty.Ignored)
				{
					throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _contract.UnderlyingType));
				}
				Remove(jsonProperty);
			}
			Add(property);
		}

		/// <summary>
		/// Gets the closest matching <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> object.
		/// First attempts to get an exact case match of propertyName and then
		/// a case insensitive match.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>A matching property if found.</returns>
		public JsonProperty GetClosestMatchProperty(string propertyName)
		{
			JsonProperty property = GetProperty(propertyName, StringComparison.Ordinal);
			if (property == null)
			{
				property = GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);
			}
			return property;
		}

		/// <summary>
		/// Gets a property by property name.
		/// </summary>
		/// <param name="propertyName">The name of the property to get.</param>
		/// <param name="comparisonType">Type property name string comparison.</param>
		/// <returns>A matching property if found.</returns>
		public JsonProperty GetProperty(string propertyName, StringComparison comparisonType)
		{
			using (IEnumerator<JsonProperty> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					JsonProperty current = enumerator.Current;
					if (string.Equals(propertyName, current.PropertyName, comparisonType))
					{
						return current;
					}
				}
			}
			return null;
		}
	}
}
