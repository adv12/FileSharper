// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                    return new MatchResult(MatchResultType.Yes, null);
                }
            }
            else
            {
                if (m_Parameters.CaseSensitive)
                {
                    if (path.Contains(m_Parameters.Text))
                    {
                        return new MatchResult(MatchResultType.Yes, null);
                    }
                }
                else
                {
                    if (path.ToLower().Contains(m_LowerCaseText))
                    {
                        return new MatchResult(MatchResultType.Yes, null);
                    }
                }
            }
            return new MatchResult(MatchResultType.No, null);
        }
    }
}
