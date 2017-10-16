// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileSharperCore.Conditions
{
    public class FilePathContainsTextCondition : ConditionBase
    {
        private ContainsTextParameters m_Parameters = new ContainsTextParameters();
        private Regex m_Regex;
        private string m_LowerCaseText;

        public override string Name => "File Path Contains Text";

        public override string Description => "Whether the file path contains the specified text";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders
        {
            get
            {
                if (m_Parameters.UseRegex)
                {
                    return new string[] { "File Path Matches Regex \"" + m_Parameters.Text + "\"" };
                }
                return new string[] { "File Path Contains \"" + m_Parameters.Text + "\"" };
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
            string path = file.FullName;
            if (m_Parameters.UseRegex)
            {
                if (m_Regex.IsMatch(path))
                {
                    return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                }
            }
            else
            {
                if (m_Parameters.CaseSensitive)
                {
                    if (path.Contains(m_Parameters.Text))
                    {
                        return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                    }
                }
                else
                {
                    if (path.ToLower().Contains(m_LowerCaseText))
                    {
                        return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
                    }
                }
            }
            return new MatchResult(MatchResultType.No, new string[] { "No" });
        }
    }
}
