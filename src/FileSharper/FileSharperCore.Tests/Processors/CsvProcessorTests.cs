// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using CsvHelper;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class CsvProcessorTests : TestBase
    {
        [TestMethod]
        public void Windows()
        {
            TestSubcase("Windows.csv", PathFormat.DirectoryThenName, LineEndingsNoFile.Windows, false);
        }

        [TestMethod]
        public void Unix()
        {
            TestSubcase("Unix.csv", PathFormat.DirectoryThenName, LineEndingsNoFile.Unix, false);
        }

        [TestMethod]
        public void ClassicMacOS()
        {
            TestSubcase("ClassicMacOS.csv", PathFormat.DirectoryThenName, LineEndingsNoFile.ClassicMacOS, false);
        }

        [TestMethod]
        public void FullPath()
        {
            TestSubcase("FullPath.csv", PathFormat.FullPath, LineEndingsNoFile.Windows, false);
        }

        [TestMethod]
        public void NameThenDirectory()
        {
            TestSubcase("NameThenDirectory.csv", PathFormat.NameThenDirectory, LineEndingsNoFile.Windows, false);
        }

        public void TestSubcase(string outputFilename, PathFormat pathFormat,
            LineEndingsNoFile lineEndings, bool autoOpen)
        {
            Mock<ICondition> mockCondition = new Mock<ICondition>();
            Mock<IFieldSource> mockFieldSource = new Mock<IFieldSource>();
            Mock<IRunInfo> mockRunInfo = new Mock<IRunInfo>();
            mockCondition.Setup(c => c.ColumnCount).Returns(3);
            mockCondition.Setup(c => c.ColumnHeaders).Returns(new string[] { "foo", "bar", "baz" });
            mockFieldSource.Setup(f => f.ColumnCount).Returns(2);
            mockFieldSource.Setup(f => f.ColumnHeaders).Returns(new string[] { "hello", "world" });
            mockRunInfo.Setup(r => r.Condition).Returns(mockCondition.Object);
            mockRunInfo.Setup(r => r.FieldSources).Returns(new IFieldSource[] { mockFieldSource.Object });
            CsvProcessor csvProcessor = new CsvProcessor();
            csvProcessor.SetParameter("Filename", GetCurrentTestResultsFilePath(outputFilename));
            csvProcessor.SetParameter("PathFormat", pathFormat);
            csvProcessor.SetParameter("LineEndings", lineEndings);
            csvProcessor.SetParameter("AutoOpen", autoOpen);
            csvProcessor.Init(mockRunInfo.Object);
            FileInfo file1 = GetTestFile("BasicTextFile.txt");
            FileInfo file2 = GetTestFile("TextFileWithNewlines.txt");
            string[] vals1 = new string[] { "a", "b", "c", "d", "e" };
            string[] vals2 = new string[] { "q", "w", "e", "r", "t" };
            ProcessingResult result1;
            ProcessingResult result2;
            try
            {
                result1 = csvProcessor.Process(file1, MatchResultType.Yes, vals1,
                    new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
                result2 = csvProcessor.Process(file2, MatchResultType.No, vals2,
                    new FileInfo[0], ProcessInput.OriginalFile, CancellationToken.None);
            }
            finally
            {
                csvProcessor.Cleanup();
            }
            Assert.AreEqual(ProcessingResultType.Success, result1.Type);
            Assert.AreEqual(ProcessingResultType.Success, result2.Type);
        }

        public override void AssertFileEquality(FileInfo expected, FileInfo result)
        {
            using (CsvReader expectedReader = new CsvReader(new StreamReader(expected.FullName)))
            using (CsvReader resultReader = new CsvReader(new StreamReader(result.FullName)))
            {
                expectedReader.Read();
                expectedReader.ReadHeader();
                resultReader.Read();
                resultReader.ReadHeader();
                try
                {
                    int numCols = expectedReader.Context.HeaderRecord.Length;
                    int pathIndex = Array.IndexOf(expectedReader.Context.HeaderRecord, "Path");
                    if (pathIndex == -1)
                    {
                        pathIndex = Array.IndexOf(expectedReader.Context.HeaderRecord, "Filename");
                    }
                    Assert.AreEqual(numCols, resultReader.Context.HeaderRecord.Length);
                    while (expectedReader.Read())
                    {
                        Assert.IsTrue(resultReader.Read());

                        for (int i = 0; i < numCols; i++)
                        {
                            string expectedVal = expectedReader.GetField(i);
                            string resultVal = resultReader.GetField(i);
                            if (i == pathIndex)
                            {
                                int expectedIndex = expectedVal.LastIndexOf("TestFiles");
                                int resultIndex = resultVal.LastIndexOf("TestFiles");
                                Assert.AreNotEqual(-1, expectedIndex);
                                Assert.AreNotEqual(-1, resultIndex);
                                Assert.AreEqual(expectedVal.Substring(expectedIndex),
                                    resultVal.Substring(resultIndex));
                            }
                            else
                            {
                                Assert.AreEqual(expectedVal, resultVal);
                            }
                        }
                    }
                }
                catch (Exception ex) when (!(ex is AssertFailedException))
                {
                    Assert.Fail();
                }
            }
        }
    }
}
