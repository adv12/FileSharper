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
    public class PrependOrAppendTextProcessorTests : TestBase
    {
        [TestMethod]
        public void PrependSuccess()
        {
            Success(PrependAppend.Prepend, "foo bar baz", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void PrependOneNewlineSuccess()
        {
            Success(PrependAppend.Prepend, "foo bar baz\r\n", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void PrependTwoNewlineSuccess()
        {
            Success(PrependAppend.Prepend, "foo bar baz\r\n\r\n", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void AppendSuccess()
        {
            Success(PrependAppend.Append, "foo bar baz", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void AppendOneNewlineSuccess()
        {
            Success(PrependAppend.Append, "foo bar baz\r\n", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void AppendTwoNewlineSuccess()
        {
            Success(PrependAppend.Append, "foo bar baz\r\n\r\n", LineEndings.MatchInput,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void PrependClassicMacOSSuccess()
        {
            Success(PrependAppend.Prepend, "foo bar baz", LineEndings.ClassicMacOS,
                OutputEncodingType.MatchInput);
        }

        [TestMethod]
        public void PrependUtf16Success()
        {
            Success(PrependAppend.Prepend, "foo bar baz", LineEndings.MatchInput,
                OutputEncodingType.UTF16_LE);
        }

        [TestMethod]
        public void FailsFileExists()
        {
            string outFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("Lipsum.txt");
            PrependOrAppendTextProcessor processor = new PrependOrAppendTextProcessor();
            processor.SetParameter("Text", "foo bar baz");
            processor.SetParameter("FileName", outFilename);
            processor.SetParameter("OverwriteExistingFile", false);
            File.WriteAllBytes(outFilename, new byte[0]);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void SucceedsFileExists()
        {
            string outFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("Lipsum.txt");
            PrependOrAppendTextProcessor processor = new PrependOrAppendTextProcessor();
            processor.SetParameter("Text", "foo bar baz");
            processor.SetParameter("FileName", outFilename);
            processor.SetParameter("OverwriteExistingFile", true);
            File.WriteAllBytes(outFilename, new byte[0]);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }

        [TestMethod]
        public void FailsFileInUse()
        {
            string outFilename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("Lipsum.txt");
            PrependOrAppendTextProcessor processor = new PrependOrAppendTextProcessor();
            processor.SetParameter("Text", "foo bar baz");
            processor.SetParameter("FileName", outFilename);
            processor.SetParameter("OverwriteExistingFile", true);
            ProcessingResult result;
            using (FileStream fs = File.OpenWrite(outFilename))
            {
                processor.Init(RunInfo);
                result = processor.Process(file, MatchResultType.Yes, new string[0],
                    new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
                processor.Cleanup();
            }
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        private void Success(PrependAppend prependAppend, string text, LineEndings lineEndings,
            OutputEncodingType outputEncoding)
        {
            FileInfo file = GetTestFile("Lipsum.txt");
            PrependOrAppendTextProcessor processor = new PrependOrAppendTextProcessor();
            processor.SetParameter("PrependOrAppend", prependAppend);
            processor.SetParameter("Text", text);
            processor.SetParameter("LineEndings", lineEndings);
            processor.SetParameter("OutputEncoding", outputEncoding);
            processor.SetParameter("FileName", GetCurrentTestResultsFilePath("out.txt"));
            processor.SetParameter("OverwriteExistingFile", false);
            processor.SetParameter("MoveOriginalToRecycleBin", false);
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
        }
    }
}
