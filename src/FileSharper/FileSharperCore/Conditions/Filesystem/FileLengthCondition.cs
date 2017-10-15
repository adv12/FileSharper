// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
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

    public class FileLengthComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public ComparisonType ComparisonType { get; set; } = ComparisonType.LessThan;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public double Value { get; set; } = 1;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public SizeUnits Units { get; set; } = SizeUnits.Kilobytes;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool Metric { get; set; }
        [PropertyOrder(5, UsageContextEnum.Both)]
        public string Format { get; set; } = "F2";
    }

    public class FileLengthCondition : ConditionBase
    {
        private FileLengthComparisonParameters m_Parameters = new FileLengthComparisonParameters();

        public override string Name => "File Length";

        public override string Description => "File length compares to the specified length";

        public override string Category => "Filesystem";

        public override object Parameters => m_Parameters;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "File Length (" + UnitUtil.GetUnitSymbol(m_Parameters.Units) + ")" };

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            double size = UnitUtil.ConvertSize(file.Length, SizeUnits.Bytes, m_Parameters.Units, m_Parameters.Metric);
            MatchResultType resultType = CompareUtil.Compare(size, m_Parameters.ComparisonType, m_Parameters.Value);
            return new MatchResult(resultType, new string[] { size.ToString(m_Parameters.Format) });
        }
    }
}
