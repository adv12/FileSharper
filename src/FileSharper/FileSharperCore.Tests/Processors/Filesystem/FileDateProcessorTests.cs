// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Processors.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Filesystem
{
    [TestClass]
    public class FileDateProcessorTests : TestBase
    {
        [TestMethod]
        public void SetsModificationDate()
        {
            SetsDate(FileDateType.Modified);
        }

        [TestMethod]
        public void SetsCreatedDate()
        {
            SetsDate(FileDateType.Created);
        }

        [TestMethod]
        public void SetsAccessedDate()
        {
            SetsDate(FileDateType.Accessed);
        }

        [TestMethod]
        public void FailsWhenFileLocked()
        {
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            FileInfo outputFile = new FileInfo(outputPath);
            DateTime date = new DateTime(2019, 1, 3);
            File.WriteAllText(outputPath, "This file will not have its date set.");
            FileDateProcessor processor = new FileDateProcessor();
            processor.SetParameter("FileDateType", FileDateType.Modified);
            processor.SetParameter("Date", date);
            ProcessingResult result;
            using (FileStream stream = File.OpenRead(outputPath))
            {
                processor.Init(RunInfo);
                result = processor.Process(outputFile,
                    MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                    CancellationToken.None);
                processor.Cleanup();
            }

            DateTime finalDate = File.GetLastWriteTime(outputPath);

            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreNotEqual(date, finalDate);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        private void SetsDate(FileDateType type)
        {
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            FileInfo outputFile = new FileInfo(outputPath);
            DateTime date = new DateTime(2019, 1, 3);
            File.WriteAllText(outputPath, "This file will have its date set.");
            FileDateProcessor processor = new FileDateProcessor();
            processor.SetParameter("FileDateType", type);
            processor.SetParameter("Date", date);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(outputFile,
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            DateTime finalDate;
            switch (type)
            {
                case FileDateType.Accessed:
                    finalDate = File.GetLastAccessTime(outputPath);
                    break;
                case FileDateType.Created:
                    finalDate = File.GetCreationTime(outputPath);
                    break;
                case FileDateType.Modified:
                    finalDate = File.GetLastWriteTime(outputPath);
                    break;
                default:
                    finalDate = DateTime.MinValue;
                    break;
            }
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(date, finalDate);
            Assert.AreEqual(1, result.OutputFiles.Length);
            Assert.AreEqual(outputFile, result.OutputFiles[0]);
        }

    }
}
