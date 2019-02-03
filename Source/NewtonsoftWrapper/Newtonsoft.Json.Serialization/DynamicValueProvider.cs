using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Get and set values for a <see cref="T:System.Reflection.MemberInfo" /> using dynamic methods.
	/// </summary>
	public class DynamicValueProvider : IValueProvider
	{
		private readonly MemberInfo _memberInfo;

		private Func<object, object> _getter;

		private Action<object, object> _setter;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.DynamicValueProvider" /> class.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		public DynamicValueProvider(MemberInfo memberInfo)
		{
			ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
			_memberInfo = memberInfo;
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="target">The target to set the value on.</param>
		/// <param name="value">The value to set on the target.</param>
		public void SetValue(object target, object value)
		{
			try
			{
				if (_setter == null)
				{
					_setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(_memberInfo);
				}
				_setter(target, value);
			}
			catch (Exception innerException)
			{
				throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
			}
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <param name="target">The target to get the value from.</param>
		/// <returns>The value.</returns>
		public object GetValue(object target)
		{
			try
			{
				if (_getter == null)
				{
					_getter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(_memberInfo);
				}
				return _getter(target);
			}
			catch (Exception innerException)
			{
				throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
			}
		}
	}
}
