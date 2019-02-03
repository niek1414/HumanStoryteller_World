using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	public class JsonArrayContract : JsonContract
	{
		private readonly bool _isCollectionItemTypeNullableType;

		private readonly Type _genericCollectionDefinitionType;

		private Type _genericWrapperType;

		private MethodCall<object, object> _genericWrapperCreator;

		internal Type CollectionItemType
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonArrayContract" /> class.
		/// </summary>
		/// <param name="underlyingType">The underlying type for the contract.</param>
		public JsonArrayContract(Type underlyingType)
			: base(underlyingType)
		{
			if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
			{
				CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
			}
			else
			{
				CollectionItemType = ReflectionUtils.GetCollectionItemType(base.UnderlyingType);
			}
			if (CollectionItemType != null)
			{
				_isCollectionItemTypeNullableType = ReflectionUtils.IsNullableType(CollectionItemType);
			}
			if (IsTypeGenericCollectionInterface(base.UnderlyingType))
			{
				base.CreatedType = ReflectionUtils.MakeGenericType(typeof(List<>), CollectionItemType);
			}
		}

		internal IWrappedCollection CreateWrapper(object list)
		{
			if ((list is IList && (CollectionItemType == null || !_isCollectionItemTypeNullableType)) || base.UnderlyingType.IsArray)
			{
				return new CollectionWrapper<object>((IList)list);
			}
			if (_genericWrapperType == null)
			{
				_genericWrapperType = ReflectionUtils.MakeGenericType(typeof(CollectionWrapper<>), CollectionItemType);
				ConstructorInfo constructor = _genericWrapperType.GetConstructor(new Type[1]
				{
					_genericCollectionDefinitionType
				});
				_genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructor);
			}
			return (IWrappedCollection)_genericWrapperCreator(null, list);
		}

		private bool IsTypeGenericCollectionInterface(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition != typeof(IList<>) && genericTypeDefinition != typeof(ICollection<>))
			{
				return genericTypeDefinition == typeof(IEnumerable<>);
			}
			return true;
		}
	}
}
