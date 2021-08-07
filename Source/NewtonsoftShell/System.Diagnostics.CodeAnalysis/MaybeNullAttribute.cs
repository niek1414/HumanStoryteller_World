using System;

namespace HumanStoryteller.NewtonsoftShell.System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
	internal sealed class MaybeNullAttribute : Attribute
	{
	}
}
