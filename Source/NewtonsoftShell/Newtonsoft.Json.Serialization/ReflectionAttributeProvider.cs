using System;
using System.Collections.Generic;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Utilities;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Serialization
{
	
	
	public class ReflectionAttributeProvider : IAttributeProvider
	{
		private readonly object _attributeProvider;

		public ReflectionAttributeProvider(object attributeProvider)
		{
			ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");
			_attributeProvider = attributeProvider;
		}

		public IList<Attribute> GetAttributes(bool inherit)
		{
			return ReflectionUtils.GetAttributes(_attributeProvider, null, inherit);
		}

		public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
		{
			return ReflectionUtils.GetAttributes(_attributeProvider, attributeType, inherit);
		}
	}
}
