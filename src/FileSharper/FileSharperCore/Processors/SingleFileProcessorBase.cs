// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class SingleFileProcessorBase : ProcessorBase
    {
        public abstract ProcessingResult Process(FileInfo file, string[] values, CancellationToken token);

        public override ProcessingResult Process(FileInfo originalFile, string[] values, FileInfo[] generatedFiles, CancellationToken token)
        {
            List<FileInfo> resultFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            if (InputFileSource == InputFileSource.PreviousOutput || InputFileSource == InputFileSource.ParentInput)
            {
                if (generatedFiles != null)
                {
                    foreach (FileInfo f in generatedFiles)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            resultFiles.AddRange(Process(f, values, token).OutputFiles);
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
            return new ProcessingResult(resultType, resultFiles.ToArray());
        }
    }
}
