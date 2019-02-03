using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Instructs the <see cref="T:Newtonsoft.Json.JsonSerializer" /> to use the specified <see cref="T:Newtonsoft.Json.JsonConverter" /> when serializing the member or class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class JsonConverterAttribute : Attribute
	{
		private readonly Type _converterType;

		/// <summary>
		/// Gets the type of the converter.
		/// </summary>
		/// <value>The type of the converter.</value>
		public Type ConverterType => _converterType;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonConverterAttribute" /> class.
		/// </summary>
		/// <param name="converterType">Type of the converter.</param>
		public JsonConverterAttribute(Type converterType)
		{
			if (converterType == null)
			{
				throw new ArgumentNullException("converterType");
			}
			_converterType = converterType;
		}

		internal static JsonConverter CreateJsonConverterInstance(Type converterType)
		{
			try
			{
				return (JsonConverter)Activator.CreateInstance(converterType);
			}
			catch (Exception innerException)
			{
				throw new Exception("Error creating {0}".FormatWith(CultureInfo.InvariantCulture, converterType), innerException);
			}
		}
	}
}
