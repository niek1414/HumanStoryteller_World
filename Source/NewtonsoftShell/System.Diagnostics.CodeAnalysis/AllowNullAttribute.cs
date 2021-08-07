using System;

namespace HumanStoryteller.NewtonsoftShell.System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	internal sealed class AllowNullAttribute : Attribute
	{
	}
}
