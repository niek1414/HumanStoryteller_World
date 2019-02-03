using System;
using System.Data;

namespace Newtonsoft.Json.Converters
{
//	/// <summary>
//	/// Converts a <see cref="T:System.Data.DataTable" /> to and from JSON.
//	/// </summary>
//	public class DataTableConverter : JsonConverter
//	{
//		/// <summary>
//		/// Writes the JSON representation of the object.
//		/// </summary>
//		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
//		/// <param name="value">The value.</param>
//		/// <param name="serializer">The calling serializer.</param>
//		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//		{
//			DataTable dataTable = (DataTable)value;
//			writer.WriteStartArray();
//			foreach (DataRow row in dataTable.Rows)
//			{
//				writer.WriteStartObject();
//				foreach (DataColumn column in row.Table.Columns)
//				{
//					writer.WritePropertyName(column.ColumnName);
//					serializer.Serialize(writer, row[column]);
//				}
//				writer.WriteEndObject();
//			}
//			writer.WriteEndArray();
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
//			DataTable dataTable;
//			if (reader.TokenType == JsonToken.PropertyName)
//			{
//				dataTable = new DataTable((string)reader.Value);
//				reader.Read();
//			}
//			else
//			{
//				dataTable = new DataTable();
//			}
//			reader.Read();
//			while (reader.TokenType == JsonToken.StartObject)
//			{
//				DataRow dataRow = dataTable.NewRow();
//				reader.Read();
//				while (reader.TokenType == JsonToken.PropertyName)
//				{
//					string text = (string)reader.Value;
//					reader.Read();
//					if (!dataTable.Columns.Contains(text))
//					{
//						Type columnDataType = GetColumnDataType(reader.TokenType);
//						dataTable.Columns.Add(new DataColumn(text, columnDataType));
//					}
//					dataRow[text] = reader.Value;
//					reader.Read();
//				}
//				dataRow.EndEdit();
//				dataTable.Rows.Add(dataRow);
//				reader.Read();
//			}
//			reader.Read();
//			return dataTable;
//		}
//
//		private static Type GetColumnDataType(JsonToken tokenType)
//		{
//			switch (tokenType)
//			{
//			case JsonToken.Integer:
//				return typeof(long);
//			case JsonToken.Float:
//				return typeof(double);
//			case JsonToken.String:
//			case JsonToken.Null:
//			case JsonToken.Undefined:
//				return typeof(string);
//			case JsonToken.Boolean:
//				return typeof(bool);
//			case JsonToken.Date:
//				return typeof(DateTime);
//			default:
//				throw new ArgumentOutOfRangeException();
//			}
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
//			return valueType == typeof(DataTable);
//		}
//	}
}
