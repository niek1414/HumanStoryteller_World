using System;

namespace HumanStoryteller.NewtonsoftShell.System.Diagnostics.CodeAnalysis;

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	internal sealed class NotNullWhenAttribute : Attribute
	{
		public bool ReturnValue
		{
			get;
		}

		public NotNullWhenAttribute(bool returnValue)
		{
			ReturnValue = returnValue;
		}
	}
