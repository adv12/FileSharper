// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FieldSources.Filesystem
{

    public class FileDateParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public FileDateType FileDateType { get; set; } = FileDateType.Modified;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string FormatString { get; set; } = "yyyy/MM/dd hh:mm:ss tt";
    }

    public class FileDateFieldSource : FieldSourceBase
    {
        private FileDateParameters m_Parameters = new FileDateParameters();

        public override string Name => "File Date";

        public override string Description => "The file's creation, modified, or accessed date";

        public override string Category => "Filesystem";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders
        {
            get
            {
                string headerText = string.Empty;
                switch (m_Parameters.FileDateType)
                {
                    case FileDateType.Created:
                        headerText = "Created Date";
                        break;
                    case FileDateType.Modified:
                        headerText = "Last Modified Date";
                        break;
                    default:
                        headerText = "Last Accessed Date";
                        break;
                }
                return new string[] { headerText };
            }
        }

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> cacheTypes, CancellationToken token)
        {
            DateTime fileDate;
            switch (m_Parameters.FileDateType)
            {
                case FileDateType.Created:
                    fileDate = file.CreationTime;
                    break;
                case FileDateType.Modified:
                    fileDate = file.LastWriteTime;
                    break;
                default:
                    fileDate = file.LastAccessTime;
                    break;
            }
            return new string[] { fileDate.ToString(m_Parameters.FormatString) };
        }
    }
}
