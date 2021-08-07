using System;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization
{
	
	public interface IContractResolver
	{
		JsonContract ResolveContract(Type type);
	}
}
