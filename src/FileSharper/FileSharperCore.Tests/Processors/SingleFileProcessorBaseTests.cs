// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class SingleFileProcessorBaseTests : TestBase
    {
        [TestMethod]
        public void OriginalFile_Success()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] outputFiles = new FileInfo[] { file2, file2, file2 };
            mockProcessor.Setup(p => p.Process(file1, values, CancellationToken.None))
                .Returns(new ProcessingResult(ProcessingResultType.Success, "Success!", outputFiles));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual("Success!", result.Message);
            Assert.AreEqual(outputFiles.Length, result.OutputFiles.Length);
            for (int i = 0; i < outputFiles.Length; i++)
            {
                Assert.AreEqual(outputFiles[i], result.OutputFiles[i]);
            }
        }

        [TestMethod]
        public void OriginalFile_NoOutputFiles()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] outputFiles = new FileInfo[] { file2, file2, file2 };
            mockProcessor.Setup(p => p.Process(file1, values, CancellationToken.None))
                .Returns(new ProcessingResult(ProcessingResultType.Success, "Success!", new FileInfo[0]));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual("Success!", result.Message);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void OriginalFile_Failure()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] outputFiles = new FileInfo[] { file2, file2, file2 };
            mockProcessor.Setup(p => p.Process(file1, values, CancellationToken.None))
                .Returns(new ProcessingResult(ProcessingResultType.Failure, "Failure!", outputFiles));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Failure!", result.Message);
            Assert.AreEqual(outputFiles.Length, result.OutputFiles.Length);
            for (int i = 0; i < outputFiles.Length; i++)
            {
                Assert.AreEqual(outputFiles[i], result.OutputFiles[i]);
            }
        }

        [TestMethod]
        public void OriginalFile_Exception()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] outputFiles = new FileInfo[] { file2, file2, file2 };
            mockProcessor.Setup(p => p.Process(file1, values, CancellationToken.None))
                .Throws(new Exception("Something went wrong in Process()!"));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Something went wrong in Process()!", result.Message);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void GeneratedFiles_Success()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo file3 = GetTestFile("Lipsum.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            mockProcessor.Setup(p => p.Process(file2, values, CancellationToken.None))
                .Returns(new ProcessingResult(ProcessingResultType.Success, "Success!", outputFiles));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual("Success! | Success! | Success!", result.Message);
            Assert.AreEqual(9, result.OutputFiles.Length);
            for (int i = 1; i < 9; i++)
            {
                Assert.AreEqual(file3, result.OutputFiles[i]);
            }
        }

        [TestMethod]
        public void GeneratedFiles_Failure()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo file3 = GetTestFile("Lipsum.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            mockProcessor.Setup(p => p.Process(file2, values, CancellationToken.None))
                .Returns(new ProcessingResult(ProcessingResultType.Failure, "Failure!", outputFiles));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Failure! | Failure! | Failure!", result.Message);
            Assert.AreEqual(9, result.OutputFiles.Length);
            for (int i = 1; i < 9; i++)
            {
                Assert.AreEqual(file3, result.OutputFiles[i]);
            }
        }

        [TestMethod]
        public void GeneratedFiles_Exception()
        {
            Mock<SingleFileProcessorBase> mockProcessor = new Mock<SingleFileProcessorBase>();
            mockProcessor.CallBase = true;
            string[] values = new string[] { "foo", "bar", "baz" };
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo file3 = GetTestFile("Lipsum.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            mockProcessor.Setup(p => p.Process(file2, values, CancellationToken.None))
                .Throws(new Exception("Something went wrong in Process()!"));
            SingleFileProcessorBase processor = mockProcessor.Object;
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Something went wrong in Process()! | " + 
                "Something went wrong in Process()! | " + 
                "Something went wrong in Process()!", result.Message);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }
    }
}
