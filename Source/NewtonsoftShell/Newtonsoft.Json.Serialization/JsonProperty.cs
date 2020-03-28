using Newtonsoft.Json.Utilities;
using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	
	
	public class JsonProperty
	{
		internal Required? _required;

		internal bool _hasExplicitDefaultValue;

		private object _defaultValue;

		private bool _hasGeneratedDefaultValue;

		private string _propertyName;

		internal bool _skipPropertyNameEscape;

		private Type _propertyType;

		internal JsonContract PropertyContract
		{
			get;
			set;
		}

		public string PropertyName
		{
			get
			{
				return _propertyName;
			}
			set
			{
				_propertyName = value;
				_skipPropertyNameEscape = !JavaScriptUtils.ShouldEscapeJavaScriptString(_propertyName, JavaScriptUtils.HtmlCharEscapeFlags);
			}
		}

		public Type DeclaringType
		{
			get;
			set;
		}

		public int? Order
		{
			get;
			set;
		}

		public string UnderlyingName
		{
			get;
			set;
		}

		public IValueProvider ValueProvider
		{
			get;
			set;
		}

		public IAttributeProvider AttributeProvider
		{
			get;
			set;
		}

		public Type PropertyType
		{
			get
			{
				return _propertyType;
			}
			set
			{
				if (_propertyType != value)
				{
					_propertyType = value;
					_hasGeneratedDefaultValue = false;
				}
			}
		}

		public JsonConverter Converter
		{
			get;
			set;
		}

		[Obsolete("MemberConverter is obsolete. Use Converter instead.")]
		public JsonConverter MemberConverter
		{
			get
			{
				return Converter;
			}
			set
			{
				Converter = value;
			}
		}

		public bool Ignored
		{
			get;
			set;
		}

		public bool Readable
		{
			get;
			set;
		}

		public bool Writable
		{
			get;
			set;
		}

		public bool HasMemberAttribute
		{
			get;
			set;
		}

		public object DefaultValue
		{
			get
			{
				if (!_hasExplicitDefaultValue)
				{
					return null;
				}
				return _defaultValue;
			}
			set
			{
				_hasExplicitDefaultValue = true;
				_defaultValue = value;
			}
		}

		public Required Required
		{
			get
			{
				return _required.GetValueOrDefault();
			}
			set
			{
				_required = value;
			}
		}

		public bool IsRequiredSpecified => _required.HasValue;

		public bool? IsReference
		{
			get;
			set;
		}

		public NullValueHandling? NullValueHandling
		{
			get;
			set;
		}

		public DefaultValueHandling? DefaultValueHandling
		{
			get;
			set;
		}

		public ReferenceLoopHandling? ReferenceLoopHandling
		{
			get;
			set;
		}

		public ObjectCreationHandling? ObjectCreationHandling
		{
			get;
			set;
		}

		public TypeNameHandling? TypeNameHandling
		{
			get;
			set;
		}

		
		[field: Nullable(new byte[]
		{
			2,
			1
		})]
		public Predicate<object> ShouldSerialize
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

		
		[field: Nullable(new byte[]
		{
			2,
			1
		})]
		public Predicate<object> ShouldDeserialize
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

		
		[field: Nullable(new byte[]
		{
			2,
			1
		})]
		public Predicate<object> GetIsSpecified
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

		
		[field: Nullable(new byte[]
		{
			2,
			1,
			2
		})]
		public Action<object, object> SetIsSpecified
		{
			[return: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			get;
			[param: Nullable(new byte[]
			{
				2,
				1,
				2
			})]
			set;
		}

		public JsonConverter ItemConverter
		{
			get;
			set;
		}

		public bool? ItemIsReference
		{
			get;
			set;
		}

		public TypeNameHandling? ItemTypeNameHandling
		{
			get;
			set;
		}

		public ReferenceLoopHandling? ItemReferenceLoopHandling
		{
			get;
			set;
		}

		internal object GetResolvedDefaultValue()
		{
			if (_propertyType == null)
			{
				return null;
			}
			if (!_hasExplicitDefaultValue && !_hasGeneratedDefaultValue)
			{
				_defaultValue = ReflectionUtils.GetDefaultValue(_propertyType);
				_hasGeneratedDefaultValue = true;
			}
			return _defaultValue;
		}

		
		public override string ToString()
		{
			return PropertyName ?? string.Empty;
		}

		
		internal void WritePropertyName(JsonWriter writer)
		{
			string propertyName = PropertyName;
			if (_skipPropertyNameEscape)
			{
				writer.WritePropertyName(propertyName, escape: false);
			}
			else
			{
				writer.WritePropertyName(propertyName);
			}
		}
	}
}
