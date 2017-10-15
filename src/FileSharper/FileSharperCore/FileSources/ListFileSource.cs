// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    yield return new FileInfo(filename);
                }
            }
        }

        public override string Name => "List";

        public override string Description => "A list of file paths";

        public override object Parameters => m_Parameters;
    }
}
