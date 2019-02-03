// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace FileSharperCore.Processors.Filesystem
{
    public class RecycleProcessor : SingleFileProcessorBase
    {
        public override string Name => "Move to Recycle Bin";

        public override string Category => "Filesystem";

        public override string Description => "Move the file to the Recycle Bin";

        public override object Parameters => null;

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            try
            {
                FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin,
                    UICancelOption.DoNothing);
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                return new ProcessingResult(ProcessingResultType.Failure, ex.Message, new FileInfo[0]);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[0]);
        }
    }
}
