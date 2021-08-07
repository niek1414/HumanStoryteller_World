using System;

namespace HumanStoryteller.NewtonsoftShell.System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal class DoesNotReturnIfAttribute : Attribute
	{
		public bool ParameterValue
		{
			get;
		}

		public DoesNotReturnIfAttribute(bool parameterValue)
		{
			ParameterValue = parameterValue;
		}
	}
}
