// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using FileSharperCore.FileCaches;

namespace FileSharperCore.Outputs.Image
{
    public class ImageDimensionsParameters
    {
        [DisplayName("Dimension(s)")]
        public ImageDimensionOrDimensions Dimension { get; set; } = ImageDimensionOrDimensions.WidthHeight;
    }

    public class ImageDimensionsOutput : OutputBase
    {
        private ImageDimensionsParameters m_Parameters = new ImageDimensionsParameters();

        public override int ColumnCount
        {
            get
            {
                switch (m_Parameters.Dimension)
                {
                    case ImageDimensionOrDimensions.Height:
                    case ImageDimensionOrDimensions.Width:
                        return 1;
                    default:
                        return 2;
                }
            }
        }

        public override string[] ColumnHeaders
        {
            get
            {
                switch (m_Parameters.Dimension)
                {
                    case ImageDimensionOrDimensions.Width:
                        return new string[] { "Width" };
                    case ImageDimensionOrDimensions.Height:
                        return new string[] { "Height" };
                    case ImageDimensionOrDimensions.WidthHeight:
                        return new string[] { "Width", "Height" };
                    case ImageDimensionOrDimensions.HeightWidth:
                        return new string[] { "Height", "Width" };
                }
                return new string[] { "N/A" };
            }
        }

        public override string Category => "Image";

        public override string Name => "Image Dimensions";

        public override string Description => "The image's width, height, or both";

        public override object Parameters => m_Parameters;

        public override Type[] CacheTypes => new Type[] { typeof(BitmapFileCache) };

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            BitmapFileCache cache = (BitmapFileCache)fileCaches[typeof(BitmapFileCache)];
            if (!cache.IsBitmap)
            {
                if (ColumnCount == 2)
                {
                    return new string[] { "N/A", "N/A" };
                }
                return new string[] { "N/A" };
            }
            Bitmap bitmap = cache.Bitmap;
            switch (m_Parameters.Dimension)
            {
                case ImageDimensionOrDimensions.Width:
                    return new string[] { bitmap.Width.ToString() };
                case ImageDimensionOrDimensions.Height:
                    return new string[] { bitmap.Height.ToString() };
                case ImageDimensionOrDimensions.WidthHeight:
                    return new string[] { bitmap.Width.ToString(), bitmap.Height.ToString() };
                case ImageDimensionOrDimensions.HeightWidth:
                    return new string[] { bitmap.Height.ToString(), bitmap.Width.ToString() };
            }
            return new string[ColumnCount];
        }
    }
}
