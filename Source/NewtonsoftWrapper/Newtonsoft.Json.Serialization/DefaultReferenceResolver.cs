using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	internal class DefaultReferenceResolver : IReferenceResolver
	{
		private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return object.ReferenceEquals(x, y);
			}

			int IEqualityComparer<object>.GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		private int _referenceCount;

		private BidirectionalDictionary<string, object> _mappings;

		private BidirectionalDictionary<string, object> Mappings
		{
			get
			{
				if (_mappings == null)
				{
					_mappings = new BidirectionalDictionary<string, object>(EqualityComparer<string>.Default, new ReferenceEqualsEqualityComparer());
				}
				return _mappings;
			}
		}

		public object ResolveReference(string reference)
		{
			Mappings.TryGetByFirst(reference, out object second);
			return second;
		}

		public string GetReference(object value)
		{
			if (!Mappings.TryGetBySecond(value, out string first))
			{
				_referenceCount++;
				first = _referenceCount.ToString(CultureInfo.InvariantCulture);
				Mappings.Add(first, value);
			}
			return first;
		}

		public void AddReference(string reference, object value)
		{
			Mappings.Add(reference, value);
		}

		public bool IsReferenced(object value)
		{
			string first;
			return Mappings.TryGetBySecond(value, out first);
		}
	}
}
