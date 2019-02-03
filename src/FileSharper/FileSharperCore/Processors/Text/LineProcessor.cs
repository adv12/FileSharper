// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Text;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors.Text
{
    public abstract class LineProcessor : SingleFileProcessorBase
    {
        protected internal abstract bool MoveOriginalToRecycleBin { get; }

        protected internal abstract LineEndings LineEndings { get; }

        protected internal abstract OutputEncodingType OutputEncodingType { get; }

        protected internal abstract string FileName { get; }

        protected internal abstract bool OverwriteExistingFile { get; }

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = Util.ReplaceUtil.Replace(FileName, file);
            if (!OverwriteExistingFile && File.Exists(outputFilename))
            {
                return new ProcessingResult(ProcessingResultType.Failure, "File exists", new FileInfo[0]);
            }
            Encoding encoding = TextUtil.DetectEncoding(file);
            string tmpFile = Path.GetTempFileName();
            bool endsWithNewLine = TextUtil.FileEndsWithNewline(file, encoding);
            using (StreamWriter writer = TextUtil.CreateStreamWriterWithAppropriateEncoding(
                tmpFile, encoding, OutputEncodingType))
            {
                writer.NewLine = TextUtil.GetNewline(file, LineEndings);
                using (StreamReader reader = TextUtil.CreateStreamReaderWithAppropriateEncoding(
                    file, encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        line = TransformLine(line);
                        if (!reader.EndOfStream || endsWithNewLine)
                        {
                            writer.WriteLine(line);
                        }
                        else
                        {
                            writer.Write(line);
                        }
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                OverwriteExistingFile, MoveOriginalToRecycleBin);
        }

        protected internal abstract string TransformLine(string line);
    }
}
