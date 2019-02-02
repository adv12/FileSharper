// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using FileSharperCore.Processors.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Text
{
    [TestClass]
    public class ChangeLineEndingsProcessorTests : TestBase
    {
        [TestMethod]
        public void ExposesParameters()
        {
            // The use of these properties is tested in LineProcessorTests.
            // All we need to do here is verify that the parameters are
            // being exposed properly.

            ChangeLineEndingsProcessor processor = new ChangeLineEndingsProcessor();

            processor.SetParameter("MoveOriginalToRecycleBin", true);
            Assert.AreEqual(true, processor.MoveOriginalToRecycleBin);

            processor.SetParameter("LineEndings", LineEndingsNoFile.ClassicMacOS);
            Assert.AreEqual(LineEndings.ClassicMacOS, processor.LineEndings);

            processor.SetParameter("OutputEncoding", OutputEncodingType.UTF16_LE);
            Assert.AreEqual(OutputEncodingType.UTF16_LE, processor.OutputEncodingType);

            processor.SetParameter("FileName", @"C:\foo.txt");
            Assert.AreEqual(@"C:\foo.txt", processor.FileName);

            processor.SetParameter("OverwriteExistingFile", true);
            Assert.AreEqual(true, processor.OverwriteExistingFile);
        }

        [TestMethod]
        public void TransformLineSuccess()
        {
            ChangeLineEndingsProcessor processor = new ChangeLineEndingsProcessor();
            processor.SetParameter("LineEndings", LineEndingsNoFile.ClassicMacOS);

            processor.Init(RunInfo);

            string line = "The quick brown fox jumps over the lazy dog.";
            string transformedLine = processor.TransformLine(line);
            Assert.AreEqual(line, transformedLine);

            processor.Cleanup();
        }
    }
}
