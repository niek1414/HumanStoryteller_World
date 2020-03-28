using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities
{
	
	
	internal abstract class ReflectionDelegateFactory
	{
		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public Func<T, object> CreateGet< T>(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if ((object)propertyInfo != null)
			{
				if (propertyInfo.PropertyType.IsByRef)
				{
					throw new InvalidOperationException("Could not create getter for {0}. ByRef return values are not supported.".FormatWith(CultureInfo.InvariantCulture, propertyInfo));
				}
				return CreateGet<T>(propertyInfo);
			}
			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if ((object)fieldInfo != null)
			{
				return CreateGet<T>(fieldInfo);
			}
			throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
		}

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public Action<T, object> CreateSet< T>(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if ((object)propertyInfo != null)
			{
				return CreateSet<T>(propertyInfo);
			}
			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if ((object)fieldInfo != null)
			{
				return CreateSet<T>(fieldInfo);
			}
			throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
		}

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public abstract MethodCall<T, object> CreateMethodCall< T>(MethodBase method);

		public abstract ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method);

		public abstract Func<T> CreateDefaultConstructor< T>(Type type);

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public abstract Func<T, object> CreateGet< T>(PropertyInfo propertyInfo);

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public abstract Func<T, object> CreateGet< T>(FieldInfo fieldInfo);

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public abstract Action<T, object> CreateSet< T>(FieldInfo fieldInfo);

		[return: Nullable(new byte[]
		{
			1,
			1,
			2
		})]
		public abstract Action<T, object> CreateSet< T>(PropertyInfo propertyInfo);
	}
}
