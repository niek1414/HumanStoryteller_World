using System.Xml.Linq;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;

	
	
	internal class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType, IXmlNode
	{
		private readonly XDocumentType _documentType;

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

		public XDocumentTypeWrapper(XDocumentType documentType)
			: base(documentType)
		{
			_documentType = documentType;
		}
	}
