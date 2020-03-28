using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	
	
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
}
