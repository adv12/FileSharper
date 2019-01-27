// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Image
{

    public class ImageResizeParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string NewPath { get; set; } = @"{DirectoryName}\{NameWithoutExtension}.jpg";
        [PropertyOrder(2, UsageContextEnum.Both)]
        public ImageSaveFormat Format { get; set; } = ImageSaveFormat.Jpeg;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public MediaDimension ResizeBy { get; set; } = MediaDimension.Width;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public int Size { get; set; } = 48;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool Overwrite { get; set; } = false;
    }

    public class ImageResizeProcessor : SingleFileProcessorBase
    {
        private ImageResizeParameters m_Parameters = new ImageResizeParameters();

        public override string Name => "Create resized image";

        public override string Category => "Image";

        public override string Description => "Export a resized image";

        public override object Parameters => m_Parameters;

        public override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            int width;
            int height;
            bool isBitmap = false;
            string outPath = null;
            FileInfo[] generatedFiles = new FileInfo[0];

            ProcessingResultType resultType = ProcessingResultType.Failure;
            string message = "Success";
            Bitmap b = null;
            try
            {
                b = new Bitmap(file.FullName);
                isBitmap = true;
            }
            catch (Exception ex)
            {
                resultType = ProcessingResultType.NotApplicable;
            }
            if (isBitmap)
            {
                try
                {
                    if (m_Parameters.ResizeBy == MediaDimension.Width)
                    {
                        width = m_Parameters.Size;
                        height = (int)Math.Round((double)b.Height * width / b.Width);
                    }
                    else
                    {
                        height = m_Parameters.Size;
                        width = (int)Math.Round((double)b.Width * height / b.Height);
                    }
                    using (Bitmap bout = new Bitmap(b, new Size(width, height)))
                    {
                        outPath = ReplaceUtil.Replace(m_Parameters.NewPath, file);
                        ImageFormat format = ImageFormat.Jpeg;
                        switch (m_Parameters.Format)
                        {
                            case ImageSaveFormat.Bitmap:
                                format = ImageFormat.Bmp;
                                break;
                            case ImageSaveFormat.Exif:
                                format = ImageFormat.Exif;
                                break;
                            case ImageSaveFormat.Gif:
                                format = ImageFormat.Gif;
                                break;
                            case ImageSaveFormat.Jpeg:
                                format = ImageFormat.Jpeg;
                                break;
                            case ImageSaveFormat.Png:
                                format = ImageFormat.Png;
                                break;
                            case ImageSaveFormat.Tiff:
                                format = ImageFormat.Tiff;
                                break;
                        }
                        if (m_Parameters.Overwrite || !File.Exists(outPath))
                        {
                            string outDirPath = Path.GetDirectoryName(outPath);
                            Directory.CreateDirectory(outDirPath);
                            bout.Save(outPath, format);
                            resultType = ProcessingResultType.Success;
                            generatedFiles = new FileInfo[] { new FileInfo(outPath) };
                        }
                        else
                        {
                            resultType = ProcessingResultType.Failure;
                            message = "Target file exists";
                            generatedFiles = new FileInfo[0];
                        }
                        
                        
                    }
                }
                catch(Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                    message = ex.Message;
                    resultType = ProcessingResultType.Failure;
                }
                finally
                {
                    b?.Dispose();
                }
            }
            return new ProcessingResult(resultType, message, generatedFiles);
        }
    }
}
