using System;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization;

	public class JsonLinqContract : JsonContract
	{
		
		public JsonLinqContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Linq;
		}
	}
