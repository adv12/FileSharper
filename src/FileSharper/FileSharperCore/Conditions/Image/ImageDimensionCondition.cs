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
    public class ImageDimensionComparisonParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public ImageDimension Dimension { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public ComparisonType ComparisonType { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public int Size { get; set; }
    }

    public class ImageDimensionCondition : ConditionBase
    {
        private ImageDimensionComparisonParameters m_Parameters = new ImageDimensionComparisonParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { m_Parameters.Dimension.ToString() };

        public override string Name => "Image Dimension";

        public override string Category => "Image";

        public override string Description => "Compares the width or height of an image to the specified value";

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
            int imageSize = m_Parameters.Dimension == ImageDimension.Height ? bitmap.Height : bitmap.Width;
            MatchResultType resultType = CompareUtil.Compare(imageSize, m_Parameters.ComparisonType, m_Parameters.Size);
            return new MatchResult(resultType, imageSize.ToString());
        }
    }
}
