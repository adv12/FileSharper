// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Drawing;
using System.IO;

namespace FileSharperCore.FileCaches
{
    public class BitmapFileCache : IFileCache
    {
        public Bitmap Bitmap
        {
            get;
            private set;
        }

        public bool IsBitmap
        {
            get;
            private set;
        }

        public void Load(FileInfo file)
        {
            try
            {
                Bitmap = new Bitmap(file.FullName);
                IsBitmap = true;
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null;
        }
    }
}
