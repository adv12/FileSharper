// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Text
{

    public class ContainsTextParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string Text { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool MatchOnlyWithinSingleLine { get; set; } = true;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool UseRegex { get; set; }
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool RegexStartAndEndMatchPerLine { get; set; }
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool RegexDotMatchesNewline { get; set; }
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; }
    }

    public class ContainsTextCondition : ConditionBase
    {
        private ContainsTextParameters m_Parameters = new ContainsTextParameters();
        private Regex m_Regex;

        public override string Name => "Contains Text";

        public override string Category => "Text";

        public override string Description => "Whether the file contains the specififed text";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders
        {
            get
            {
                if (m_Parameters.UseRegex)
                {
                    return new string[] { "Matches Regex \"" + m_Parameters.Text + "\"" };
                }
                return new string[] { "Contains \"" + m_Parameters.Text + "\"" };
            }
        }

        public override void LocalInit()
        {
            base.LocalInit();
            RegexOptions regexOptions = RegexOptions.None;
            if (!m_Parameters.CaseSensitive)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            if (m_Parameters.RegexStartAndEndMatchPerLine)
            {
                regexOptions |= RegexOptions.Multiline;
            }
            if (m_Parameters.RegexDotMatchesNewline)
            {
                regexOptions |= RegexOptions.Singleline;
            }
            if (m_Parameters.UseRegex)
            {
                m_Regex = new Regex(m_Parameters.Text, regexOptions);
            }
            else
            {
                m_Regex = new Regex(Regex.Escape(m_Parameters.Text), regexOptions);
            }
        }

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            Encoding detectedEncoding = TextUtil.DetectEncoding(file);
            if (!m_Parameters.MatchOnlyWithinSingleLine)
            {
                string text = File.ReadAllText(file.FullName, detectedEncoding);
                if (m_Regex.IsMatch(text))
                {
                    return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                }
            }
            else
            {
                using (StreamReader reader = TextUtil.CreateStreamReaderWithAppropriateEncoding(
                    file, detectedEncoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (m_Regex.IsMatch(line))
                        {
                            return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                        }
                    }
                }
            }
            return new MatchResult(MatchResultType.No, new string[] { "No" });
        }
    }
}
