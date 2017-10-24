// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using FileSharperCore.FileCaches;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Image
{

    public class ImageAspectRatioComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public ComparisonType ComparisonType { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public uint Width { get; set; } = 1;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public uint Height { get; set; } = 1;
    }

    public class ImageAspectRatioCondition : ConditionBase
    {
        private ImageAspectRatioComparisonParameters m_Parameters = new ImageAspectRatioComparisonParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Aspect Ratio" };

        public override string Name => "Image Aspect Ratio";

        public override string Category => "Image";

        public override string Description => "Compares the image's aspect ratio to the specified value";

        public override object Parameters => m_Parameters;

        public override Type[] CacheTypes => new Type[] { typeof(BitmapFileCache) };

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            BitmapFileCache cache = fileCaches[typeof(BitmapFileCache)] as BitmapFileCache;
            if (cache == null || !cache.IsBitmap)
            {
                return new MatchResult(MatchResultType.NotApplicable, "N/A");
            }
            Bitmap bitmap = cache.Bitmap;
            PositiveFraction val = new PositiveFraction((uint)bitmap.Width, (uint)bitmap.Height);
            PositiveFraction paramVal = new PositiveFraction(m_Parameters.Width, m_Parameters.Height);
            if (paramVal.Denominator == 0)
            {
                return new MatchResult(MatchResultType.NotApplicable, "N/A");
            }
            MatchResultType resultType = CompareUtil.Compare(val, m_Parameters.ComparisonType, paramVal);
            return new MatchResult(resultType, val.ToString());
        }
    }
}
