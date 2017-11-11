// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Text
{

    public class WordCountComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public ComparisonType ComparisonType { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public int Count { get; set; }
    }

    public class WordCountCondition : ConditionBase
    {
        private WordCountComparisonParameters m_Parameters = new WordCountComparisonParameters();

        public override string Name => "Word Count";

        public override string Category => "Text";

        public override string Description => "Compares the word count to the specified value";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Word Count" };

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            int wordCount = 0;
            MatchResultType resultType = MatchResultType.NotApplicable;
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    wordCount = TextUtil.GetWordCount(reader);
                }
                resultType = CompareUtil.Compare(wordCount, m_Parameters.ComparisonType, m_Parameters.Count);
            }
            catch (Exception ex)
            {

            }
            return new MatchResult(resultType, wordCount.ToString());
        }
    }
}
