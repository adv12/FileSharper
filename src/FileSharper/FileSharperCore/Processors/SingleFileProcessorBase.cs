// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class SingleFileProcessorBase : ProcessorBase
    {
        public abstract FileInfo[] Process(FileInfo file, string[] values, CancellationToken token);

        public override FileInfo[] Process(FileInfo file, string[] values, FileInfo[] filesFromPrevious, CancellationToken token)
        {
            List<FileInfo> resultFiles = new List<FileInfo>();
            if (ChainFromPrevious)
            {
                foreach (FileInfo f in filesFromPrevious)
                {
                    token.ThrowIfCancellationRequested();
                    resultFiles.AddRange(Process(f, values, token));
                }
            }
            else
            {
                token.ThrowIfCancellationRequested();
                FileInfo[] tmp = Process(file, values, token);
                if (tmp != null)
                {
                    resultFiles.AddRange(tmp);
                }
            }
            return resultFiles.ToArray();
        }
    }
}
