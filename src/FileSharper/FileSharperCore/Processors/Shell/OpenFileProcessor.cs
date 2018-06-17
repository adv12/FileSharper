// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors.Shell
{
    public class OpenFileProcessor: SingleFileProcessorBase
    {
        public override string Name => "Open file";

        public override string Category => "Shell";

        public override string Description => "Opens the file in its default editor";

        public override object Parameters => null;

        public override ProcessingResult Process(FileInfo file, string[] values,
            IList<ExceptionInfo> exceptionInfos, CancellationToken token)
        {
            System.Diagnostics.Process.Start(file.FullName);
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { file });
        }
    }
}
