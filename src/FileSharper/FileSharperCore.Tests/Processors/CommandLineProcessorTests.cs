// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

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
            p.Process(file, new string[0], CancellationToken.None);
            p.Cleanup();
        }

    }
}
