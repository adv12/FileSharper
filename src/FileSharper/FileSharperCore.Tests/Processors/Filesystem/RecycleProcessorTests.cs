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
    public class RecycleProcessorTests : TestBase
    {
        [TestMethod]
        public void RecyclesFile()
        {
            RecycleProcessor processor = new RecycleProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            FileInfo outputFile = new FileInfo(outputPath);
            File.WriteAllText(outputPath, "This file will be moved to the Recycle Bin.");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(outputFile, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.IsFalse(File.Exists(outputPath));
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void FailsWhenFileInUse()
        {
            RecycleProcessor processor = new RecycleProcessor();
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            FileInfo outputFile = new FileInfo(outputPath);
            File.WriteAllText(outputPath, "This file will not be moved to the Recycle Bin.");
            ProcessingResult result;
            using (FileStream stream = File.OpenRead(outputPath))
            {
                processor.Init(RunInfo);
                result = processor.Process(outputFile, MatchResultType.Yes, new string[0],
                    new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
                processor.Cleanup();
            }
            Assert.IsTrue(File.Exists(outputPath));
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }
    }
}
