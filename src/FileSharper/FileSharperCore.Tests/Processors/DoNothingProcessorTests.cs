// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class DoNothingProcessorTests: TestBase
    {
        [TestMethod]
        public void DoNothingProcessor_Succeeds()
        {
            DoNothingProcessor p = new DoNothingProcessor();
            p.Init(RunInfo);
            FileInfo file = GetTestFile("BasicTextFile.txt");
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0], new FileInfo[0],
                ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            p.Cleanup();
        }
    }
}
