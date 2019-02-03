// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileSharperCore.Processors.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Filesystem
{
    [TestClass]
    public class CreateDirectoryProcessorTests : TestBase
    {
        [TestMethod]
        public void Success()
        {
            CreateDirectoryProcessor processor = new CreateDirectoryProcessor();
            processor.SetParameter("DirectoryPath", GetCurrentTestResultsFilePath("Subdir"));
            FileInfo file = GetTestFile("BasicTextFile.txt");
            processor.Init(RunInfo);
            ProcessingResult result = processor.Process(file, MatchResultType.Yes,
                new string[0], new FileInfo[0], ProcessInput.OriginalFile,
                CancellationToken.None);
            processor.Cleanup();
            Assert.AreEqual(ProcessingResultType.Success, result.Type);
            Assert.AreEqual(0, result.OutputFiles.Length);
        }
    }
}
