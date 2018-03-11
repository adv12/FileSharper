// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Xml
{
    public class XPathStringValueParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public bool IgnoreDefaultNamespace { get; set; } = true;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string DefaultNamespacePrefixIfNotIgnored { get; set; } = "x";
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string XPath { get; set; } = "*";
        [PropertyOrder(4, UsageContextEnum.Both)]
        public string Text { get; set; } = string.Empty;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool UseRegex { get; set; } = false;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; } = false;
    }

    public class XPathStringValueCondition : ConditionBase
    {
        private XPathStringValueParameters m_Parameters = new XPathStringValueParameters();
        private XPathExpression m_Expression;
        private Regex m_Regex;
        private string m_LowerCaseText;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "XPath String Value Contains Text" };

        public override string Category => "XML";

        public override string Name => "XPath String Value Contains Text";

        public override string Description => null;

        public override object Parameters => m_Parameters;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            base.LocalInit(exceptionProgress);
            m_Expression = XPathExpression.Compile("string(" + m_Parameters.XPath + ")");
            if (m_Parameters.Text == null)
            {
                m_Parameters.Text = string.Empty;
            }
            if (m_Parameters.UseRegex)
            {
                RegexOptions opts = RegexOptions.None;
                if (!m_Parameters.CaseSensitive)
                {
                    opts |= RegexOptions.IgnoreCase;
                }
                m_Regex = new Regex(m_Parameters.Text, opts);
            }
            else
            {
                m_LowerCaseText = m_Parameters.Text.ToLower();
            }
        }

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                if (m_Parameters.IgnoreDefaultNamespace)
                {
                    string docstr = File.ReadAllText(file.FullName);
                    docstr = Regex.Replace(docstr, @"xmlns\s*=\s*", "foobar=");
                    xmlDoc.LoadXml(docstr);
                }
                else
                {
                    xmlDoc.Load(file.FullName);
                }
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
                string value = "" + navigator.Evaluate(m_Expression);

                if (m_Parameters.UseRegex)
                {
                    if (m_Regex.IsMatch(value))
                    {
                        return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                    }
                }
                else
                {
                    if (m_Parameters.CaseSensitive)
                    {
                        if (value.Contains(m_Parameters.Text))
                        {
                            return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                        }
                    }
                    else
                    {
                        if (value.ToLower().Contains(m_LowerCaseText))
                        {
                            return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                        }
                    }
                }
            }
            catch (XPathException)
            {

            }
            return new MatchResult(MatchResultType.No, "No");
        }
    }
}
