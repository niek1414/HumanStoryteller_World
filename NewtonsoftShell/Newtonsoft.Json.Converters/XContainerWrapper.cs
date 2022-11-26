using System.Collections.Generic;
using System.Xml.Linq;

namespace HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;

	
	
	internal class XContainerWrapper : XObjectWrapper
	{
		
		private List<IXmlNode> _childNodes;

		private XContainer Container => (XContainer)base.WrappedNode;

		public override List<IXmlNode> ChildNodes
		{
			get
			{
				if (_childNodes == null)
				{
					if (!HasChildNodes)
					{
						_childNodes = XmlNodeConverter.EmptyChildNodes;
					}
					else
					{
						_childNodes = new List<IXmlNode>();
						foreach (XNode item in Container.Nodes())
						{
							_childNodes.Add(WrapNode(item));
						}
					}
				}
				return _childNodes;
			}
		}

		protected virtual bool HasChildNodes => Container.LastNode != null;

		
		public override IXmlNode ParentNode
		{
			
			get
			{
				if (Container.Parent == null)
				{
					return null;
				}
				return WrapNode(Container.Parent);
			}
		}

		public XContainerWrapper(XContainer container)
			: base(container)
		{
		}

		internal static IXmlNode WrapNode(XObject node)
		{
			XDocument xDocument = node as XDocument;
			if (xDocument != null)
			{
				return new XDocumentWrapper(xDocument);
			}
			XElement xElement = node as XElement;
			if (xElement != null)
			{
				return new XElementWrapper(xElement);
			}
			XContainer xContainer = node as XContainer;
			if (xContainer != null)
			{
				return new XContainerWrapper(xContainer);
			}
			XProcessingInstruction xProcessingInstruction = node as XProcessingInstruction;
			if (xProcessingInstruction != null)
			{
				return new XProcessingInstructionWrapper(xProcessingInstruction);
			}
			XText xText = node as XText;
			if (xText != null)
			{
				return new XTextWrapper(xText);
			}
			XComment xComment = node as XComment;
			if (xComment != null)
			{
				return new XCommentWrapper(xComment);
			}
			XAttribute xAttribute = node as XAttribute;
			if (xAttribute != null)
			{
				return new XAttributeWrapper(xAttribute);
			}
			XDocumentType xDocumentType = node as XDocumentType;
			if (xDocumentType != null)
			{
				return new XDocumentTypeWrapper(xDocumentType);
			}
			return new XObjectWrapper(node);
		}

		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			Container.Add(newChild.WrappedNode);
			_childNodes = null;
			return newChild;
		}
	}
