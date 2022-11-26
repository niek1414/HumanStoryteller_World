using System;
using HumanStoryteller.NewtonsoftShell.System.Runtime.CompilerServices;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization;

	public class JsonISerializableContract : JsonContainerContract
	{
		
		[field: Nullable(new byte[]
		{
			2,
			1
		})]
		public ObjectConstructor<object> ISerializableCreator
		{
			[return: Nullable(new byte[]
			{
				2,
				1
			})]
			get;
			[param: Nullable(new byte[]
			{
				2,
				1
			})]
			set;
		}

		
		public JsonISerializableContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Serializable;
		}
	}
