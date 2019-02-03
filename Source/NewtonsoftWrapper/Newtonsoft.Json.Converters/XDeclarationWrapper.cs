using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration, IXmlNode
	{
		internal readonly XDeclaration _declaration;

		public override XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

		public string Version => _declaration.Version;

		public string Encoding
		{
			get
			{
				return _declaration.Encoding;
			}
			set
			{
				_declaration.Encoding = value;
			}
		}

		public string Standalone
		{
			get
			{
				return _declaration.Standalone;
			}
			set
			{
				_declaration.Standalone = value;
			}
		}

		public XDeclarationWrapper(XDeclaration declaration)
			: base(null)
		{
			_declaration = declaration;
		}
	}
}
