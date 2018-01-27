// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FieldSources.Filesystem
{
    public class FileLengthParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public SizeUnits Units { get; set; } = SizeUnits.Kilobytes;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool Metric { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string Format { get; set; } = "F2";
    }

    public class FileLengthFieldSource : FieldSourceBase
    {
        private FileLengthParameters m_Parameters = new FileLengthParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "File Length (" + UnitUtil.GetUnitSymbol(m_Parameters.Units) + ")" };

        public override string Name => "File Length";

        public override string Description => "The length of the file in the specified units";

        public override string Category => "Filesystem";

        public override object Parameters => m_Parameters;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> cacheTypes, CancellationToken token)
        {
            return new string[] { UnitUtil.ConvertSize(file.Length, SizeUnits.Bytes, m_Parameters.Units, m_Parameters.Metric).ToString(m_Parameters.Format) };
        }
    }
}
