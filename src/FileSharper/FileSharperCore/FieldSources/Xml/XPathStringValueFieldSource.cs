// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FieldSources.Xml
{
    public class XPathStringValueParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public bool IgnoreDefaultNamespace { get; set; } = true;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string DefaultNamespacePrefixIfNotIgnored { get; set; } = "x";
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string XPath { get; set; } = "*";
    }

    public class XPathStringValueFieldSource : FieldSourceBase
    {
        private XPathStringValueParameters m_Parameters = new XPathStringValueParameters();
        private XPathExpression m_Expression;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "XPath Result for " + m_Parameters.XPath };

        public override string Category => "XML";

        public override string Name => "XPath Result String Value";

        public override string Description => null;

        public override object Parameters => m_Parameters;

        public override void LocalInit()
        {
            base.LocalInit();
            m_Expression = XPathExpression.Compile("string(" + m_Parameters.XPath + ")");
        }

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlUtil.LoadXmlDocument(xmlDoc, file, m_Parameters.IgnoreDefaultNamespace);
            }
            catch (Exception)
            {
                return new string[] { string.Empty };
            }
            XPathNavigator navigator = xmlDoc.CreateNavigator();
            XmlNamespaceManager namespaceManager = XmlUtil.GetNamespaceManager(
                xmlDoc, navigator, m_Parameters.IgnoreDefaultNamespace,
                m_Parameters.DefaultNamespacePrefixIfNotIgnored);
            try
            {
                m_Expression.SetContext(namespaceManager);
                string value = "" + navigator.Evaluate(m_Expression);
                return new string[] { value };
            }
            catch (XPathException)
            {

            }
            return new string[] { string.Empty };
        }
    }
}
