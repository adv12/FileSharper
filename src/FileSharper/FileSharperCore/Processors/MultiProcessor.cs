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
    public class MultiProcessor : ProcessorBase
    {
        public override string Name => "Collect output files";

        public override string Category => "\u0002Compound";

        public override string Description => null;

        public override object Parameters => null;

        public List<IProcessor> Processors { get; } = new List<IProcessor>();

        public override void LocalInit()
        {
            foreach (IProcessor processor in Processors)
            {
                processor.Init(RunInfo);
            }
        }

        public override ProcessingResult Process(FileInfo originalFile,
            MatchResultType matchResultType, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            CancellationToken token)
        {
            StringBuilder message = new StringBuilder();
            List<FileInfo> outputFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            
            foreach (IProcessor processor in Processors)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    ProcessInput what = whatToProcess;
                    if (processor.InputFileSource == InputFileSource.OriginalFile)
                    {
                        what = ProcessInput.OriginalFile;
                    }
                    ProcessingResult result = processor?.Process(originalFile, matchResultType, values,
                        generatedFiles ?? new FileInfo[0], what, token);
                    if (result != null)
                    {
                        if (result.OutputFiles != null)
                        {
                            outputFiles.AddRange(result.OutputFiles);
                        }
                        if (result.Type == ProcessingResultType.Failure)
                        {
                            resultType = ProcessingResultType.Failure;
                        }
                        if (result.Message != null)
                        {
                            if (message.Length > 0)
                            {
                                message.Append(" ");
                            }
                            message.Append(result.Message);
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, originalFile));
                }
            }
            if (message.Length == 0)
            {
                message.Append("Success");
            }
            return new ProcessingResult(resultType, message.ToString(), outputFiles.ToArray());
        }

        public override void ProcessAggregated(CancellationToken token)
        {
            foreach (IProcessor processor in Processors)
            {
                try
                {
                    processor?.ProcessAggregated(token);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
        }

        public override void LocalCleanup()
        {
            foreach (IProcessor processor in Processors)
            {
                processor.Cleanup();
            }
        }
    }
}
