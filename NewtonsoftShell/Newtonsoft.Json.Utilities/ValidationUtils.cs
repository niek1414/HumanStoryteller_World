using System;
using HumanStoryteller.NewtonsoftShell.System.Diagnostics.CodeAnalysis;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities;

	internal static class ValidationUtils
	{
		
		public static void ArgumentNotNull( [NotNull] object value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}
	}
