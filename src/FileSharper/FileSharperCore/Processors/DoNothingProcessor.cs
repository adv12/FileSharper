// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public class DoNothingProcessor : ProcessorBase
    {
        public override string Name => null;

        public override string Category => string.Empty;

        public override string Description => null;

        public override object Parameters => null;

        public override ProcessingResult Process(FileInfo file, string[] values, FileInfo[] generatedFiles, ProcessInput whatToProcess, CancellationToken token)
        {
            return new ProcessingResult(ProcessingResultType.Success, null);
        }
    }
}
