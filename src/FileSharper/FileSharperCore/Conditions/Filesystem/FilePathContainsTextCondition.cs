// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Filesystem
{
    public class FilePathContainsTextParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string Text { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool UseRegex { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; }
    }

    public class FilePathContainsTextCondition : ConditionBase
    {
        private FilePathContainsTextParameters m_Parameters = new FilePathContainsTextParameters();
        private Regex m_Regex;

        public override string Name => "File Path Contains Text";

        public override string Category => "Filesystem";

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

        public override void LocalInit(IList<ExceptionInfo> exceptionInfos)
        {
            base.LocalInit(exceptionInfos);
            RegexOptions regexOptions = RegexOptions.None;
            if (!m_Parameters.CaseSensitive)
            {
                regexOptions |= RegexOptions.IgnoreCase;
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
            if (m_Regex.IsMatch(file.FullName))
            {
                return new MatchResult(MatchResultType.Yes, new string[] { "Yes" });
            }
            return new MatchResult(MatchResultType.No, new string[] { "No" });
        }
    }
}
