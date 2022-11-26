using System;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class JsonIgnoreAttribute : Attribute
	{
	}
