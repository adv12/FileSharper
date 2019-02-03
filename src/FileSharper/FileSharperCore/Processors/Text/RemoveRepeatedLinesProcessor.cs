// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Text;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class RemoveRepeatedLinesParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.MatchInput;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public OutputEncodingType OutputEncoding { get; set; } = OutputEncodingType.MatchInput;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class RemoveRepeatedLinesProcessor : SingleFileProcessorBase
    {
        private RemoveRepeatedLinesParameters m_Parameters = new RemoveRepeatedLinesParameters();

        public override string Category => "Text";

        public override string Name => "Remove repeated lines";

        public override string Description => "Removes repeated sequential lines (like the UNIX uniq command)";

        public override object Parameters => m_Parameters;

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = Util.ReplaceUtil.Replace(m_Parameters.FileName, file);
            if (!m_Parameters.OverwriteExistingFile && File.Exists(outputFilename))
            {
                return new ProcessingResult(ProcessingResultType.Failure, "File exists", new FileInfo[0]);
            }
            Encoding encoding = TextUtil.DetectEncoding(file);
            string tmpFile = Path.GetTempFileName();
            bool endsWithNewLine = TextUtil.FileEndsWithNewline(file, encoding);
            using (StreamWriter writer = TextUtil.CreateStreamWriterWithAppropriateEncoding(
                tmpFile, encoding, m_Parameters.OutputEncoding))
            {
                writer.NewLine = TextUtil.GetNewline(file, m_Parameters.LineEndings);
                using (StreamReader reader = TextUtil.CreateStreamReaderWithAppropriateEncoding(
                    file, encoding))
                {
                    string lastLine = null;
                    bool writtenAny = false;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (lastLine != line)
                        {
                            if (writtenAny)
                            {
                                writer.WriteLine();
                            }
                            writer.Write(line);
                            writtenAny = true;
                        }
                        lastLine = line;
                    }
                    if (endsWithNewLine)
                    {
                        writer.WriteLine();
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                m_Parameters.OverwriteExistingFile, m_Parameters.MoveOriginalToRecycleBin);
        }
    }
}
