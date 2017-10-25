using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public override ProcessingResult Process(FileInfo originalFile, string[] values, FileInfo[] filesFromPrevious, CancellationToken token)
        {
            FileInfo[] filesToProcess = ChainFromPrevious ? filesFromPrevious : new FileInfo[] { originalFile };
            if (filesToProcess == null)
            {
                return new ProcessingResult(ProcessingResultType.NotApplicable, new FileInfo[0]);
            }
            List<FileInfo> outputFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            foreach (FileInfo file in filesToProcess)
            {
                FileInfo[] lastOutputs = new FileInfo[0];
                foreach (IProcessor processor in Processors)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        ProcessingResult result = processor?.Process(file, values, lastOutputs ?? new FileInfo[0], token);
                        if (result != null)
                        {
                            if (result.Type == ProcessingResultType.Failure)
                            {
                                resultType = ProcessingResultType.Failure;
                            }
                        }
                        lastOutputs = result != null ? result.OutputFiles : new FileInfo[0];
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        lastOutputs = new FileInfo[0];
                    }
                    if (lastOutputs != null)
                    {
                        outputFiles.AddRange(lastOutputs);
                    }
                }
            }
            return new ProcessingResult(resultType, outputFiles.ToArray());
        }
    }
}
