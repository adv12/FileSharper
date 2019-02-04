// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using FileSharperCore.Processors.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Text
{
    [TestClass]
    public class RemoveRepeatedLinesProcessorTests : TestBase
    {
        [TestMethod]
        public void Success()
        {
            Success(LineEndings.MatchInput, OutputEncodingType.MatchInput, "LipsumWithRepeatedLines.txt");
        }

        [TestMethod]
        public void SuccessClassicMacOS()
        {
            Success(LineEndings.ClassicMacOS, OutputEncodingType.MatchInput, "LipsumWithRepeatedLines.txt");
        }

        [TestMethod]
        public void SuccessUtf16()
        {
            Success(LineEndings.MatchInput, OutputEncodingType.UTF16_LE, "LipsumWithRepeatedLines.txt");
        }

        [TestMethod]
        public void SuccessNoTrailingNewline()
        {
            Success(LineEndings.MatchInput, OutputEncodingType.MatchInput, "LipsumWithRepeatedLinesAndNoTrailingNewline.txt");
        }

        [TestMethod]
        public void FailsFileExists()
        {
            string outputFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("LipsumWithRepeatedLines.txt");
            RemoveRepeatedLinesProcessor processor = new RemoveRepeatedLinesProcessor();
            processor.SetParameter("FileName", outputFilename);
            processor.SetParameter("OverwriteExistingFile", false);
            File.WriteAllBytes(outputFilename, new byte[0]);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void SucceedsFileExists()
        {
            string outputFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("LipsumWithRepeatedLines.txt");
            RemoveRepeatedLinesProcessor processor = new RemoveRepeatedLinesProcessor();
            processor.SetParameter("FileName", outputFilename);
            processor.SetParameter("OverwriteExistingFile", true);
            File.WriteAllBytes(outputFilename, new byte[0]);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }

        [TestMethod]
        public void FailsFileInUse()
        {
            string outputFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("LipsumWithRepeatedLines.txt");
            RemoveRepeatedLinesProcessor processor = new RemoveRepeatedLinesProcessor();
            processor.SetParameter("FileName", outputFilename);
            processor.SetParameter("OverwriteExistingFile", false);
            ProcessingResult result;
            using (FileStream fs = File.OpenWrite(outputFilename))
            {
                processor.Init(RunInfo);
                result = processor.Process(file, MatchResultType.Yes,
                    new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
                processor.Cleanup();
            }
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        private void Success(LineEndings lineEndings, OutputEncodingType outputEncoding, string inputFilename)
        {
            FileInfo file = GetTestFile(inputFilename);
            RemoveRepeatedLinesProcessor processor = new RemoveRepeatedLinesProcessor();
            processor.SetParameter("LineEndings", lineEndings);
            processor.SetParameter("OutputEncoding", outputEncoding);
            processor.SetParameter("FileName", GetCurrentTestResultsFilePath("out.txt"));
            processor.SetParameter("OverwriteExistingFile", false);
            processor.SetParameter("MoveOriginalToRecycleBin", false);
            
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }
    }
}
