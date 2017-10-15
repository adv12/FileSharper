// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors
{
    public class CopyFileParameters
    {
        public string NewPath { get; set; } = @"{Desktop}\{Name}";
    }

    public class CopyFileProcessor : SingleFileProcessorBase
    {
        private CopyFileParameters m_Parameters = new CopyFileParameters();

        public override string Name => "Copy file";

        public override string Description => "Copy file to the specified location";

        public override object Parameters => m_Parameters;

        public override bool ProducesFiles => true;

        public override FileInfo[] Process(FileInfo file, string[] values, CancellationToken token)
        {
            string newPath = ReplaceUtil.Replace(m_Parameters.NewPath, file);
            file.CopyTo(newPath);
            return new FileInfo[] { new FileInfo(newPath) };
        }
    }
}
