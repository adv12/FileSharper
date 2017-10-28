// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors
{

    public class ZipParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public ProcessorScope OneZipFilePer { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool AddContainingFolder { get; set; } = true;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string OutputPath { get; set; } = @"{Desktop}\{Name}";
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool Overwrite { get; set; } = false;
    }

    public class ZipProcessor : ProcessorBase
    {
        private ZipParameters m_Parameters = new ZipParameters();
        private List<FileInfo> m_Files = new List<FileInfo>();

        public override string Name => "Zip file(s)";

        public override string Description => "Creates a zip file per search, per input file, or per previous output file depending on the settings.";

        public override object Parameters => m_Parameters;

        public override HowOften ProducesFiles => HowOften.Sometimes;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            if (m_Parameters.OneZipFilePer == ProcessorScope.Search)
            {
                m_Files.Clear();
            }
        }

        public override ProcessingResult Process(FileInfo file, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess, CancellationToken token)
        {
            ProcessorScope scope = m_Parameters.OneZipFilePer;
            bool perInput = scope == ProcessorScope.InputFile;
            bool perPreviousOutput = scope == ProcessorScope.GeneratedOutputFile;
            bool scopedToMethod = perInput || perPreviousOutput;
            List<FileInfo> outputFiles = new List<FileInfo>();
            if (scopedToMethod)
            {
                m_Files.Clear();
            }
            if (whatToProcess == ProcessInput.GeneratedFiles)
            {
                if (perPreviousOutput)
                {
                    foreach (FileInfo previousFile in generatedFiles)
                    {
                        m_Files.Clear();
                        m_Files.Add(previousFile);
                        outputFiles.Add(GenerateZip(previousFile, token));
                    }
                }
                else
                {
                    m_Files.AddRange(generatedFiles);
                }
            }
            else
            {
                m_Files.Add(file);
            }
            if (perInput)
            {
                outputFiles.Add(GenerateZip(file, token));
            }
            return new ProcessingResult(ProcessingResultType.Success, outputFiles.ToArray());
        }

        public override void ProcessAggregated(CancellationToken token)
        {
            base.ProcessAggregated(token);
            if (m_Parameters.OneZipFilePer == ProcessorScope.Search)
            {
                GenerateZip(null, token);
            }
        }

        public FileInfo GenerateZip(FileInfo fileForName, CancellationToken token)
        {
            string outputPath = Util.ReplaceUtil.Replace(m_Parameters.OutputPath, fileForName);
            if (m_Files.Count == 0)
            {
                return null;
            }
            string path0 = Path.GetFullPath(m_Files[0].FullName);
            string[] path0Pieces = path0.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            int numSharedPieces = path0Pieces.Length - 1;
            foreach (FileInfo file in m_Files)
            {
                token.ThrowIfCancellationRequested();
                string path = Path.GetFullPath(file.FullName);
                string[] pathPieces = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                numSharedPieces = Math.Min(numSharedPieces, pathPieces.Length);
                for (int i = numSharedPieces - 1; i >= 0; i--)
                {
                    if (!String.Equals(path0Pieces[i], pathPieces[i], StringComparison.OrdinalIgnoreCase))
                    {
                        numSharedPieces = i;
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < numSharedPieces; i++)
            {
                if (i > 0)
                {
                    sb.Append(Path.DirectorySeparatorChar);
                }
                sb.Append(path0Pieces[i]);
            }
            string sharedPath = sb.ToString();
            int sharedPathLength = sharedPath.Length;
            if (!File.Exists(outputPath) || m_Parameters.Overwrite)
            {
                using (FileStream stream = File.Create(outputPath))
                {
                    using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        foreach (FileInfo file in m_Files)
                        {
                            token.ThrowIfCancellationRequested();
                            string path = Path.GetFullPath(file.FullName);
                            string pathInZip = path.Substring(sharedPathLength).TrimStart(Path.DirectorySeparatorChar)
                                .TrimStart(Path.AltDirectorySeparatorChar);
                            if (m_Parameters.AddContainingFolder)
                            {
                                string containingFolderName = Path.GetFileNameWithoutExtension(outputPath);
                                pathInZip = Path.Combine(containingFolderName, pathInZip);
                            }
                            ZipArchiveEntry entry = archive.CreateEntry(pathInZip);
                            entry.LastWriteTime = file.LastWriteTime;
                            using (Stream outputStream = entry.Open())
                            {
                                using (Stream inputStream = File.OpenRead(path))
                                {
                                    inputStream.CopyTo(outputStream);
                                }
                            }
                        }
                    }
                }
            }
            return new FileInfo(outputPath);
        }
    }
}
