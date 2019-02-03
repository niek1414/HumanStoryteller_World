using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a collection of <see cref="T:Newtonsoft.Json.Linq.JToken" /> objects.
	/// </summary>
	/// <typeparam name="T">The type of token</typeparam>
	public interface IJEnumerable<T> : IEnumerable<T>, IEnumerable where T : JToken
	{
		/// <summary>
		/// Gets the <see cref="T:Newtonsoft.Json.Linq.IJEnumerable`1" /> with the specified key.
		/// </summary>
		/// <value></value>
		IJEnumerable<JToken> this[object key]
		{
			get;
		}
	}
}
