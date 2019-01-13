// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Diagnostics;
using System.IO;
using System.Threading;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class CommandLineProcessorTests: TestBase
    {

        [TestMethod]
        public void RunsCommandLine()
        {
            CommandLineProcessor p = new CommandLineProcessor();
            p.SetParameter("CommandLine", $"copy \"{{FullName}}\" \"{CurrentTestResultsDirectoryPath}\"");
            p.SetParameter("WaitForTaskToFinish", true);
            p.Init(RunInfo);
            FileInfo file = GetTestFile("BasicTextFile.txt");
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0], new FileInfo[0],
                ProcessInput.OriginalFile, CancellationToken.None);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            p.Cleanup();
        }

        [TestMethod]
        public void WaitsOrNot()
        {
            CommandLineProcessor p = new CommandLineProcessor();
            // File isn't used; just a dummy.
            FileInfo file = GetTestFile("BasicTextFile.txt");
            // ping command waits 1s between pings, so telling it to ping 3 times
            // guarantees the command will take at least 2s to run.  This is all
            // because there's no sleep command and this is what folks on
            // StackOverflow recommend.
            p.SetParameter("CommandLine", "ping -n 3 127.0.0.1");
            p.Init(RunInfo);
            Stopwatch sw = new Stopwatch();
            // Wait for this run to finish...
            p.SetParameter("WaitForTaskToFinish", true);
            sw.Start();
            ProcessingResult result = p.Process(file, MatchResultType.Yes, new string[0], new FileInfo[0],
                ProcessInput.OriginalFile, CancellationToken.None);
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds >= 2000);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            // Don't wait for this run to finish...
            p.SetParameter("WaitForTaskToFinish", false);
            sw.Restart();
            result = p.Process(file, MatchResultType.Yes, new string[0], new FileInfo[0],
                ProcessInput.OriginalFile, CancellationToken.None);
            sw.Stop();
            // Should have gotten started in less than a second, right?
            Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            p.Cleanup();
        }

    }
}
