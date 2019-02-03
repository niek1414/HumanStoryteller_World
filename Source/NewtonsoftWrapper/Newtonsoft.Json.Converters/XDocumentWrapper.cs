using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XDocumentWrapper : XContainerWrapper, IXmlDocument, IXmlNode
	{
		private XDocument Document => (XDocument)base.WrappedNode;

		public override IList<IXmlNode> ChildNodes
		{
			get
			{
				IList<IXmlNode> childNodes = base.ChildNodes;
				if (Document.Declaration != null)
				{
					childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));
				}
				return childNodes;
			}
		}

		public IXmlElement DocumentElement
		{
			get
			{
				if (Document.Root == null)
				{
					return null;
				}
				return new XElementWrapper(Document.Root);
			}
		}

		public XDocumentWrapper(XDocument document)
			: base(document)
		{
		}

		public IXmlNode CreateComment(string text)
		{
			return new XObjectWrapper(new XComment(text));
		}

		public IXmlNode CreateTextNode(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateCDataSection(string data)
		{
			return new XObjectWrapper(new XCData(data));
		}

		public IXmlNode CreateWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateSignificantWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
		}

		public IXmlNode CreateProcessingInstruction(string target, string data)
		{
			return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
		}

		public IXmlElement CreateElement(string elementName)
		{
			return new XElementWrapper(new XElement(elementName));
		}

		public IXmlElement CreateElement(string qualifiedName, string namespaceURI)
		{
			string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
			return new XElementWrapper(new XElement(XName.Get(localName, namespaceURI)));
		}

		public IXmlNode CreateAttribute(string name, string value)
		{
			return new XAttributeWrapper(new XAttribute(name, value));
		}

		public IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value)
		{
			string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
			return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceURI), value));
		}

		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			XDeclarationWrapper xDeclarationWrapper = newChild as XDeclarationWrapper;
			if (xDeclarationWrapper != null)
			{
				Document.Declaration = xDeclarationWrapper._declaration;
				return xDeclarationWrapper;
			}
			return base.AppendChild(newChild);
		}
	}
}
