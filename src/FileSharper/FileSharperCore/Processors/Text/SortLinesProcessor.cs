﻿// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public LineEndings LineEndings { get; set; } = LineEndings.MatchInput;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public OutputEncodingType OutputEncoding { get; set; } = OutputEncodingType.MatchInput;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(7, UsageContextEnum.Both)]
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

        public override void LocalInit()
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

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = Util.ReplaceUtil.Replace(m_Parameters.FileName, file);
            if (!m_Parameters.OverwriteExistingFile && File.Exists(outputFilename))
            {
                return new ProcessingResult(ProcessingResultType.Failure, "File exists", new FileInfo[0]);
            }
            Encoding detectedEncoding = TextUtil.DetectEncoding(file);
            string[] lines = File.ReadAllLines(file.FullName, detectedEncoding);
            Array.Sort(lines, m_Comparer);
            string tmpFile = Path.GetTempFileName();
            bool endsWithNewLine = TextUtil.FileEndsWithNewline(file, detectedEncoding);
            using (StreamWriter writer = TextUtil.CreateStreamWriterWithAppropriateEncoding(
                tmpFile, detectedEncoding, m_Parameters.OutputEncoding))
            {
                writer.NewLine = TextUtil.GetNewline(file, m_Parameters.LineEndings);
                int i = 0;
                foreach (string line in lines)
                {
                    writer.Write(line);
                    if (i < lines.Length - 1 || endsWithNewLine)
                    {
                        writer.WriteLine();
                    }
                    i++;
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                m_Parameters.OverwriteExistingFile, m_Parameters.MoveOriginalToRecycleBin);
        }
    }
}
