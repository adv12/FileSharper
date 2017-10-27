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

        public override ProcessingResult Process(FileInfo originalFile, string[] values, FileInfo[] generatedFiles, CancellationToken token)
        {
            List<FileInfo> outputFiles = new List<FileInfo>();
            ProcessingResultType resultType = ProcessingResultType.Success;
            
            foreach (IProcessor processor in Processors)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    ProcessingResult result = processor?.Process(originalFile, values, generatedFiles ?? new FileInfo[0], token);
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
                    }
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                }
            }
            
            return new ProcessingResult(resultType, outputFiles.ToArray());
        }
    }
}
