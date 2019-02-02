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
    public class LineProcessorTests : TestBase
    {
        [TestMethod]
        public void Success()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.MatchInput, LineEndings.SystemDefault);
        }

        [TestMethod]
        public void SuccessMatchInput()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.MatchInput, LineEndings.MatchInput);
        }

        [TestMethod]
        public void SuccessWindows()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.MatchInput, LineEndings.Windows);
        }

        [TestMethod]
        public void SuccessClassicMacOS()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.MatchInput, LineEndings.ClassicMacOS);
        }

        [TestMethod]
        public void SuccessUnix()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.MatchInput, LineEndings.Unix);
        }

        [TestMethod]
        public void SuccessUtf16()
        {
            SuccessWithEncodingAndLineEndings(OutputEncodingType.UTF16_LE, LineEndings.SystemDefault);
        }

        private void SuccessWithEncodingAndLineEndings(OutputEncodingType encoding,
            LineEndings lineEndings)
        {
            string filename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("TextFileWithNewlines.txt");
            TestLineProcessor p = new TestLineProcessor(filename, lineEndings,
                encoding, overwriteExistingFile: true,
                moveOriginalToRecyclingBin: false);
            p.Init(RunInfo);
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            p.Cleanup();
        }

        [TestMethod]
        public void SuccessNoTrailingNewline()
        {
            string filename = GetCurrentTestResultsFilePath("out.txt");
            FileInfo file = GetTestFile("TextFileWithNoTrailingNewline.txt");
            TestLineProcessor p = new TestLineProcessor(filename, LineEndings.Windows,
                OutputEncodingType.MatchInput, overwriteExistingFile: true,
                moveOriginalToRecyclingBin: false);
            p.Init(RunInfo);
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            p.Cleanup();
        }

        [TestMethod]
        public void FailsExistingFile()
        {
            string filename = GetCurrentTestResultsFilePath("out.txt");
            File.WriteAllBytes(filename, new byte[0]);
            FileInfo file = GetTestFile("TextFileWithNewlines.txt");
            TestLineProcessor p = new TestLineProcessor(filename, LineEndings.Windows,
                OutputEncodingType.MatchInput, overwriteExistingFile: false,
                moveOriginalToRecyclingBin: false);
            p.Init(RunInfo);
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Failure, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
            p.Cleanup();
        }

        [TestMethod]
        public void SucceedsExistingFile()
        {
            string filename = GetCurrentTestResultsFilePath("out.txt");
            File.WriteAllBytes(filename, new byte[0]);
            FileInfo file = GetTestFile("TextFileWithNewlines.txt");
            TestLineProcessor p = new TestLineProcessor(filename, LineEndings.Windows,
                OutputEncodingType.MatchInput, overwriteExistingFile: true,
                moveOriginalToRecyclingBin: false);
            p.Init(RunInfo);
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0],
                new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(1, result.OutputFiles.Length);
            p.Cleanup();
        }

        [TestMethod]
        public void FailsFileInUse()
        {
            string filename = GetCurrentTestResultsFilePath("out.txt");
            using (FileStream fs = File.Create(filename))
            {
                FileInfo file = GetTestFile("TextFileWithNewlines.txt");
                TestLineProcessor p = new TestLineProcessor(filename, LineEndings.Windows,
                    OutputEncodingType.MatchInput, overwriteExistingFile: true,
                    moveOriginalToRecyclingBin: false);
                p.Init(RunInfo);
                ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0],
                    new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
                Assert.AreEqual(ProcessingResultType.Failure, result.Type);
                Assert.AreEqual(0, result.OutputFiles.Length);
                p.Cleanup();
            }
        }
    }

    public class TestLineProcessor : LineProcessor
    {
        private bool m_MoveOriginalToRecyclingBin;
        private LineEndings m_LineEndings;
        private OutputEncodingType m_OutputEncodingType;
        private string m_FileName;
        private bool m_OverwriteExistingFile;

        public override string Category => "Text";

        public override string Name => "TestLineProcessor";

        public override string Description => "Helps to test the LineProcessor class";

        public override object Parameters => null;

        protected override bool MoveOriginalToRecycleBin => m_MoveOriginalToRecyclingBin;

        protected override LineEndings LineEndings => m_LineEndings;

        protected override OutputEncodingType OutputEncodingType => m_OutputEncodingType;

        protected override string FileName => m_FileName;

        protected override bool OverwriteExistingFile => m_OverwriteExistingFile;

        public TestLineProcessor(string filename, LineEndings lineEndings,
            OutputEncodingType outputEncodingType, bool overwriteExistingFile,
            bool moveOriginalToRecyclingBin)
        {
            m_FileName = filename;
            m_LineEndings = lineEndings;
            m_OutputEncodingType = outputEncodingType;
            m_OverwriteExistingFile = overwriteExistingFile;
            m_MoveOriginalToRecyclingBin = moveOriginalToRecyclingBin;
        }

        protected override string TransformLine(string line)
        {
            return "The transformed line";
        }
    }
}
