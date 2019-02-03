using Newtonsoft.Json.Utilities;
using System;

namespace Newtonsoft.Json.Schema
{
	/// <summary>
	/// Returns detailed information related to the <see cref="T:Newtonsoft.Json.Schema.ValidationEventHandler" />.
	/// </summary>
	public class ValidationEventArgs : EventArgs
	{
		private readonly JsonSchemaException _ex;

		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaException" /> associated with the validation event.
		/// </summary>
		/// <value>The JsonSchemaException associated with the validation event.</value>
		public JsonSchemaException Exception => _ex;

		/// <summary>
		/// Gets the text description corresponding to the validation event.
		/// </summary>
		/// <value>The text description.</value>
		public string Message => _ex.Message;

		internal ValidationEventArgs(JsonSchemaException ex)
		{
			ValidationUtils.ArgumentNotNull(ex, "ex");
			_ex = ex;
		}
	}
}
