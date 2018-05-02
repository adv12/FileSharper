// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Filesystem
{
    public class FileAgeComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public FileDateType TimeSince { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public SimpleComparisonType ComparisonType { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public double TimeSpan { get; set; }
        [PropertyOrder(4, UsageContextEnum.Both)]
        public TimeSpanUnits Units { get; set; } = TimeSpanUnits.Days;
    }

    public class FileAgeCondition : ConditionBase
    {
        private FileAgeComparisonParameters m_Parameters = new FileAgeComparisonParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] {
            $"Elapsed Time ({m_Parameters.Units}) Since {m_Parameters.TimeSince}" };

        public override string Category => "Filesystem";

        public override string Name => "File Age";

        public override string Description => "Tests whether the elapsed time since a file date " +
            "is greater or less than the specified period";

        public override object Parameters => m_Parameters;

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
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
            MatchResultType resultType = CompareUtil.Compare(elapsedValue,
                m_Parameters.ComparisonType, m_Parameters.TimeSpan);
            return new MatchResult(resultType, elapsedValue.ToString());
        }
    }
}
