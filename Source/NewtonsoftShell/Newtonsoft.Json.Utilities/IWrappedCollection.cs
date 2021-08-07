using System.Collections;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities
{
	internal interface IWrappedCollection : IList, ICollection, IEnumerable
	{
		
		object UnderlyingCollection
		{
			
			get;
		}
	}
}
