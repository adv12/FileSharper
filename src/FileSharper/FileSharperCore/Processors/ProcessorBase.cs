// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Microsoft.VisualBasic.FileIO;

namespace FileSharperCore.Processors
{
    public abstract class ProcessorBase : PluggableItemBase, IProcessor
    {

        public const string ORIGINAL_FILE_PATH = @"{DirectoryName}\{NameWithoutExtension}{Extension}";

        public InputFileSource InputFileSource { get; set; }

        public abstract ProcessingResult Process(FileInfo originalFile,
            MatchResultType matchResultType, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token);

        public virtual void ProcessAggregated(IProgress<ExceptionInfo> exceptionProgress,
            CancellationToken token)
        {

        }

        public bool CopyAndDeleteTempFile(string tmpFile, string outFile, bool overwrite, bool moveOriginalToRecycleBin)
        {
            bool copied = false;
            if (moveOriginalToRecycleBin && File.Exists(outFile))
            {
                FileSystem.DeleteFile(outFile, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin,
                UICancelOption.DoNothing);
                copied = true;
            }
            try
            {
                File.Copy(tmpFile, outFile, overwrite);
                copied = true;
            }
            catch (IOException)
            {
                
            }
            try
            {
                File.Delete(tmpFile);
            }
            catch (Exception)
            {

            }
            return copied;
        }

        public ProcessingResult GetProcessingResultFromCopyAndDeleteTempFile(FileInfo file, string outFileName,
            string tempFileName, bool overwrite, bool moveOriginalToRecycleBin)
        {
            bool wroteFile = CopyAndDeleteTempFile(tempFileName, outFileName, overwrite, moveOriginalToRecycleBin);
            if (wroteFile)
            {
                return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { new FileInfo(outFileName) });
            }
            else
            {
                return new ProcessingResult(ProcessingResultType.Failure, "Failure", new FileInfo[] { });
            }
        }
    }
}
