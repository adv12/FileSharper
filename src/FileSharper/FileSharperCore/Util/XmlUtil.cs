using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FileSharperCore.Util
{
    public class XmlUtil
    {
        public static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc, XPathNavigator navigator,
            bool ignoreDefaultNamespace, string defaultNamespacePrefixIfNotIgnored)
        {
            XmlNamespaceManager namespaceManager = null;
            if (navigator.NameTable != null)
            {
                namespaceManager = new XmlNamespaceManager(navigator.NameTable);
                if (!ignoreDefaultNamespace)
                {
                    namespaceManager.AddNamespace(defaultNamespacePrefixIfNotIgnored,
                        xmlDoc.DocumentElement.NamespaceURI);
                }
                IDictionary<string, string> namespaces =
                    xmlDoc.DocumentElement.CreateNavigator().GetNamespacesInScope(XmlNamespaceScope.All);
                foreach (KeyValuePair<string, string> pair in namespaces)
                {
                    if (pair.Key != null && pair.Key != string.Empty)
                    {
                        namespaceManager.AddNamespace(pair.Key, pair.Value);
                    }
                }
            }
            return namespaceManager;
        }
    }
}
