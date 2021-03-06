// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FileSharperCore.Editors;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class TextParameter
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public PrependAppend PrependOrAppend { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        [Editor(typeof(FileSharperMultiLineTextEditor), typeof(FileSharperMultiLineTextEditor))]
        public string Text { get; set; }
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

    public class PrependOrAppendTextProcessor : SingleFileProcessorBase
    {
        private TextParameter m_Parameters = new TextParameter();

        public override string Name => "Prepend or append text";

        public override string Category => "Text";

        public override string Description => "Prepend or append the specified text to the file";

        public override object Parameters => m_Parameters;

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = ReplaceUtil.Replace(m_Parameters.FileName, file);
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
                using (StreamReader reader = TextUtil.CreateStreamReaderWithAppropriateEncoding(file, encoding))
                {
                    string text = m_Parameters.Text ?? string.Empty;

                    if (m_Parameters.PrependOrAppend == PrependAppend.Prepend)
                    {
                        WriteLines(writer, text);
                    }
                    bool writtenAnyFromFile = false;
                    while (!reader.EndOfStream)
                    {
                        if (writtenAnyFromFile)
                        {
                            writer.WriteLine();
                        }
                        writer.Write(reader.ReadLine());
                        writtenAnyFromFile = true;
                    }
                    // Add a trailing line ending if the original file ended in one
                    if (endsWithNewLine)
                    {
                        writer.WriteLine();
                    }
                    if (m_Parameters.PrependOrAppend == PrependAppend.Append)
                    {
                        WriteLines(writer, text);
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                m_Parameters.OverwriteExistingFile, m_Parameters.MoveOriginalToRecycleBin);
        }

        public void WriteLines(StreamWriter writer, string text)
        {
            string[] lines = Regex.Split(text, "\r\n|\r|\n");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i > 0)
                {
                    writer.WriteLine();
                }
                writer.Write(line);
            }
        }
    }
}
