using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	internal static class ConvertUtils
	{
		internal struct TypeConvertKey : IEquatable<TypeConvertKey>
		{
			private readonly Type _initialType;

			private readonly Type _targetType;

			public Type InitialType => _initialType;

			public Type TargetType => _targetType;

			public TypeConvertKey(Type initialType, Type targetType)
			{
				_initialType = initialType;
				_targetType = targetType;
			}

			public override int GetHashCode()
			{
				return _initialType.GetHashCode() ^ _targetType.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (!(obj is TypeConvertKey))
				{
					return false;
				}
				return Equals((TypeConvertKey)obj);
			}

			public bool Equals(TypeConvertKey other)
			{
				if (_initialType == other._initialType)
				{
					return _targetType == other._targetType;
				}
				return false;
			}
		}

		private static readonly ThreadSafeStore<TypeConvertKey, Func<object, object>> CastConverters = new ThreadSafeStore<TypeConvertKey, Func<object, object>>(CreateCastConverter);

		private static Func<object, object> CreateCastConverter(TypeConvertKey t)
		{
			MethodInfo method = t.TargetType.GetMethod("op_Implicit", new Type[1]
			{
				t.InitialType
			});
			if (method == null)
			{
				method = t.TargetType.GetMethod("op_Explicit", new Type[1]
				{
					t.InitialType
				});
			}
			if (method == null)
			{
				return null;
			}
			MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
			return (object o) => call(null, o);
		}

		public static bool CanConvertType(Type initialType, Type targetType, bool allowTypeNameToString)
		{
			ValidationUtils.ArgumentNotNull(initialType, "initialType");
			ValidationUtils.ArgumentNotNull(targetType, "targetType");
			if (ReflectionUtils.IsNullableType(targetType))
			{
				targetType = Nullable.GetUnderlyingType(targetType);
			}
			if (targetType == initialType)
			{
				return true;
			}
			if (typeof(IConvertible).IsAssignableFrom(initialType) && typeof(IConvertible).IsAssignableFrom(targetType))
			{
				return true;
			}
			if (initialType == typeof(DateTime) && targetType == typeof(DateTimeOffset))
			{
				return true;
			}
			if (initialType == typeof(Guid) && (targetType == typeof(Guid) || targetType == typeof(string)))
			{
				return true;
			}
			if (initialType == typeof(Type) && targetType == typeof(string))
			{
				return true;
			}
			TypeConverter converter = GetConverter(initialType);
			if (converter != null && !IsComponentConverter(converter) && converter.CanConvertTo(targetType) && (allowTypeNameToString || converter.GetType() != typeof(TypeConverter)))
			{
				return true;
			}
			TypeConverter converter2 = GetConverter(targetType);
			if (converter2 != null && !IsComponentConverter(converter2) && converter2.CanConvertFrom(initialType))
			{
				return true;
			}
			if (initialType == typeof(DBNull) && ReflectionUtils.IsNullable(targetType))
			{
				return true;
			}
			return false;
		}

		private static bool IsComponentConverter(TypeConverter converter)
		{
			return converter is ComponentConverter;
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <returns>The converted type.</returns>
		public static T Convert<T>(object initialValue)
		{
			return Convert<T>(initialValue, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <returns>The converted type.</returns>
		public static T Convert<T>(object initialValue, CultureInfo culture)
		{
			return (T)Convert(initialValue, culture, typeof(T));
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="targetType">The type to convert the value to.</param>
		/// <returns>The converted type.</returns>
		public static object Convert(object initialValue, CultureInfo culture, Type targetType)
		{
			if (initialValue == null)
			{
				throw new ArgumentNullException("initialValue");
			}
			if (ReflectionUtils.IsNullableType(targetType))
			{
				targetType = Nullable.GetUnderlyingType(targetType);
			}
			Type type = initialValue.GetType();
			if (targetType == type)
			{
				return initialValue;
			}
			if (initialValue is string && typeof(Type).IsAssignableFrom(targetType))
			{
				return Type.GetType((string)initialValue, throwOnError: true);
			}
			if (targetType.IsInterface || targetType.IsGenericTypeDefinition || targetType.IsAbstract)
			{
				throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), "targetType");
			}
			if (initialValue is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
			{
				if (targetType.IsEnum)
				{
					if (initialValue is string)
					{
						return Enum.Parse(targetType, initialValue.ToString(), ignoreCase: true);
					}
					if (IsInteger(initialValue))
					{
						return Enum.ToObject(targetType, initialValue);
					}
				}
				return System.Convert.ChangeType(initialValue, targetType, culture);
			}
			if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
			{
				return new DateTimeOffset((DateTime)initialValue);
			}
			if (initialValue is string)
			{
				if (targetType == typeof(Guid))
				{
					return new Guid((string)initialValue);
				}
				if (targetType == typeof(Uri))
				{
					return new Uri((string)initialValue);
				}
				if (targetType == typeof(TimeSpan))
				{
					return TimeSpan.Parse((string)initialValue);
				}
			}
			TypeConverter converter = GetConverter(type);
			if (converter != null && converter.CanConvertTo(targetType))
			{
				return converter.ConvertTo(null, culture, initialValue, targetType);
			}
			TypeConverter converter2 = GetConverter(targetType);
			if (converter2 != null && converter2.CanConvertFrom(type))
			{
				return converter2.ConvertFrom(null, culture, initialValue);
			}
			if (initialValue == DBNull.Value)
			{
				if (ReflectionUtils.IsNullable(targetType))
				{
					return EnsureTypeAssignable(null, type, targetType);
				}
				throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, type, targetType));
			}
			if (initialValue is INullable)
			{
				return EnsureTypeAssignable(ToValue((INullable)initialValue), type, targetType);
			}
			throw new Exception("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, type, targetType));
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvert<T>(object initialValue, out T convertedValue)
		{
			return TryConvert(initialValue, CultureInfo.CurrentCulture, out convertedValue);
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvert<T>(object initialValue, CultureInfo culture, out T convertedValue)
		{
			return MiscellaneousUtils.TryAction(delegate
			{
				TryConvert(initialValue, CultureInfo.CurrentCulture, typeof(T), out object convertedValue2);
				return (T)convertedValue2;
			}, out convertedValue);
		}

		/// <summary>
		/// Converts the value to the specified type.
		/// </summary>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="targetType">The type to convert the value to.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
		{
			return MiscellaneousUtils.TryAction(() => Convert(initialValue, culture, targetType), out convertedValue);
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert or cast the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <returns>The converted type. If conversion was unsuccessful, the initial value is returned if assignable to the target type</returns>
		public static T ConvertOrCast<T>(object initialValue)
		{
			return ConvertOrCast<T>(initialValue, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert or cast the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <returns>The converted type. If conversion was unsuccessful, the initial value is returned if assignable to the target type</returns>
		public static T ConvertOrCast<T>(object initialValue, CultureInfo culture)
		{
			return (T)ConvertOrCast(initialValue, culture, typeof(T));
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="targetType">The type to convert or cast the value to.</param>
		/// <returns>
		/// The converted type. If conversion was unsuccessful, the initial value
		/// is returned if assignable to the target type.
		/// </returns>
		public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
		{
			if (targetType == typeof(object))
			{
				return initialValue;
			}
			if (initialValue == null && ReflectionUtils.IsNullable(targetType))
			{
				return null;
			}
			if (TryConvert(initialValue, culture, targetType, out object convertedValue))
			{
				return convertedValue;
			}
			return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvertOrCast<T>(object initialValue, out T convertedValue)
		{
			return TryConvertOrCast(initialValue, CultureInfo.CurrentCulture, out convertedValue);
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvertOrCast<T>(object initialValue, CultureInfo culture, out T convertedValue)
		{
			return MiscellaneousUtils.TryAction(delegate
			{
				TryConvertOrCast(initialValue, CultureInfo.CurrentCulture, typeof(T), out object convertedValue2);
				return (T)convertedValue2;
			}, out convertedValue);
		}

		/// <summary>
		/// Converts the value to the specified type. If the value is unable to be converted, the
		/// value is checked whether it assignable to the specified type.
		/// </summary>
		/// <param name="initialValue">The value to convert.</param>
		/// <param name="culture">The culture to use when converting.</param>
		/// <param name="targetType">The type to convert the value to.</param>
		/// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
		/// <returns>
		/// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryConvertOrCast(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
		{
			return MiscellaneousUtils.TryAction(() => ConvertOrCast(initialValue, culture, targetType), out convertedValue);
		}

		private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
		{
			Type type = value?.GetType();
			if (value != null)
			{
				if (targetType.IsAssignableFrom(type))
				{
					return value;
				}
				Func<object, object> func = CastConverters.Get(new TypeConvertKey(type, targetType));
				if (func != null)
				{
					return func(value);
				}
			}
			else if (ReflectionUtils.IsNullable(targetType))
			{
				return null;
			}
			throw new Exception("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, (initialType != null) ? initialType.ToString() : "{null}", targetType));
		}

		public static object ToValue(INullable nullableValue)
		{
			if (nullableValue == null)
			{
				return null;
			}
			if (nullableValue is SqlInt32)
			{
				return ToValue((SqlInt32)nullableValue);
			}
			if (nullableValue is SqlInt64)
			{
				return ToValue((SqlInt64)nullableValue);
			}
			if (nullableValue is SqlBoolean)
			{
				return ToValue((SqlBoolean)nullableValue);
			}
			if (nullableValue is SqlString)
			{
				return ToValue((SqlString)nullableValue);
			}
			if (nullableValue is SqlDateTime)
			{
				return ToValue((SqlDateTime)nullableValue);
			}
			throw new Exception("Unsupported INullable type: {0}".FormatWith(CultureInfo.InvariantCulture, nullableValue.GetType()));
		}

		internal static TypeConverter GetConverter(Type t)
		{
			return JsonTypeReflector.GetTypeConverter(t);
		}

		public static bool IsInteger(object value)
		{
			switch (System.Convert.GetTypeCode(value))
			{
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				return true;
			default:
				return false;
			}
		}
	}
}
