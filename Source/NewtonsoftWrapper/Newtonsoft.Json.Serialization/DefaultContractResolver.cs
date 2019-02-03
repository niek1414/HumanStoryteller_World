using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DataContractAttribute = System.Runtime.Serializationa.DataContractAttribute;
using DataMemberAttribute = System.Runtime.Serializationa.DataMemberAttribute;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Used by <see cref="T:Newtonsoft.Json.JsonSerializer" /> to resolves a <see cref="T:Newtonsoft.Json.Serialization.JsonContract" /> for a given <see cref="T:System.Type" />.
	/// </summary>
	public class DefaultContractResolver : IContractResolver
	{
		internal static readonly IContractResolver Instance = new DefaultContractResolver(shareCache: true);

		private static readonly IList<JsonConverter> BuiltInConverters = new List<JsonConverter>
		{
			new EntityKeyMemberConverter(),
			new BinaryConverter(),
			new KeyValuePairConverter(),
			new XmlNodeConverter(),
//			new DataSetConverter(),
//			new DataTableConverter(),
			new BsonObjectIdConverter()
		};

		private static Dictionary<ResolverContractKey, JsonContract> _sharedContractCache;

		private static readonly object _typeContractCacheLock = new object();

		private Dictionary<ResolverContractKey, JsonContract> _instanceContractCache;

		private readonly bool _sharedCache;

		/// <summary>
		/// Gets a value indicating whether members are being get and set using dynamic code generation.
		/// This value is determined by the runtime permissions available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if using dynamic code generation; otherwise, <c>false</c>.
		/// </value>
		public bool DynamicCodeGeneration => JsonTypeReflector.DynamicCodeGeneration;

		/// <summary>
		/// Gets or sets the default members search flags.
		/// </summary>
		/// <value>The default members search flags.</value>
		public BindingFlags DefaultMembersSearchFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether compiler generated members should be serialized.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if serialized compiler generated members; otherwise, <c>false</c>.
		/// </value>
		public bool SerializeCompilerGeneratedMembers
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.DefaultContractResolver" /> class.
		/// </summary>
		public DefaultContractResolver()
			: this(shareCache: false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.DefaultContractResolver" /> class.
		/// </summary>
		/// <param name="shareCache">
		/// If set to <c>true</c> the <see cref="T:Newtonsoft.Json.Serialization.DefaultContractResolver" /> will use a cached shared with other resolvers of the same type.
		/// Sharing the cache will significantly performance because expensive reflection will only happen once but could cause unexpected
		/// behavior if different instances of the resolver are suppose to produce different results. When set to false it is highly
		/// recommended to reuse <see cref="T:Newtonsoft.Json.Serialization.DefaultContractResolver" /> instances with the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
		/// </param>
		public DefaultContractResolver(bool shareCache)
		{
			DefaultMembersSearchFlags = (BindingFlags.Instance | BindingFlags.Public);
			_sharedCache = shareCache;
		}

		private Dictionary<ResolverContractKey, JsonContract> GetCache()
		{
			if (_sharedCache)
			{
				return _sharedContractCache;
			}
			return _instanceContractCache;
		}

		private void UpdateCache(Dictionary<ResolverContractKey, JsonContract> cache)
		{
			if (_sharedCache)
			{
				_sharedContractCache = cache;
			}
			else
			{
				_instanceContractCache = cache;
			}
		}

		/// <summary>
		/// Resolves the contract for a given type.
		/// </summary>
		/// <param name="type">The type to resolve a contract for.</param>
		/// <returns>The contract for a given type.</returns>
		public virtual JsonContract ResolveContract(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			ResolverContractKey key = new ResolverContractKey(GetType(), type);
			Dictionary<ResolverContractKey, JsonContract> cache = GetCache();
			if (cache == null || !cache.TryGetValue(key, out JsonContract value))
			{
				value = CreateContract(type);
				lock (_typeContractCacheLock)
				{
					cache = GetCache();
					Dictionary<ResolverContractKey, JsonContract> dictionary = (cache != null) ? new Dictionary<ResolverContractKey, JsonContract>(cache) : new Dictionary<ResolverContractKey, JsonContract>();
					dictionary[key] = value;
					UpdateCache(dictionary);
					return value;
				}
			}
			return value;
		}

		/// <summary>
		/// Gets the serializable members for the type.
		/// </summary>
		/// <param name="objectType">The type to get serializable members for.</param>
		/// <returns>The serializable members for the type.</returns>
		protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
			List<MemberInfo> list = (from m in ReflectionUtils.GetFieldsAndProperties(objectType, DefaultMembersSearchFlags)
			where !ReflectionUtils.IsIndexedProperty(m)
			select m).ToList();
			List<MemberInfo> list2 = (from m in ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where !ReflectionUtils.IsIndexedProperty(m)
			select m).ToList();
			List<MemberInfo> list3 = new List<MemberInfo>();
			foreach (MemberInfo item in list2)
			{
				if (SerializeCompilerGeneratedMembers || !item.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
				{
					if (list.Contains(item))
					{
						list3.Add(item);
					}
					else if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(item) != null)
					{
						list3.Add(item);
					}
					else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(item) != null)
					{
						list3.Add(item);
					}
				}
			}
			if (objectType.AssignableToTypeName("System.Data.Objects.DataClasses.EntityObject", out Type _))
			{
				list3 = list3.Where(ShouldSerializeEntityMember).ToList();
			}
			return list3;
		}

		private bool ShouldSerializeEntityMember(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null && propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().FullName == "System.Data.Objects.DataClasses.EntityReference`1")
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" /> for the given type.</returns>
		protected virtual JsonObjectContract CreateObjectContract(Type objectType)
		{
			JsonObjectContract jsonObjectContract = new JsonObjectContract(objectType);
			InitializeContract(jsonObjectContract);
			jsonObjectContract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType);
			jsonObjectContract.Properties.AddRange(CreateProperties(jsonObjectContract));
			if (jsonObjectContract.DefaultCreator == null || jsonObjectContract.DefaultCreatorNonPublic)
			{
				jsonObjectContract.ParametrizedConstructor = GetParametrizedConstructor(objectType);
			}
			return jsonObjectContract;
		}

		private ConstructorInfo GetParametrizedConstructor(Type objectType)
		{
			ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if (constructors.Length == 1)
			{
				return constructors[0];
			}
			return null;
		}

		/// <summary>
		/// Resolves the default <see cref="T:Newtonsoft.Json.JsonConverter" /> for the contract.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns></returns>
		protected virtual JsonConverter ResolveContractConverter(Type objectType)
		{
			return JsonTypeReflector.GetJsonConverter(objectType, objectType);
		}

		private Func<object> GetDefaultCreator(Type createdType)
		{
			return JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
		}

		private void InitializeContract(JsonContract contract)
		{
			JsonContainerAttribute jsonContainerAttribute = JsonTypeReflector.GetJsonContainerAttribute(contract.UnderlyingType);
			if (jsonContainerAttribute != null)
			{
				contract.IsReference = jsonContainerAttribute._isReference;
			}
			else
			{
				DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.UnderlyingType);
				if (dataContractAttribute != null && dataContractAttribute.IsReference)
				{
					contract.IsReference = true;
				}
			}
			contract.Converter = ResolveContractConverter(contract.UnderlyingType);
			contract.InternalConverter = JsonSerializer.GetMatchingConverter(BuiltInConverters, contract.UnderlyingType);
			if (ReflectionUtils.HasDefaultConstructor(contract.CreatedType, nonPublic: true) || contract.CreatedType.IsValueType)
			{
				contract.DefaultCreator = GetDefaultCreator(contract.CreatedType);
				contract.DefaultCreatorNonPublic = (!contract.CreatedType.IsValueType && ReflectionUtils.GetDefaultConstructor(contract.CreatedType) == null);
			}
			MethodInfo[] methods = contract.UnderlyingType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (!methodInfo.ContainsGenericParameters)
				{
					Type prevAttributeType = null;
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (IsValidCallback(methodInfo, parameters, typeof(OnSerializingAttribute), contract.OnSerializing, ref prevAttributeType))
					{
						contract.OnSerializing = methodInfo;
					}
					if (IsValidCallback(methodInfo, parameters, typeof(OnSerializedAttribute), contract.OnSerialized, ref prevAttributeType))
					{
						contract.OnSerialized = methodInfo;
					}
					if (IsValidCallback(methodInfo, parameters, typeof(OnDeserializingAttribute), contract.OnDeserializing, ref prevAttributeType))
					{
						contract.OnDeserializing = methodInfo;
					}
					if (IsValidCallback(methodInfo, parameters, typeof(OnDeserializedAttribute), contract.OnDeserialized, ref prevAttributeType))
					{
						contract.OnDeserialized = methodInfo;
					}
					if (IsValidCallback(methodInfo, parameters, typeof(OnErrorAttribute), contract.OnError, ref prevAttributeType))
					{
						contract.OnError = methodInfo;
					}
				}
			}
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonDictionaryContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonDictionaryContract" /> for the given type.</returns>
		protected virtual JsonDictionaryContract CreateDictionaryContract(Type objectType)
		{
			JsonDictionaryContract jsonDictionaryContract = new JsonDictionaryContract(objectType);
			InitializeContract(jsonDictionaryContract);
			return jsonDictionaryContract;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonArrayContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonArrayContract" /> for the given type.</returns>
		protected virtual JsonArrayContract CreateArrayContract(Type objectType)
		{
			JsonArrayContract jsonArrayContract = new JsonArrayContract(objectType);
			InitializeContract(jsonArrayContract);
			return jsonArrayContract;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonPrimitiveContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonPrimitiveContract" /> for the given type.</returns>
		protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type objectType)
		{
			JsonPrimitiveContract jsonPrimitiveContract = new JsonPrimitiveContract(objectType);
			InitializeContract(jsonPrimitiveContract);
			return jsonPrimitiveContract;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonLinqContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonLinqContract" /> for the given type.</returns>
		protected virtual JsonLinqContract CreateLinqContract(Type objectType)
		{
			JsonLinqContract jsonLinqContract = new JsonLinqContract(objectType);
			InitializeContract(jsonLinqContract);
			return jsonLinqContract;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonISerializableContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonISerializableContract" /> for the given type.</returns>
		protected virtual JsonISerializableContract CreateISerializableContract(Type objectType)
		{
			JsonISerializableContract jsonISerializableContract = new JsonISerializableContract(objectType);
			InitializeContract(jsonISerializableContract);
			ConstructorInfo constructor = objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(SerializationInfo),
				typeof(StreamingContext)
			}, null);
			if (constructor != null)
			{
				MethodCall<object, object> methodCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructor);
				jsonISerializableContract.ISerializableCreator = ((object[] args) => methodCall(null, args));
			}
			return jsonISerializableContract;
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonStringContract" /> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonStringContract" /> for the given type.</returns>
		protected virtual JsonStringContract CreateStringContract(Type objectType)
		{
			JsonStringContract jsonStringContract = new JsonStringContract(objectType);
			InitializeContract(jsonStringContract);
			return jsonStringContract;
		}

		/// <summary>
		/// Determines which contract type is created for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="T:Newtonsoft.Json.Serialization.JsonContract" /> for the given type.</returns>
		protected virtual JsonContract CreateContract(Type objectType)
		{
			if (JsonConvert.IsJsonPrimitiveType(objectType))
			{
				return CreatePrimitiveContract(objectType);
			}
			if (JsonTypeReflector.GetJsonObjectAttribute(objectType) != null)
			{
				return CreateObjectContract(objectType);
			}
			if (JsonTypeReflector.GetJsonArrayAttribute(objectType) != null)
			{
				return CreateArrayContract(objectType);
			}
			if (objectType.IsSubclassOf(typeof(JToken)))
			{
				return CreateLinqContract(objectType);
			}
			if (CollectionUtils.IsDictionaryType(objectType))
			{
				return CreateDictionaryContract(objectType);
			}
			if (typeof(IEnumerable).IsAssignableFrom(objectType))
			{
				return CreateArrayContract(objectType);
			}
			if (CanConvertToString(objectType))
			{
				return CreateStringContract(objectType);
			}
			if (typeof(ISerializable).IsAssignableFrom(objectType))
			{
				return CreateISerializableContract(objectType);
			}
			return CreateObjectContract(objectType);
		}

		internal static bool CanConvertToString(Type type)
		{
			TypeConverter converter = ConvertUtils.GetConverter(type);
			if (converter != null && !(converter is ComponentConverter) && !(converter is ReferenceConverter) && converter.GetType() != typeof(TypeConverter) && converter.CanConvertTo(typeof(string)))
			{
				return true;
			}
			if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
			{
				return true;
			}
			return false;
		}

		private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo currentCallback, ref Type prevAttributeType)
		{
			if (!method.IsDefined(attributeType, inherit: false))
			{
				return false;
			}
			if (currentCallback != null)
			{
				throw new Exception("Invalid attribute. Both '{0}' and '{1}' in type '{2}' have '{3}'.".FormatWith(CultureInfo.InvariantCulture, method, currentCallback, GetClrTypeFullName(method.DeclaringType), attributeType));
			}
			if (prevAttributeType != null)
			{
				throw new Exception("Invalid Callback. Method '{3}' in type '{2}' has both '{0}' and '{1}'.".FormatWith(CultureInfo.InvariantCulture, prevAttributeType, attributeType, GetClrTypeFullName(method.DeclaringType), method));
			}
			if (method.IsVirtual)
			{
				throw new Exception("Virtual Method '{0}' of type '{1}' cannot be marked with '{2}' attribute.".FormatWith(CultureInfo.InvariantCulture, method, GetClrTypeFullName(method.DeclaringType), attributeType));
			}
			if (method.ReturnType != typeof(void))
			{
				throw new Exception("Serialization Callback '{1}' in type '{0}' must return void.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method));
			}
			if (attributeType == typeof(OnErrorAttribute))
			{
				if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
				{
					throw new Exception("Serialization Error Callback '{1}' in type '{0}' must have two parameters of type '{2}' and '{3}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext), typeof(ErrorContext)));
				}
			}
			else if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
			{
				throw new Exception("Serialization Callback '{1}' in type '{0}' must have a single parameter of type '{2}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext)));
			}
			prevAttributeType = attributeType;
			return true;
		}

		internal static string GetClrTypeFullName(Type type)
		{
			if (type.IsGenericTypeDefinition || !type.ContainsGenericParameters)
			{
				return type.FullName;
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", type.Namespace, type.Name);
		}

		/// <summary>
		/// Creates properties for the given <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" />.
		/// </summary>
		/// <param name="contract">The contract to create properties for.</param>
		/// <returns>Properties for the given <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" />.</returns>
		protected virtual IList<JsonProperty> CreateProperties(JsonObjectContract contract)
		{
			List<MemberInfo> serializableMembers = GetSerializableMembers(contract.UnderlyingType);
			if (serializableMembers == null)
			{
				throw new JsonSerializationException("Null collection of seralizable members returned.");
			}
			JsonPropertyCollection jsonPropertyCollection = new JsonPropertyCollection(contract);
			foreach (MemberInfo item in serializableMembers)
			{
				JsonProperty jsonProperty = CreateProperty(contract, item);
				if (jsonProperty != null)
				{
					jsonPropertyCollection.AddProperty(jsonProperty);
				}
			}
			return jsonPropertyCollection;
		}

		/// <summary>
		/// Creates the <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> used by the serializer to get and set values from a member.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <returns>The <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> used by the serializer to get and set values from a member.</returns>
		protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
		{
			if (DynamicCodeGeneration)
			{
				return new DynamicValueProvider(member);
			}
			return new ReflectionValueProvider(member);
		}

		/// <summary>
		/// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.
		/// </summary>
		/// <param name="contract">The member's declaring types <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" />.</param>
		/// <param name="member">The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for.</param>
		/// <returns>A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.</returns>
		protected virtual JsonProperty CreateProperty(JsonObjectContract contract, MemberInfo member)
		{
			JsonProperty jsonProperty = new JsonProperty();
			jsonProperty.PropertyType = ReflectionUtils.GetMemberUnderlyingType(member);
			jsonProperty.ValueProvider = CreateMemberValueProvider(member);
			jsonProperty.Converter = JsonTypeReflector.GetJsonConverter(member, jsonProperty.PropertyType);
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(member.DeclaringType);
			DataMemberAttribute dataMemberAttribute = (dataContractAttribute == null) ? null : JsonTypeReflector.GetAttribute<DataMemberAttribute>(member);
			JsonPropertyAttribute attribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member);
			bool flag = JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(member) != null;
			string propertyName = (attribute != null && attribute.PropertyName != null) ? attribute.PropertyName : ((dataMemberAttribute == null || dataMemberAttribute.Name == null) ? member.Name : dataMemberAttribute.Name);
			jsonProperty.PropertyName = ResolvePropertyName(propertyName);
			if (attribute != null)
			{
				jsonProperty.Required = attribute.Required;
			}
			else if (dataMemberAttribute != null)
			{
				jsonProperty.Required = (dataMemberAttribute.IsRequired ? Required.AllowNull : Required.Default);
			}
			else
			{
				jsonProperty.Required = Required.Default;
			}
			jsonProperty.Ignored = (flag || (contract.MemberSerialization == MemberSerialization.OptIn && attribute == null && dataMemberAttribute == null));
			bool nonPublic = false;
			if ((DefaultMembersSearchFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
			{
				nonPublic = true;
			}
			if (attribute != null)
			{
				nonPublic = true;
			}
			if (dataMemberAttribute != null)
			{
				nonPublic = true;
			}
			jsonProperty.Readable = ReflectionUtils.CanReadMemberValue(member, nonPublic);
			jsonProperty.Writable = ReflectionUtils.CanSetMemberValue(member, nonPublic);
			jsonProperty.MemberConverter = JsonTypeReflector.GetJsonConverter(member, ReflectionUtils.GetMemberUnderlyingType(member));
			jsonProperty.DefaultValue = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(member)?.Value;
			jsonProperty.NullValueHandling = attribute?._nullValueHandling;
			jsonProperty.DefaultValueHandling = attribute?._defaultValueHandling;
			jsonProperty.ReferenceLoopHandling = attribute?._referenceLoopHandling;
			jsonProperty.ObjectCreationHandling = attribute?._objectCreationHandling;
			jsonProperty.TypeNameHandling = attribute?._typeNameHandling;
			jsonProperty.IsReference = attribute?._isReference;
			jsonProperty.ShouldSerialize = CreateShouldSerializeTest(member);
			return jsonProperty;
		}

		private Predicate<object> CreateShouldSerializeTest(MemberInfo member)
		{
			MethodInfo method = member.DeclaringType.GetMethod("ShouldSerialize" + member.Name, new Type[0]);
			if (method == null || method.ReturnType != typeof(bool))
			{
				return null;
			}
			MethodCall<object, object> shouldSerializeCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
			return (object o) => (bool)shouldSerializeCall(o);
		}

		/// <summary>
		/// Resolves the name of the property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Name of the property.</returns>
		protected virtual string ResolvePropertyName(string propertyName)
		{
			return propertyName;
		}
	}
}
