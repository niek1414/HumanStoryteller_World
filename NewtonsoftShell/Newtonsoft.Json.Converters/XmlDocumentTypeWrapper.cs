using System.Xml;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;

	
	
	internal class XmlDocumentTypeWrapper : XmlNodeWrapper, IXmlDocumentType, IXmlNode
	{
		private readonly XmlDocumentType _documentType;

		public string Name => _documentType.Name;

		public string System => _documentType.SystemId;

		public string Public => _documentType.PublicId;

		public string InternalSubset => _documentType.InternalSubset;

		
		public override string LocalName
		{
			
			get
			{
				return "DOCTYPE";
			}
		}

		public XmlDocumentTypeWrapper(XmlDocumentType documentType)
			: base(documentType)
		{
			_documentType = documentType;
		}
	}
