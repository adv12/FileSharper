// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public class OpenFileProcessor: SingleFileProcessorBase
    {
        public override string Name => "Open file";

        public override string Description => "Opens the file in its default editor";

        public override object Parameters => null;

        public override FileInfo[] Process(FileInfo file, string[] values, CancellationToken token)
        {
            System.Diagnostics.Process.Start(file.FullName);
            return null;
        }
    }
}
