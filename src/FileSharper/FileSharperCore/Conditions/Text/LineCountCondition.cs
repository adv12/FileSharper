// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Conditions.Text
{
    public class LineCountCondition : ConditionBase
    {
        private CountComparisonParameters m_Parameters = new CountComparisonParameters();

        public override string Name => "Line Count";

        public override string Category => "Text";

        public override string Description => "Compares the line count to the specified value";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Line Count" };

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            int lineCount = 0;
            MatchResultType resultType = MatchResultType.NotApplicable;
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    lineCount = TextUtil.GetLineCount(reader, token);
                }
                resultType = CompareUtil.Compare(lineCount, m_Parameters.ComparisonType, m_Parameters.Count);
            }
            catch (Exception ex)
            {

            }
            return new MatchResult(resultType, lineCount.ToString());
        }
    }
}
