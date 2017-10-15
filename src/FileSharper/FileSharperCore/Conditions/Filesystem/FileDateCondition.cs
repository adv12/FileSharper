// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using FileSharperCore.Editors;
using System.Collections.Generic;

namespace FileSharperCore.Conditions.Filesystem
{
    public class FileDateComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public FileDateType FileDateType { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public TimeComparisonType ComparisonType { get; set; }
        [Editor(typeof(DateTimePickerEditor), typeof(DateTimePicker))]
        [PropertyOrder(3, UsageContextEnum.Both)]
        public DateTime Date { get; set; } = DateTime.Now;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public string OutputFormat { get; set; } = "yyyy/MM/dd hh:mm:ss tt";
    }

    public class FileDateCondition : ConditionBase
    {
        private FileDateComparisonParameters m_Parameters = new FileDateComparisonParameters();

        public override string Name => "File Date";

        public override string Description => "File date compares to the specified date";

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

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
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
            bool matches = false;
            switch (m_Parameters.ComparisonType)
            {
                case TimeComparisonType.After:
                    matches = (fileDate > m_Parameters.Date);
                    break;
                default:
                    matches = (fileDate < m_Parameters.Date);
                    break;
            }
            MatchResultType resultType = matches ? MatchResultType.Yes : MatchResultType.No;
            return new MatchResult(resultType, new string[] { fileDate.ToString(m_Parameters.OutputFormat) });
        }
    }
}
