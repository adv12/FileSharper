// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using FileSharperCore.FileCaches;

namespace FileSharperCore.Outputs.Image
{
    public class ImageDimensionsOutput : OutputBase
    {
        public override int ColumnCount => 2;

        public override string[] ColumnHeaders => new string[] { "Width", "Height" };

        public override string Category => "Image";

        public override string Name => "Image Dimensions";

        public override string Description => "The image's width and height";

        public override object Parameters => null;

        public override Type[] CacheTypes => new Type[] { typeof(BitmapFileCache) };

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            BitmapFileCache cache = (BitmapFileCache)fileCaches[typeof(BitmapFileCache)];
            if (!cache.IsBitmap)
            {
                return new string[] { "N/A", "N/A" };
            }
            Bitmap bitmap = cache.Bitmap;
            return new string[] { bitmap.Width.ToString(), bitmap.Height.ToString() };
        }
    }
}
