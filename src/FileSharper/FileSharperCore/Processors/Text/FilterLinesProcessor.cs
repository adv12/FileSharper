// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class FilterLinesParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public LineFilterType FilterType { get; set; } = LineFilterType.KeepMatchingLines;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string TextToMatch { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool UseRegex { get; set; }
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; }
        [PropertyOrder(5, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.MatchInput;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public OutputEncodingType OutputEncoding { get; set; } = OutputEncodingType.MatchInput;
        [PropertyOrder(7, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(8, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(9, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class FilterLinesProcessor : SingleFileProcessorBase
    {
        private FilterLinesParameters m_Parameters = new FilterLinesParameters();
        private Regex m_Regex;

        public override string Category => "Text";

        public override string Name => "Filter lines";

        public override string Description => "Keeps or removes lines matching the specified text.";

        public override object Parameters => m_Parameters;

        public override void LocalInit()
        {
            base.LocalInit();
            RegexOptions regexOptions = RegexOptions.None;
            if (!m_Parameters.CaseSensitive)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            if (m_Parameters.UseRegex)
            {
                m_Regex = new Regex(m_Parameters.TextToMatch, regexOptions);
            }
            else
            {
                m_Regex = new Regex(Regex.Escape(m_Parameters.TextToMatch), regexOptions);
            }
        }

        public override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = Util.ReplaceUtil.Replace(m_Parameters.FileName, file);
            if (!m_Parameters.OverwriteExistingFile && File.Exists(outputFilename))
            {
                return new ProcessingResult(ProcessingResultType.Failure, "File exists", new FileInfo[0]);
            }
            Encoding encoding = TextUtil.DetectEncoding(file);
            string tmpFile = Path.GetTempFileName();
            using (StreamWriter writer = TextUtil.CreateStreamWriterWithAppropriateEncoding(
                tmpFile, encoding, m_Parameters.OutputEncoding))
            {
                writer.NewLine = TextUtil.GetNewline(file, m_Parameters.LineEndings);
                using (StreamReader reader = TextUtil.CreateStreamReaderWithAppropriateEncoding(
                    file, encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        bool matches = m_Regex.IsMatch(line);
                        bool keep = m_Parameters.FilterType == LineFilterType.KeepMatchingLines;
                        if (matches == keep)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                m_Parameters.OverwriteExistingFile, m_Parameters.MoveOriginalToRecycleBin);
        }
    }
}
