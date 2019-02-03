using Newtonsoft.Json.Utilities;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serializationa;
using System.Security.Permissions;

namespace Newtonsoft.Json.Serialization
{
	internal static class JsonTypeReflector
	{
		public const string IdPropertyName = "$id";

		public const string RefPropertyName = "$ref";

		public const string TypePropertyName = "$type";

		public const string ArrayValuesPropertyName = "$values";

		public const string ShouldSerializePrefix = "ShouldSerialize";

		private const string MetadataTypeAttributeTypeName = "System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

		private static readonly ThreadSafeStore<ICustomAttributeProvider, Type> JsonConverterTypeCache = new ThreadSafeStore<ICustomAttributeProvider, Type>(GetJsonConverterTypeFromAttribute);

		private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(GetAssociateMetadataTypeFromAttribute);

		private static Type _cachedMetadataTypeAttributeType;

		private static bool? _dynamicCodeGeneration;

		public static bool DynamicCodeGeneration
		{
			get
			{
				if (!_dynamicCodeGeneration.HasValue)
				{
					try
					{
						new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
						new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Demand();
						new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
						_dynamicCodeGeneration = true;
					}
					catch (Exception)
					{
						_dynamicCodeGeneration = false;
					}
				}
				return _dynamicCodeGeneration.Value;
			}
		}

		public static ReflectionDelegateFactory ReflectionDelegateFactory
		{
			get
			{
				if (DynamicCodeGeneration)
				{
					return DynamicReflectionDelegateFactory.Instance;
				}
				return LateBoundReflectionDelegateFactory.Instance;
			}
		}

		public static JsonContainerAttribute GetJsonContainerAttribute(Type type)
		{
			return CachedAttributeGetter<JsonContainerAttribute>.GetAttribute(type);
		}

		public static JsonObjectAttribute GetJsonObjectAttribute(Type type)
		{
			return GetJsonContainerAttribute(type) as JsonObjectAttribute;
		}

		public static JsonArrayAttribute GetJsonArrayAttribute(Type type)
		{
			return GetJsonContainerAttribute(type) as JsonArrayAttribute;
		}

		public static DataContractAttribute GetDataContractAttribute(Type type)
		{
			return CachedAttributeGetter<DataContractAttribute>.GetAttribute(type);
		}

		public static MemberSerialization GetObjectMemberSerialization(Type objectType)
		{
			JsonObjectAttribute jsonObjectAttribute = GetJsonObjectAttribute(objectType);
			if (jsonObjectAttribute == null)
			{
				DataContractAttribute dataContractAttribute = GetDataContractAttribute(objectType);
				if (dataContractAttribute != null)
				{
					return MemberSerialization.OptIn;
				}
				return MemberSerialization.OptOut;
			}
			return jsonObjectAttribute.MemberSerialization;
		}

		private static Type GetJsonConverterType(ICustomAttributeProvider attributeProvider)
		{
			return JsonConverterTypeCache.Get(attributeProvider);
		}

		private static Type GetJsonConverterTypeFromAttribute(ICustomAttributeProvider attributeProvider)
		{
			return GetAttribute<JsonConverterAttribute>(attributeProvider)?.ConverterType;
		}

		public static JsonConverter GetJsonConverter(ICustomAttributeProvider attributeProvider, Type targetConvertedType)
		{
			Type jsonConverterType = GetJsonConverterType(attributeProvider);
			if (jsonConverterType != null)
			{
				JsonConverter jsonConverter = JsonConverterAttribute.CreateJsonConverterInstance(jsonConverterType);
				if (!jsonConverter.CanConvert(targetConvertedType))
				{
					throw new JsonSerializationException("JsonConverter {0} on {1} is not compatible with member type {2}.".FormatWith(CultureInfo.InvariantCulture, jsonConverter.GetType().Name, attributeProvider, targetConvertedType.Name));
				}
				return jsonConverter;
			}
			return null;
		}

		public static TypeConverter GetTypeConverter(Type type)
		{
			return TypeDescriptor.GetConverter(type);
		}

		private static Type GetAssociatedMetadataType(Type type)
		{
			return AssociatedMetadataTypesCache.Get(type);
		}

		private static Type GetAssociateMetadataTypeFromAttribute(Type type)
		{
			Type metadataTypeAttributeType = GetMetadataTypeAttributeType();
			if (metadataTypeAttributeType == null)
			{
				return null;
			}
			object obj = type.GetCustomAttributes(metadataTypeAttributeType, inherit: true).SingleOrDefault();
			if (obj == null)
			{
				return null;
			}
			IMetadataTypeAttribute metadataTypeAttribute = DynamicCodeGeneration ? DynamicWrapper.CreateWrapper<IMetadataTypeAttribute>(obj) : new LateBoundMetadataTypeAttribute(obj);
			return metadataTypeAttribute.MetadataClassType;
		}

		private static Type GetMetadataTypeAttributeType()
		{
			if (_cachedMetadataTypeAttributeType == null)
			{
				Type type = Type.GetType("System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
				if (type == null)
				{
					return null;
				}
				_cachedMetadataTypeAttributeType = type;
			}
			return _cachedMetadataTypeAttributeType;
		}

		private static T GetAttribute<T>(Type type) where T : Attribute
		{
			Type associatedMetadataType = GetAssociatedMetadataType(type);
			if (associatedMetadataType != null)
			{
				T attribute = ReflectionUtils.GetAttribute<T>(associatedMetadataType, inherit: true);
				if (attribute != null)
				{
					return attribute;
				}
			}
			return ReflectionUtils.GetAttribute<T>(type, inherit: true);
		}

		private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
		{
			Type associatedMetadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
			if (associatedMetadataType != null)
			{
				MemberInfo memberInfo2 = associatedMetadataType.GetMember(memberInfo.Name, memberInfo.MemberType, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).SingleOrDefault();
				if (memberInfo2 != null)
				{
					T attribute = ReflectionUtils.GetAttribute<T>(memberInfo2, inherit: true);
					if (attribute != null)
					{
						return attribute;
					}
				}
			}
			return ReflectionUtils.GetAttribute<T>(memberInfo, inherit: true);
		}

		public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
		{
			Type type = attributeProvider as Type;
			if (type != null)
			{
				return GetAttribute<T>(type);
			}
			MemberInfo memberInfo = attributeProvider as MemberInfo;
			if (memberInfo != null)
			{
				return GetAttribute<T>(memberInfo);
			}
			return ReflectionUtils.GetAttribute<T>(attributeProvider, inherit: true);
		}
	}
}
