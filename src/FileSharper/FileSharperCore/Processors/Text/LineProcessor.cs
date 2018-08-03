// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors.Text
{
    public abstract class LineProcessor : SingleFileProcessorBase
    {

        protected abstract bool MoveOriginalToRecycleBin { get; }

        protected abstract LineEndings LineEndings { get; }

        protected abstract OutputEncodingType OutputEncodingType { get; }

        protected abstract string FileName { get; }

        protected abstract bool OverwriteExistingFile { get; }

        public override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string outputFilename = Util.ReplaceUtil.Replace(FileName, file);
            if (!OverwriteExistingFile && File.Exists(outputFilename))
            {
                return new ProcessingResult(ProcessingResultType.Failure, "File exists", new FileInfo[0]);
            }
            Encoding encoding = TextUtil.DetectEncoding(file);
            string tmpFile = Path.GetTempFileName();
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
                        writer.WriteLine(line);
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, outputFilename, tmpFile,
                OverwriteExistingFile, MoveOriginalToRecycleBin);
        }

        protected abstract string TransformLine(string line);
    }
}
