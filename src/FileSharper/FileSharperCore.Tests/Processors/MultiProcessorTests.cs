// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class MultiProcessorTests : TestBase
    {
        [TestMethod]
        public void MultiProcessor_AllSuccess()
        {
            Mock<IProcessor> mockProcessor1 = new Mock<IProcessor>();
            Mock<IProcessor> mockProcessor2 = new Mock<IProcessor>();
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("Lipsum.txt");
            FileInfo file3 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            string[] values = new string[] { "foo", "bar", "baz" };
            
            mockProcessor1.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor1.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor1.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 1 Success", outputFiles));
            mockProcessor1.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor1.Setup(p => p.Cleanup());
            
            mockProcessor2.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor2.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor2.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 2 Success", outputFiles));
            mockProcessor2.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor2.Setup(p => p.Cleanup());

            MultiProcessor multiProcessor = new MultiProcessor();
            multiProcessor.Processors.Add(mockProcessor1.Object);
            multiProcessor.Processors.Add(mockProcessor2.Object);

            multiProcessor.Init(RunInfo);
            ProcessingResult result = multiProcessor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            multiProcessor.ProcessAggregated(CancellationToken.None);
            multiProcessor.Cleanup();

            mockProcessor1.VerifyAll();
            mockProcessor2.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual("Processor 1 Success | Processor 2 Success", result.Message);
            Assert.AreEqual(6, result.OutputFiles.Length);
            Assert.IsTrue(result.OutputFiles.All(file => file == file3));
        }

        [TestMethod]
        public void MultiProcessor_SingleFailure()
        {
            Mock<IProcessor> mockProcessor1 = new Mock<IProcessor>();
            Mock<IProcessor> mockProcessor2 = new Mock<IProcessor>();
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("Lipsum.txt");
            FileInfo file3 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            string[] values = new string[] { "foo", "bar", "baz" };

            mockProcessor1.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor1.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor1.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 1 Success", outputFiles));
            mockProcessor1.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor1.Setup(p => p.Cleanup());

            mockProcessor2.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor2.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor2.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Failure,
                "Processor 2 Failure", new FileInfo[] { }));
            mockProcessor2.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor2.Setup(p => p.Cleanup());

            MultiProcessor multiProcessor = new MultiProcessor();
            multiProcessor.Processors.Add(mockProcessor1.Object);
            multiProcessor.Processors.Add(mockProcessor2.Object);

            multiProcessor.Init(RunInfo);
            ProcessingResult result = multiProcessor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            multiProcessor.ProcessAggregated(CancellationToken.None);
            multiProcessor.Cleanup();

            mockProcessor1.VerifyAll();
            mockProcessor2.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Processor 1 Success | Processor 2 Failure", result.Message);
            Assert.AreEqual(3, result.OutputFiles.Length);
            Assert.IsTrue(result.OutputFiles.All(file => file == file3));
        }

        [TestMethod]
        public void MultiProcessor_SingleException()
        {
            Mock<IProcessor> mockProcessor1 = new Mock<IProcessor>();
            Mock<IProcessor> mockProcessor2 = new Mock<IProcessor>();
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("Lipsum.txt");
            FileInfo file3 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            string[] values = new string[] { "foo", "bar", "baz" };

            mockProcessor1.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor1.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor1.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 1 Success", outputFiles));
            mockProcessor1.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor1.Setup(p => p.Cleanup());

            mockProcessor2.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor2.Setup(p => p.InputFileSource).Returns(InputFileSource.ParentInput);
            mockProcessor2.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.GeneratedFiles, It.IsAny<CancellationToken>()))
                .Throws(new Exception("Some problem occurred!"));
            mockProcessor2.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor2.Setup(p => p.Cleanup());

            MultiProcessor multiProcessor = new MultiProcessor();
            multiProcessor.Processors.Add(mockProcessor1.Object);
            multiProcessor.Processors.Add(mockProcessor2.Object);

            multiProcessor.Init(RunInfo);
            ProcessingResult result = multiProcessor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            multiProcessor.ProcessAggregated(CancellationToken.None);
            multiProcessor.Cleanup();

            mockProcessor1.VerifyAll();
            mockProcessor2.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual("Processor 1 Success | Some problem occurred!", result.Message);
            Assert.AreEqual(3, result.OutputFiles.Length);
            Assert.IsTrue(result.OutputFiles.All(file => file == file3));
        }

        [TestMethod]
        public void MultiProcessor_OriginalFile()
        {
            Mock<IProcessor> mockProcessor1 = new Mock<IProcessor>();
            Mock<IProcessor> mockProcessor2 = new Mock<IProcessor>();
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("Lipsum.txt");
            FileInfo file3 = GetTestFile("TextFileWithNewlines.txt");
            FileInfo[] generatedFiles = new FileInfo[] { file2, file2, file2 };
            FileInfo[] outputFiles = new FileInfo[] { file3, file3, file3 };
            string[] values = new string[] { "foo", "bar", "baz" };

            mockProcessor1.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor1.Setup(p => p.InputFileSource).Returns(InputFileSource.OriginalFile);
            mockProcessor1.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.OriginalFile, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 1 Success", outputFiles));
            mockProcessor1.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor1.Setup(p => p.Cleanup());

            mockProcessor2.Setup(p => p.Init(It.IsAny<IRunInfo>()));
            mockProcessor2.Setup(p => p.InputFileSource).Returns(InputFileSource.OriginalFile);
            mockProcessor2.Setup(p => p.Process(file1, It.IsAny<MatchResultType>(),
                values, generatedFiles, ProcessInput.OriginalFile, It.IsAny<CancellationToken>()))
                .Returns(new ProcessingResult(ProcessingResultType.Success,
                "Processor 2 Success", outputFiles));
            mockProcessor2.Setup(p => p.ProcessAggregated(It.IsAny<CancellationToken>()));
            mockProcessor2.Setup(p => p.Cleanup());

            MultiProcessor multiProcessor = new MultiProcessor();
            multiProcessor.Processors.Add(mockProcessor1.Object);
            multiProcessor.Processors.Add(mockProcessor2.Object);

            multiProcessor.Init(RunInfo);
            ProcessingResult result = multiProcessor.Process(file1, MatchResultType.Yes,
                values, generatedFiles, ProcessInput.GeneratedFiles, CancellationToken.None);
            multiProcessor.ProcessAggregated(CancellationToken.None);
            multiProcessor.Cleanup();

            mockProcessor1.VerifyAll();
            mockProcessor2.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual("Processor 1 Success | Processor 2 Success", result.Message);
            Assert.AreEqual(6, result.OutputFiles.Length);
            Assert.IsTrue(result.OutputFiles.All(file => file == file3));
        }

    }
}
