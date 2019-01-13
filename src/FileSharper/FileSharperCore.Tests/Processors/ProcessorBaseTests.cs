// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class ProcessorBaseTests : TestBase
    {
        [TestMethod]
        public void CopyAndDeleteTempFile_Success()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This is the output text.");
            processor.CopyAndDeleteTempFile(tmpFile, GetCurrentTestResultsFilePath("out.txt"), true, false);
            Assert.IsFalse(File.Exists(tmpFile));
        }

        [TestMethod]
        public void CopyAndDeleteTempFile_Overwrites()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This is the final text.");
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            // This should be overwritten.
            File.WriteAllText(outputPath, "This text will be overwritten.");
            processor.CopyAndDeleteTempFile(tmpFile, outputPath, true, false);
            Assert.IsFalse(File.Exists(tmpFile));
        }

        [TestMethod]
        public void CopyAndDeleteTempFile_DoesNotOverwrite()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This temp file will not overwrite the existing file.");
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            // This should not be overwritten.
            File.WriteAllText(outputPath, "This file will not be overwritten.");
            bool copied = processor.CopyAndDeleteTempFile(tmpFile, outputPath, false, false);
            Assert.IsFalse(File.Exists(tmpFile));
            Assert.IsFalse(copied);
        }

        [TestMethod]
        public void CopyAndDeleteTempFile_MovesToRecycleBin()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This is the final text.");
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            // This will be moved to the Recycle Bin.
            File.WriteAllText(outputPath, "This file will be moved to the Recycle Bin.");
            bool copied = processor.CopyAndDeleteTempFile(tmpFile, outputPath, false, true);
            Assert.IsFalse(File.Exists(tmpFile));
            Assert.IsTrue(copied);
        }

        [TestMethod]
        public void GetProcessingResultFromCopyAndDeleteTempFile_Successs()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This is the final text.");
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            ProcessingResult result = processor.GetProcessingResultFromCopyAndDeleteTempFile(
                null, outputPath, tmpFile, true, true);
            Assert.IsFalse(File.Exists(tmpFile));
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
        }

        [TestMethod]
        public void GetProcessingResultFromCopyAndDeleteTempFile_Failure()
        {
            Mock<ProcessorBase> mockProcessor = new Mock<ProcessorBase>();
            ProcessorBase processor = mockProcessor.Object;
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "This temp file will not overwrite the existing file.");
            string outputPath = GetCurrentTestResultsFilePath("out.txt");
            // This should not be overwritten.
            File.WriteAllText(outputPath, "This file will not be overwritten.");
            ProcessingResult result = processor.GetProcessingResultFromCopyAndDeleteTempFile(
                null, outputPath, tmpFile, false, false);
            Assert.IsFalse(File.Exists(tmpFile));
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
        }

    }
}
