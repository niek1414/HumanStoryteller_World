using System;
using HumanStoryteller.NewtonsoftShell.System.Runtime.CompilerServices;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization;

	
	
	public class ErrorEventArgs : EventArgs
	{
		
		[field: Nullable(2)]
		public object CurrentObject
		{
			
			get;
		}

		public ErrorContext ErrorContext
		{
			get;
		}

		public ErrorEventArgs( object currentObject, ErrorContext errorContext)
		{
			CurrentObject = currentObject;
			ErrorContext = errorContext;
		}
	}
