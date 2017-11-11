// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Text
{

    public class ContainsTextParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string Text { get; set; } = string.Empty;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool UseRegex { get; set; } = false;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; } = false;
    }

    public class ContainsTextCondition : ConditionBase
    {
        private ContainsTextParameters m_Parameters = new ContainsTextParameters();
        private Regex m_Regex;
        private string m_LowerCaseText;

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

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            base.LocalInit(exceptionProgress);
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
            using (StreamReader reader = new StreamReader(file.FullName))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }
                    if (m_Parameters.UseRegex)
                    {
                        if (m_Regex.IsMatch(line))
                        {
                            return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                        }
                    }
                    else
                    {
                        if (m_Parameters.CaseSensitive)
                        {
                            if (line.Contains(m_Parameters.Text))
                            {
                                return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                            }
                        }
                        else
                        {
                            if (line.ToLower().Contains(m_LowerCaseText))
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
}
