using System;
using HumanStoryteller.NewtonsoftShell.System.Runtime.CompilerServices;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization;

	
	
	public class ErrorContext
	{
		internal bool Traced
		{
			get;
			set;
		}

		public Exception Error
		{
			get;
		}

		
		[field: Nullable(2)]
		public object OriginalObject
		{
			
			get;
		}

		
		[field: Nullable(2)]
		public object Member
		{
			
			get;
		}

		public string Path
		{
			get;
		}

		public bool Handled
		{
			get;
			set;
		}

		internal ErrorContext( object originalObject,  object member, string path, Exception error)
		{
			OriginalObject = originalObject;
			Member = member;
			Error = error;
			Path = path;
		}
	}
