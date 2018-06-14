// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FileSources
{
    public class FileListParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public List<string> Files { get; set; } = new List<string>();
    }

    public class ListFileSource : FileSourceBase
    {
        private FileListParameters m_Parameters = new FileListParameters();

        public override IEnumerable<FileInfo> Files
        {
            get
            {
                foreach (string filename in m_Parameters.Files)
                {
                    if (RunInfo.StopRequested)
                    {
                        yield break;
                    }
                    RunInfo.CancellationToken.ThrowIfCancellationRequested();
                    string name = filename;
                    name = ReplaceUtil.Replace(name, (FileInfo)null);
                    yield return new FileInfo(name);
                }
            }
        }

        public override string Name => "List of Paths";

        public override string Category => "Miscellaneous";

        public override string Description => "A list of file paths";

        public override object Parameters => m_Parameters;
    }
}
