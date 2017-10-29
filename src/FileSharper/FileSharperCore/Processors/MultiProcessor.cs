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

        public override string Category => " Compound";

        public override string Description => "";

        public override HowOften ProducesFiles => HowOften.Sometimes;

        public override object Parameters => null;

        public List<IProcessor> Processors { get; } = new List<IProcessor>();

        public override ProcessingResult Process(FileInfo originalFile, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
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
                    ProcessingResult result = processor?.Process(originalFile, values,
                        generatedFiles ?? new FileInfo[0], what, exceptionProgress, token);
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
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    exceptionProgress.Report(new ExceptionInfo(ex, originalFile));
                }
            }
            if (message.Length == 0)
            {
                message.Append("Success");
            }
            return new ProcessingResult(resultType, message.ToString(), outputFiles.ToArray());
        }
    }
}
