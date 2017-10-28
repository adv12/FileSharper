// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace FileSharperCore.Processors
{
    public class RecycleProcessor : SingleFileProcessorBase
    {
        public override string Name => "Move to Recycle Bin";

        public override string Description => "Move the file to the Recycle Bin";

        public override object Parameters => null;

        public override ProcessingResult Process(FileInfo file, string[] values, CancellationToken token)
        {
            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin,
                UICancelOption.DoNothing);
            return null;
        }
    }
}
