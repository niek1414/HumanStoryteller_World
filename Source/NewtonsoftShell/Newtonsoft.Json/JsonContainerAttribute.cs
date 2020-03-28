using Newtonsoft.Json.Serialization;
using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json
{
	
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public abstract class JsonContainerAttribute : Attribute
	{
		internal bool? _isReference;

		internal bool? _itemIsReference;

		internal ReferenceLoopHandling? _itemReferenceLoopHandling;

		internal TypeNameHandling? _itemTypeNameHandling;

		private Type _namingStrategyType;

		
		private object[] _namingStrategyParameters;

		public string Id
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public Type ItemConverterType
		{
			get;
			set;
		}

		
		[field: Nullable(new byte[]
		{
			2,
			1
		})]
		public object[] ItemConverterParameters
		{
			[return: Nullable(new byte[]
			{
				2,
				1
			})]
			get;
			[param: Nullable(new byte[]
			{
				2,
				1
			})]
			set;
		}

		public Type NamingStrategyType
		{
			get
			{
				return _namingStrategyType;
			}
			set
			{
				_namingStrategyType = value;
				NamingStrategyInstance = null;
			}
		}

		
		public object[] NamingStrategyParameters
		{
			[return: Nullable(new byte[]
			{
				2,
				1
			})]
			get
			{
				return _namingStrategyParameters;
			}
			[param: Nullable(new byte[]
			{
				2,
				1
			})]
			set
			{
				_namingStrategyParameters = value;
				NamingStrategyInstance = null;
			}
		}

		internal NamingStrategy NamingStrategyInstance
		{
			get;
			set;
		}

		public bool IsReference
		{
			get
			{
				return _isReference.GetValueOrDefault();
			}
			set
			{
				_isReference = value;
			}
		}

		public bool ItemIsReference
		{
			get
			{
				return _itemIsReference.GetValueOrDefault();
			}
			set
			{
				_itemIsReference = value;
			}
		}

		public ReferenceLoopHandling ItemReferenceLoopHandling
		{
			get
			{
				return _itemReferenceLoopHandling.GetValueOrDefault();
			}
			set
			{
				_itemReferenceLoopHandling = value;
			}
		}

		public TypeNameHandling ItemTypeNameHandling
		{
			get
			{
				return _itemTypeNameHandling.GetValueOrDefault();
			}
			set
			{
				_itemTypeNameHandling = value;
			}
		}

		protected JsonContainerAttribute()
		{
		}

		protected JsonContainerAttribute(string id)
		{
			Id = id;
		}
	}
}
