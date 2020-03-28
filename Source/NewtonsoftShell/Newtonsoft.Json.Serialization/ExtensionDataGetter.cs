using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	[return: Nullable(new byte[]
	{
		2,
		0,
		1,
		1
	})]
	public delegate IEnumerable<KeyValuePair<object, object>> ExtensionDataGetter(object o);
}
