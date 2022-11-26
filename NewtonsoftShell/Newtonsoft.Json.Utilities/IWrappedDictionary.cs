using System.Collections;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities;

	internal interface IWrappedDictionary : IDictionary, ICollection, IEnumerable
	{
		
		object UnderlyingDictionary
		{
			
			get;
		}
	}
