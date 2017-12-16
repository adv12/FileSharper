// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors.Text
{
    public abstract class LineProcessor : SingleFileProcessorBase
    {

        protected abstract bool MoveOriginalToRecycleBin { get; }

        protected abstract LineEndings LineEndings { get; }

        protected abstract string FileName { get; }

        protected abstract bool OverwriteExistingFile { get; }

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            string tmpFile = Path.GetTempFileName();
            using (StreamWriter writer = new StreamWriter(tmpFile))
            {
                writer.NewLine = TextUtil.GetNewline(LineEndings);
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        line = TransformLine(line);
                        writer.WriteLine(line);
                    }
                }
            }
            return GetProcessingResultFromCopyAndDeleteTempFile(file, FileName, tmpFile,
                OverwriteExistingFile, MoveOriginalToRecycleBin);
        }

        protected abstract string TransformLine(string line);
    }
}
