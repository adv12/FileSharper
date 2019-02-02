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
    public class FilterLinesProcessorTests : TestBase
    {
        [TestMethod]
        public void SuccessKeepNoRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "et", useRegex: false, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessKeepRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "et.*,", useRegex: true, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessRemoveNoRegex()
        {
            Success(LineFilterType.RemoveMatchingLines, "et", useRegex: false, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessRemoveRegex()
        {
            Success(LineFilterType.RemoveMatchingLines, "et.*,", useRegex: true, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessCaseInsensitiveNoRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "vivamus", useRegex: false, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessCaseInsensitiveRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "^vivamus", useRegex: true, caseSensitive: false);
        }

        [TestMethod]
        public void SuccessCaseSensitiveNoRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "vivamus", useRegex: false, caseSensitive: true);
        }

        [TestMethod]
        public void SuccessCaseSensitiveRegex()
        {
            Success(LineFilterType.KeepMatchingLines, "^vivamus", useRegex: true, caseSensitive: true);
        }

        private void Success(LineFilterType filterType, string textToMatch, bool useRegex, bool caseSensitive)
        {
            FilterLinesProcessor processor = new FilterLinesProcessor();
            processor.SetParameter("FilterType", filterType);
            processor.SetParameter("TextToMatch", textToMatch);
            processor.SetParameter("UseRegex", useRegex);
            processor.SetParameter("CaseSensitive", caseSensitive);
            processor.SetParameter("LineEndings", LineEndings.MatchInput);
            processor.SetParameter("OutputEncoding", OutputEncodingType.MatchInput);
            processor.SetParameter("FileName", GetCurrentTestResultsFilePath("out.txt"));
            processor.SetParameter("OverwriteExistingFile", true);
            processor.SetParameter("MoveOriginalToRecycleBin", false);
            FileInfo file = GetTestFile("Lipsum.txt");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            processor.Cleanup();
        }

        [TestMethod]
        public void SuccessNoTrailingNewline()
        {
            FilterLinesProcessor processor = new FilterLinesProcessor();
            processor.SetParameter("FilterType", LineFilterType.KeepMatchingLines);
            processor.SetParameter("TextToMatch", "il");
            processor.SetParameter("UseRegex", false);
            processor.SetParameter("CaseSensitive", false);
            processor.SetParameter("LineEndings", LineEndings.MatchInput);
            processor.SetParameter("OutputEncoding", OutputEncodingType.MatchInput);
            processor.SetParameter("FileName", GetCurrentTestResultsFilePath("out.txt"));
            processor.SetParameter("OverwriteExistingFile", true);
            processor.SetParameter("MoveOriginalToRecycleBin", false);
            FileInfo file = GetTestFile("TextFileWithNoTrailingNewline.txt");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            processor.Cleanup();
        }

        [TestMethod]
        public void FailsFileExists()
        {
            string outputFilename = GetCurrentTestResultsFilePath("out.txt");
            FilterLinesProcessor processor = new FilterLinesProcessor();
            processor.SetParameter("OverwriteExistingFile", false);
            processor.SetParameter("FileName", outputFilename);
            processor.SetParameter("TextToMatch", "foo");
            File.WriteAllBytes(outputFilename, new byte[0]);
            FileInfo file = GetTestFile("Lipsum.txt");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }

        [TestMethod]
        public void FailsFileInUse()
        {
            string outputFilename = GetCurrentTestResultsFilePath("out.txt");
            FilterLinesProcessor processor = new FilterLinesProcessor();
            processor.SetParameter("OverwriteExistingFile", true);
            processor.SetParameter("FileName", outputFilename);
            processor.SetParameter("TextToMatch", "foo");
            using (FileStream fs = File.OpenWrite(outputFilename))
            {
                FileInfo file = GetTestFile("Lipsum.txt");
                processor.Init(RunInfo);
                ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                    new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                    CancellationToken.None);
                processor.Cleanup();
                Assert.AreEqual(ProcessingResultType.Failure, result.Type);
                Assert.AreEqual(0, result.OutputFiles.Length);
            }
        }

    }
}
