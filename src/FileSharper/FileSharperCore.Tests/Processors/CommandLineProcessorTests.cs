using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
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
