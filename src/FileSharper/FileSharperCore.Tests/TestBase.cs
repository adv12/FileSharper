// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        public string AssemblyDirectoryPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public DirectoryInfo AssemblyDirectory => new DirectoryInfo(AssemblyDirectoryPath);

        public string TestFilesDirectoryPath => Path.GetFullPath(Path.Combine(AssemblyDirectoryPath, "..", "..", "TestFiles"));

        public DirectoryInfo TestFilesDirectory => new DirectoryInfo(TestFilesDirectoryPath);

        public string ExpectedResultsDirectoryPath => Path.GetFullPath(Path.Combine(AssemblyDirectoryPath, "..", "..", "ExpectedResults"));

        public DirectoryInfo ExpectedResultsDirectory => new DirectoryInfo(ExpectedResultsDirectoryPath);

        public string CurrentTestExpectedResultsDirectoryPath => Path.Combine(ExpectedResultsDirectoryPath, this.GetType().Name, TestContext.TestName);

        public DirectoryInfo CurrentTestExpectedResultsDirectory => new DirectoryInfo(CurrentTestExpectedResultsDirectoryPath);

        public string ResultsDirectoryPath => Path.Combine(AssemblyDirectoryPath, "Results");

        public DirectoryInfo ResultsDirectory => new DirectoryInfo(ResultsDirectoryPath);

        public string CurrentTestResultsDirectoryPath => Path.Combine(ResultsDirectoryPath, this.GetType().Name, TestContext.TestName);

        public DirectoryInfo CurrentTestResultsDirectory => new DirectoryInfo(CurrentTestResultsDirectoryPath);

        public RunInfo RunInfo
        {
            get
            {
                RunInfo info = new RunInfo(null, null, null, null, null, 0, CancellationToken.None,
                    null, null, null, null, null);
                return info;
            }
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            DirectoryInfo myDirectory = Directory.CreateDirectory(CurrentTestResultsDirectoryPath);
            ClearDirectory(myDirectory);
        }

        public void ClearDirectory(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                ClearDirectory(subdirectory);
                subdirectory.Delete();
            }
        }

        public void AssertResultEquality()
        {
            AssertDirectoryEquality(CurrentTestExpectedResultsDirectory, CurrentTestResultsDirectory);
        }

        public void AssertDirectoryEquality(DirectoryInfo expected, DirectoryInfo result)
        {
            if (expected.Exists || (result.Exists && DirectoryHasFiles(result)))
            {
                Assert.IsTrue(result.Exists, $"Missing matching results directory {result.FullName} for expected directory {expected.FullName}");
                FileInfo[] expectedFiles = expected.GetFiles();
                FileInfo[] resultFiles = result.GetFiles();
                Assert.AreEqual(expectedFiles.Length, resultFiles.Length, $"Expected {expectedFiles.Length} files but found {resultFiles.Length} in {result.FullName}");
                foreach (FileInfo expectedFile in expected.GetFiles())
                {
                    FileInfo resultFile = resultFiles.Where(x => x.Name == expectedFile.Name).FirstOrDefault();
                    Assert.IsNotNull(resultFile, $"Couldn't find a file named {expectedFile.Name} in {result.FullName}");
                    AssertFileEquality(expectedFile, resultFile);
                }
                DirectoryInfo[] expectedDirectories = expected.GetDirectories();
                DirectoryInfo[] resultDirectories = result.GetDirectories();
                Assert.AreEqual(expectedDirectories.Length, resultDirectories.Length, $"Expected {expectedFiles.Length} directories but found {resultFiles.Length} in {result.FullName}");
                foreach (DirectoryInfo expectedDirectory in expectedDirectories)
                {
                    DirectoryInfo resultDirectory = resultDirectories.Where(x => x.Name == expectedDirectory.Name).FirstOrDefault();
                    Assert.IsNotNull(resultDirectory, $"Couldn't find a file named {expectedDirectory.Name} in {result.FullName}");
                    AssertDirectoryEquality(expectedDirectory, resultDirectory);
                }
            }
        }

        public bool DirectoryHasFiles(DirectoryInfo dir)
        {
            return Directory.EnumerateFileSystemEntries(dir.FullName).Any();
        }

        public void AssertFileEquality(FileInfo expected, FileInfo result)
        {
            Assert.AreEqual(expected.Length, result.Length, $"Expected and result file have different lengths.  Expected: {expected.FullName}; Result: {result.FullName}");
            using (FileStream expectedStream = expected.OpenRead())
            using (FileStream resultStream = result.OpenRead())
            {
                int expectedVal = 0;
                int resultVal = 0;
                while (expectedVal != -1)
                {
                    expectedVal = expectedStream.ReadByte();
                    resultVal = resultStream.ReadByte();
                    Assert.AreEqual(expectedVal, resultVal);
                }
            }
        }

        public string GetTestFilePath(string relativePath)
        {
            return Path.Combine(TestFilesDirectoryPath, relativePath);
        }

        public FileInfo GetTestFile(string relativePath)
        {
            return new FileInfo(GetTestFilePath(relativePath));
        }

        public string GetCurrentTestExpectedResultsFilePath(string relativePath)
        {
            return Path.Combine(CurrentTestExpectedResultsDirectoryPath, relativePath);
        }

        public FileInfo GetCurrentTestExpectedResultsFile(string relativePath)
        {
            return new FileInfo(GetCurrentTestExpectedResultsFilePath(relativePath));
        }

        public string GetCurrentTestResultsFilePath(string relativePath)
        {
            return Path.Combine(CurrentTestResultsDirectoryPath, relativePath);
        }

        public FileInfo GetCurrentTestResultsFile(string relativePath)
        {
            return new FileInfo(GetCurrentTestResultsFilePath(relativePath));
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            AssertResultEquality();
        }

    }
}
