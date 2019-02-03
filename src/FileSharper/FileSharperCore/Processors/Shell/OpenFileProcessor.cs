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

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            ProcessingResultType type = ProcessingResultType.Failure;
            string message = "Success";
            try
            {
                System.Diagnostics.Process.Start(file.FullName);
                type = ProcessingResultType.Success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
            }
            return new ProcessingResult(type, message, new FileInfo[] { file });
        }
    }
}
