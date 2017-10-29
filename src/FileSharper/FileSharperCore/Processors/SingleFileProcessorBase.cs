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
        public abstract ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token);

        public override ProcessingResult Process(FileInfo originalFile, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
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
                            ProcessingResult result = Process(f, values, exceptionProgress, token);
                            if (result != null)
                            {
                                if (result.OutputFiles != null)
                                {
                                    resultFiles.AddRange(result.OutputFiles);
                                }
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            exceptionProgress.Report(new ExceptionInfo(ex, f));
                            if (message.Length > 0)
                            {
                                message.Append(" ");
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
                    ProcessingResult tmp = Process(originalFile, values, exceptionProgress, token);
                    if (tmp.Type == ProcessingResultType.Failure)
                    {
                        resultType = ProcessingResultType.Failure;
                    }
                    if (tmp.OutputFiles != null)
                    {
                        resultFiles.AddRange(tmp.OutputFiles);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch
                {
                    resultType = ProcessingResultType.Failure;
                }
            }
            if (message.Length == 0)
            {
                message.Append("Success");
            }
            return new ProcessingResult(resultType, message.ToString(), resultFiles.ToArray());
        }
    }
}
