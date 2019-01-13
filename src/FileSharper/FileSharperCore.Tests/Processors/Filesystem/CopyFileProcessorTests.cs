// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using FileSharperCore.Processors.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Filesystem
{
    [TestClass]
    public class CopyFileProcessorTests : TestBase
    {
        [TestMethod]
        public void Success()
        {
            CopyFileProcessor processor = new CopyFileProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            processor.SetParameter("NewPath", outputPath);
            processor.SetParameter("Overwrite", false);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("BasicTextFile.txt"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            Assert.AreEqual(outputPath, result.OutputFiles[0].FullName);
        }

        [TestMethod]
        public void Overwrites()
        {
            CopyFileProcessor processor = new CopyFileProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            processor.SetParameter("NewPath", outputPath);
            processor.SetParameter("Overwrite", true);
            File.WriteAllText(outputPath, "This file will be overwritten.");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("BasicTextFile.txt"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            Assert.AreEqual(outputPath, result.OutputFiles[0].FullName);
        }

        [TestMethod]
        public void DoesNotOverwrite()
        {
            CopyFileProcessor processor = new CopyFileProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            processor.SetParameter("NewPath", outputPath);
            processor.SetParameter("Overwrite", false);
            File.WriteAllText(outputPath, "This file will not be overwritten.");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(GetTestFile("BasicTextFile.txt"),
                MatchResultType.Yes, new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }
    }
}
