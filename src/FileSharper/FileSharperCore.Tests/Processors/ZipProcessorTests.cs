// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors
{
    [TestClass]
    public class ZipProcessorTests : TestBase
    {
        [TestMethod]
        public void SingleFiles()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.InputFile);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "{NameWithoutExtension}.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void SingleFiles_AddContainingFolder()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.InputFile);
            processor.SetParameter("AddContainingFolder", true);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "{NameWithoutExtension}.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void OneZipPerSearch()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.Search);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "output.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void OneZipPerSearch_AddContainingFolder()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.Search);
            processor.SetParameter("AddContainingFolder", true);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "output.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void OneZipPerSearch_Overwrites()
        {
            ZipProcessor processor = new ZipProcessor();
            string outputPath = Path.Combine(CurrentTestResultsDirectoryPath, "output.zip");
            processor.SetParameter("OneZipFilePer", ProcessorScope.Search);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", outputPath);
            processor.SetParameter("Overwrite", true);
            File.WriteAllText(outputPath, "This file will be overwritten.");
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void OneZipPerSearch_DoesNotOverwrite()
        {
            ZipProcessor processor = new ZipProcessor();
            string outputPath = Path.Combine(CurrentTestResultsDirectoryPath, "output.zip");
            processor.SetParameter("OneZipFilePer", ProcessorScope.Search);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", outputPath);
            processor.SetParameter("Overwrite", false);
            File.WriteAllText(outputPath, "This file will not be replaced.  Also it's not really a zip file.");
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[0];
            string[] values = new string[0];

            processor.Init(RunInfo);
            processor.Process(file1, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file2, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            processor.Process(file3, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.OriginalFile, CancellationToken.None);
            try
            {
                processor.ProcessAggregated(CancellationToken.None);
                Assert.Fail();
            }
            catch (Exception)
            {

            }
            processor.Cleanup();
        }

        [TestMethod]
        public void GeneratedFiles_PerInputFile()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.InputFile);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "{NameWithoutExtension}.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file = GetTestFile("BasicTextFile.txt");
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[] { file1, file2, file3 };
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.GeneratedFiles, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }

        [TestMethod]
        public void GeneratedFiles_PerGeneratedFile()
        {
            ZipProcessor processor = new ZipProcessor();
            processor.SetParameter("OneZipFilePer", ProcessorScope.GeneratedOutputFile);
            processor.SetParameter("AddContainingFolder", false);
            processor.SetParameter("OutputPath", Path.Combine(CurrentTestResultsDirectoryPath,
                "{NameWithoutExtension}.zip"));
            processor.SetParameter("Overwrite", false);
            FileInfo file = GetTestFile("BasicTextFile.txt");
            FileInfo file1 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "TextFile1ForZip.txt"));
            FileInfo file2 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile2ForZip.txt"));
            FileInfo file3 = GetTestFile(Path.Combine("ZipHierarchy", "Subdir1", "Subdir2", "Subdir3", "TextFile3ForZip.txt"));
            FileInfo[] generatedFiles = new FileInfo[] { file1, file2, file3 };
            string[] values = new string[0];
            processor.Init(RunInfo);
            processor.Process(file, MatchResultType.Yes, values, generatedFiles,
                ProcessInput.GeneratedFiles, CancellationToken.None);
            processor.ProcessAggregated(CancellationToken.None);
            processor.Cleanup();
        }
    }
}
