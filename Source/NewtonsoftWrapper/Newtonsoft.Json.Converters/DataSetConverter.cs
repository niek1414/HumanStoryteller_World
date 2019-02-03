using System;
using System.Data;

namespace Newtonsoft.Json.Converters
{
//	/// <summary>
//	/// Converts a <see cref="T:System.Data.DataSet" /> to and from JSON.
//	/// </summary>
//	public class DataSetConverter : JsonConverter
//	{
//		/// <summary>
//		/// Writes the JSON representation of the object.
//		/// </summary>
//		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
//		/// <param name="value">The value.</param>
//		/// <param name="serializer">The calling serializer.</param>
//		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//		{
//			DataSet dataSet = (DataSet)value;
//			DataTableConverter dataTableConverter = new DataTableConverter();
//			writer.WriteStartObject();
//			foreach (DataTable table in dataSet.Tables)
//			{
//				writer.WritePropertyName(table.TableName);
//				dataTableConverter.WriteJson(writer, table, serializer);
//			}
//			writer.WriteEndObject();
//		}
//
//		/// <summary>
//		/// Reads the JSON representation of the object.
//		/// </summary>
//		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
//		/// <param name="objectType">Type of the object.</param>
//		/// <param name="existingValue">The existing value of object being read.</param>
//		/// <param name="serializer">The calling serializer.</param>
//		/// <returns>The object value.</returns>
//		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//		{
//			DataSet dataSet = new DataSet();
//			DataTableConverter dataTableConverter = new DataTableConverter();
//			reader.Read();
//			while (reader.TokenType == JsonToken.PropertyName)
//			{
//				DataTable table = (DataTable)dataTableConverter.ReadJson(reader, typeof(DataTable), null, serializer);
//				dataSet.Tables.Add(table);
//			}
//			reader.Read();
//			return dataSet;
//		}
//
//		/// <summary>
//		/// Determines whether this instance can convert the specified value type.
//		/// </summary>
//		/// <param name="valueType">Type of the value.</param>
//		/// <returns>
//		/// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
//		/// </returns>
//		public override bool CanConvert(Type valueType)
//		{
//			return valueType == typeof(DataSet);
//		}
//	}
}
