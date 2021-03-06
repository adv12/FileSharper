// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class SingleFileProcessorBase : ProcessorBase
    {
        protected internal abstract ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token);

        public override ProcessingResult Process(FileInfo originalFile,
            MatchResultType matchResultType, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            CancellationToken token)
        {
            StringBuilder message = new StringBuilder();
            List<FileInfo> resultFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            if (whatToProcess == ProcessInput.GeneratedFiles)
            {
                if (generatedFiles != null)
                {
                    foreach (FileInfo f in generatedFiles)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            ProcessingResult result = Process(f, values, token);
                            if (result != null)
                            {
                                if (result.Type == ProcessingResultType.Failure)
                                {
                                    resultType = ProcessingResultType.Failure;
                                }
                                if (result.OutputFiles != null && result.OutputFiles.Length > 0)
                                {
                                    resultFiles.AddRange(result.OutputFiles);
                                }
                                else
                                {
                                    resultFiles.Add(f);
                                }
                                if (message.Length > 0)
                                {
                                    message.Append(" | ");
                                }
                                message.Append(result.Message);
                            }
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, f));
                            if (message.Length > 0)
                            {
                                message.Append(" | ");
                            }
                            message.Append(ex.Message);
                            resultType = ProcessingResultType.Failure;
                        }
                    }
                }
            }
            else
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    ProcessingResult tmp = Process(originalFile, values, token);
                    if (tmp.Type == ProcessingResultType.Failure)
                    {
                        resultType = ProcessingResultType.Failure;
                    }
                    if (tmp.OutputFiles != null)
                    {
                        resultFiles.AddRange(tmp.OutputFiles);
                    }
                    message.Append(tmp.Message);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    resultType = ProcessingResultType.Failure;
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, originalFile));
                    message.Append(ex.Message);
                }
            }
            if (message.Length == 0)
            {
                message.Append(resultType.ToString());
            }
            return new ProcessingResult(resultType, message.ToString(), resultFiles.ToArray());
        }
    }
}
