// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public class OpenFileProcessor: SingleFileProcessorBase
    {
        public override string Name => "Open file";

        public override string Description => "Opens the file in its default editor";

        public override object Parameters => null;

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            System.Diagnostics.Process.Start(file.FullName);
            return new ProcessingResult(ProcessingResultType.Success, "Success", null);
        }
    }
}
