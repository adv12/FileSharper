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

        public override ProcessingResult Process(FileInfo file, string[] values, FileInfo[] filesFromPrevious, CancellationToken token)
        {
            List<FileInfo> resultFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            if (ChainFromPrevious)
            {
                foreach (FileInfo f in filesFromPrevious)
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
            else
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    ProcessingResult tmp = Process(file, values, token);
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
