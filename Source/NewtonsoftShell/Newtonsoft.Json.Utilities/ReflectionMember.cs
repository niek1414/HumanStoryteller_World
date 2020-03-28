using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities
{
	
	
	internal class ReflectionMember
	{
		public Type MemberType
		{
			get;
			set;
		}

		
		[field: Nullable(new byte[]
		{
			2,
			1,
			2
		})]
		public Func<object, object> Getter
		{
			[return: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			get;
			[param: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			set;
		}

		
		[field: Nullable(new byte[]
		{
			2,
			1,
			2
		})]
		public Action<object, object> Setter
		{
			[return: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			get;
			[param: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			set;
		}
	}
}
