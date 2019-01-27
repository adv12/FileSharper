// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using FileSharperCore.Processors.Image;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Image
{
    [TestClass]
    public class ImageResizeProcessorTests : TestBase
    {
        [TestMethod]
        public void Width100Jpeg()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Jpeg);
        }

        [TestMethod]
        public void Width1200Jpeg()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 1200, ImageSaveFormat.Jpeg);
        }

        [TestMethod]
        public void Height100Jpeg()
        {
            ResizeBeetleSuccess(MediaDimension.Height, 100, ImageSaveFormat.Jpeg);
        }

        [TestMethod]
        public void Height900Jpeg()
        {
            ResizeBeetleSuccess(MediaDimension.Height, 900, ImageSaveFormat.Jpeg);
        }

        [TestMethod]
        public void Width100Png()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Png);
        }

        [TestMethod]
        public void Width100Bmp()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Bitmap);
        }

        [TestMethod]
        public void Width100Gif()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Gif);
        }

        [TestMethod]
        public void Width100Exif()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Exif);
        }

        [TestMethod]
        public void Width100Tiff()
        {
            ResizeBeetleSuccess(MediaDimension.Width, 100, ImageSaveFormat.Tiff);
        }

        [TestMethod]
        public void ResizeBeetleOverwriteFalse()
        {
            ImageResizeProcessor processor = GetProcessor(MediaDimension.Width, 100, ImageSaveFormat.Jpeg, false);
            string outPath = (string)processor.GetParameter("NewPath");

            File.WriteAllBytes(outPath, new byte[0]);

            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("beetle.jpg"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();

            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void ResizeBeetleOverwriteTrueSuccess()
        {
            ImageResizeProcessor processor = GetProcessor(MediaDimension.Width, 100, ImageSaveFormat.Jpeg, true);
            string outPath = (string)processor.GetParameter("NewPath");

            File.WriteAllBytes(outPath, new byte[0]);

            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("beetle.jpg"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();

            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }

        [TestMethod]
        public void ResizeBeetleOverwriteTrueFailure()
        {
            ImageResizeProcessor processor = GetProcessor(MediaDimension.Width, 100, ImageSaveFormat.Jpeg, true);
            string outPath = (string)processor.GetParameter("NewPath");

            ProcessingResult result = null;
            File.WriteAllBytes(outPath, new byte[0]);
            using (FileStream fs = File.Create(outPath))
            {
                processor.Init(RunInfo);
                result = processor.Process(GetTestFile("beetle.jpg"),
                    MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                    CancellationToken.None);
                processor.Cleanup();
            }

            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        private void ResizeBeetleSuccess(MediaDimension resizeBy, int size, ImageSaveFormat format)
        {
            ImageResizeProcessor processor = GetProcessor(resizeBy, size, format, false);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("beetle.jpg"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }

        private ImageResizeProcessor GetProcessor(MediaDimension resizeBy, int size, ImageSaveFormat format, bool overwrite)
        {
            ImageResizeProcessor processor = new ImageResizeProcessor();
            processor.SetParameter("NewPath", GetCurrentTestResultsFilePath("out" + GetExtension(format)));
            processor.SetParameter("Format", format);
            processor.SetParameter("ResizeBy", resizeBy);
            processor.SetParameter("Size", size);
            processor.SetParameter("Overwrite", overwrite);
            return processor;
        }

        private string GetExtension(ImageSaveFormat format)
        {
            switch (format)
            {
                case ImageSaveFormat.Bitmap:
                    return ".bmp";
                case ImageSaveFormat.Exif:
                    return ".exif";
                case ImageSaveFormat.Gif:
                    return ".gif";
                case ImageSaveFormat.Jpeg:
                    return ".jpg";
                case ImageSaveFormat.Png:
                    return ".png";
                case ImageSaveFormat.Tiff:
                    return ".tif";
            }
            return "";
        }
    }
}
