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
    public class CreateOrUpdateFileProcessorTests : TestBase
    {
        [TestMethod]
        public void CreatesFile()
        {
            CreateOrUpdateFileProcessor processor = new CreateOrUpdateFileProcessor();
            processor.SetParameter("FileName", GetCurrentTestResultsFilePath("out.txt"));
            processor.SetParameter("UpdateModificationDateIfExists", false);
            processor.Init(RunInfo);
            // File is irrelevant
            FileInfo file = GetTestFile("BasicTextFile.txt");
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }

        [TestMethod]
        public void UpdatesDate()
        {
            CreateOrUpdateFileProcessor processor = new CreateOrUpdateFileProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            File.WriteAllText(outputPath, "This file will have its date updated.");
            File.SetLastWriteTime(outputPath, DateTime.Now.AddYears(-2));
            processor.SetParameter("FileName", outputPath);
            processor.SetParameter("UpdateModificationDateIfExists", true);
            processor.Init(RunInfo);
            // File is irrelevant
            FileInfo file = GetTestFile("BasicTextFile.txt");
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            Assert.AreEqual(DateTime.Now.Year, File.GetLastWriteTime(outputPath).Year);
        }

        [TestMethod]
        public void DoesNotUpdateDate()
        {
            CreateOrUpdateFileProcessor processor = new CreateOrUpdateFileProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            File.WriteAllText(outputPath, "This file will not have its date updated.");
            File.SetLastWriteTime(outputPath, DateTime.Now.AddYears(-2));
            processor.SetParameter("FileName", outputPath);
            processor.SetParameter("UpdateModificationDateIfExists", false);
            processor.Init(RunInfo);
            // File is irrelevant
            FileInfo file = GetTestFile("BasicTextFile.txt");
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
            Assert.AreNotEqual(DateTime.Now.Year, File.GetLastWriteTime(outputPath).Year);
        }
    }
}
