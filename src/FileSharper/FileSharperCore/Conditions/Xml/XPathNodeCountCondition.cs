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

namespace FileSharperCore.Conditions.Xml
{
    public class XPathNodeCountComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public bool IgnoreDefaultNamespace { get; set; } = true;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string DefaultNamespacePrefixIfNotIgnored { get; set; } = "x";
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string XPath { get; set; } = "*";
        [PropertyOrder(4, UsageContextEnum.Both)]
        public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterThan;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public int Count { get; set; } = 0;
    }

    public class XPathNodeCountCondition : ConditionBase
    {
        private XPathNodeCountComparisonParameters m_Parameters = new XPathNodeCountComparisonParameters();
        private XPathExpression m_Expression;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Node Count for XPath " + m_Parameters.XPath };

        public override string Category => "XML";

        public override string Name => "XPath Node Count";

        public override string Description => null;

        public override object Parameters => m_Parameters;

        public override void LocalInit()
        {
            base.LocalInit();
            m_Expression = XPathExpression.Compile(m_Parameters.XPath);
        }

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlUtil.LoadXmlDocument(xmlDoc, file, m_Parameters.IgnoreDefaultNamespace);
            }
            catch (Exception)
            {
                return new MatchResult(MatchResultType.NotApplicable, "N/A");
            }
            XPathNavigator navigator = xmlDoc.CreateNavigator();
            XmlNamespaceManager namespaceManager = XmlUtil.GetNamespaceManager(
                xmlDoc, navigator, m_Parameters.IgnoreDefaultNamespace,
                m_Parameters.DefaultNamespacePrefixIfNotIgnored);
            try
            {
                m_Expression.SetContext(namespaceManager);
                XPathNodeIterator iterator = navigator.Select(m_Expression);
                MatchResultType resultType = CompareUtil.Compare(iterator.Count,
                    m_Parameters.ComparisonType, m_Parameters.Count);
                return new MatchResult(resultType, iterator.Count.ToString());
            }
            catch (XPathException)
            {
                return new MatchResult(MatchResultType.No, "0");
            }
        }
    }
}
