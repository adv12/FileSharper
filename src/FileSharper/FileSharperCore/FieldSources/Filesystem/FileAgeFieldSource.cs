// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FieldSources.Filesystem
{
    public class FileAgeParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public FileDateType TimeSince { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public TimeSpanUnits Units { get; set; } = TimeSpanUnits.Days;
    }

    public class FileAgeFieldSource : FieldSourceBase
    {
        private FileAgeParameters m_Parameters = new FileAgeParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] {
            $"Elapsed Time ({m_Parameters.Units}) Since {m_Parameters.TimeSince}" };

        public override string Category => "Filesystem";

        public override string Name => "File Age";

        public override string Description => "The elapsed time since the file's creation, modified, or accessed date";

        public override object Parameters => m_Parameters;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            DateTime fileDateUtc;
            switch (m_Parameters.TimeSince)
            {
                case FileDateType.Created:
                    fileDateUtc = file.CreationTimeUtc;
                    break;
                case FileDateType.Modified:
                    fileDateUtc = file.LastWriteTimeUtc;
                    break;
                default:
                    fileDateUtc = file.LastAccessTimeUtc;
                    break;
            }
            TimeSpan elapsedTime = DateTime.UtcNow - fileDateUtc;
            double elapsedValue;
            switch (m_Parameters.Units)
            {
                case TimeSpanUnits.Days:
                    elapsedValue = elapsedTime.TotalDays;
                    break;
                case TimeSpanUnits.Hours:
                    elapsedValue = elapsedTime.TotalHours;
                    break;
                case TimeSpanUnits.Minutes:
                    elapsedValue = elapsedTime.TotalMinutes;
                    break;
                case TimeSpanUnits.Seconds:
                    elapsedValue = elapsedTime.TotalSeconds;
                    break;
                default:
                    elapsedValue = elapsedTime.TotalMilliseconds;
                    break;
            }
            return new string[] { elapsedValue.ToString() };
        }
    }
}
