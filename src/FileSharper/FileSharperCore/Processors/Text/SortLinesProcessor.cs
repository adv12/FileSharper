// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class SortLinesParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public StringComparisonType ComparisonType { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool Reverse { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.SystemDefault;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class SortLinesProcessor : SingleFileProcessorBase
    {
        private SortLinesParameters m_Parameters = new SortLinesParameters();

        private IComparer<string> m_Comparer;
        
        public override string Name => "Sort lines";

        public override string Category => "Text";

        public override string Description => "Sorts the lines of the file alphabetically";

        public override object Parameters => m_Parameters;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            switch (m_Parameters.ComparisonType)
            {
                case StringComparisonType.CaseInsensitive:
                    m_Comparer = StringComparer.CurrentCultureIgnoreCase;
                    break;
                case StringComparisonType.CaseSensitive:
                    m_Comparer = StringComparer.CurrentCulture;
                    break;
                case StringComparisonType.Ordinal:
                    m_Comparer = StringComparer.Ordinal;
                    break;
            }
            if (m_Parameters.Reverse)
            {
                m_Comparer = new ReverseComparer<string>(m_Comparer);
            }
        }

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            string[] lines = File.ReadAllLines(file.FullName);
            Array.Sort(lines, m_Comparer);
            string tmpFile = Path.GetTempFileName();
            using (StreamWriter writer = new StreamWriter(tmpFile))
            {
                writer.NewLine = TextUtil.GetNewline(m_Parameters.LineEndings);
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, m_Parameters.FileName, tmpFile,
                m_Parameters.OverwriteExistingFile, m_Parameters.MoveOriginalToRecycleBin);
        }
    }
}
