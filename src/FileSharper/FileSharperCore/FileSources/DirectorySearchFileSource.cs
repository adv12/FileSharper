// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileSharperCore.Editors;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.FileSources
{
    public class FilenamePatternParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        [Editor(typeof(FolderChooserEditor), typeof(FolderChooserEditor))]
        public string Directory { get; set; } = @"C:\";
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool Recursive { get; set; } = true;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string FilePattern { get; set; } = "*";
        [PropertyOrder(4, UsageContextEnum.Both)]
        public SearchOrder SearchOrder { get; set; }
        [PropertyOrder(5, UsageContextEnum.Both)]
        public string SkipDirectoriesMatchingRegex { get; set; }
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool IncludeHidden { get; set; } = false;
        [PropertyOrder(7, UsageContextEnum.Both)]
        public bool IncludeSystem { get; set; } = false;
    }

    public class DirectorySearchFileSource : FileSourceBase
    {
        private FilenamePatternParameters m_Parameters = new FilenamePatternParameters();

        private Func<FileInfo, IComparable> m_FileSorter;
        private Func<DirectoryInfo, IComparable> m_DirectorySorter;
        private Dictionary<FileInfo, int> m_randomFileNumbers;
        private Dictionary<DirectoryInfo, int> m_randomDirectoryNumbers;
        private bool m_Reverse = false;
        private Random m_Random = new Random();
        private string[] m_FilePatterns;
        private Regex m_skipDirectoryRegex;

        public override object Parameters => m_Parameters;

        public override string Name => "Directory Search";

        public override string Category => "Miscellaneous";

        public override string Description => "Searches through the specified directory " +
            "for files matching the specified pattern.";

        public override IEnumerable<FileInfo> Files {
            get
            {
                string directory = ReplaceUtil.Replace(m_Parameters.Directory, (FileInfo)null);
                foreach (FileInfo fi in SearchDirectory(new DirectoryInfo(directory)))
                {
                    if (RunInfo.StopRequested)
                    {
                        yield break;
                    }
                    RunInfo.CancellationToken.ThrowIfCancellationRequested();
                    yield return fi;
                }
            }
        }

        private IEnumerable<FileInfo> SearchDirectory(DirectoryInfo directoryInfo)
        {
            RunInfo?.FileSourceProgress?.Report(directoryInfo.FullName);
            List<FileInfo> files = new List<FileInfo>();
            if (m_skipDirectoryRegex != null)
            {
                bool exclude = false;
                try
                {
                    exclude = m_skipDirectoryRegex.IsMatch(directoryInfo.FullName);
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
                if (exclude)
                {
                    yield break;
                }
            }
            try
            {
                HashSet<FileInfo> fileSet = new HashSet<FileInfo>();
                foreach (string filePattern in m_FilePatterns)
                {
                    if (RunInfo.StopRequested)
                    {
                        yield break;
                    }
                    RunInfo.CancellationToken.ThrowIfCancellationRequested();
                    FileInfo[] results = new FileInfo[0];
                    try
                    {
                        results = directoryInfo.GetFiles(filePattern);
                    }
                    catch (Exception ex)
                    {
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                    }
                    foreach (FileInfo file in results)
                    {
                        if (RunInfo.StopRequested)
                        {
                            yield break;
                        }
                        RunInfo.CancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            if (!fileSet.Contains(file))
                            {
                                if ((m_Parameters.IncludeHidden || !file.Attributes.HasFlag(FileAttributes.Hidden)) &&
                                    (m_Parameters.IncludeSystem || !file.Attributes.HasFlag(FileAttributes.System)))
                                {
                                    fileSet.Add(file);
                                    files.Add(file);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                        }
                    }
                }
                if (m_FileSorter != null)
                {
                    try
                    {
                        if (m_Reverse)
                        {
                            files = files.OrderByDescending(m_FileSorter).ToList();
                        }
                        else
                        {
                            files = files.OrderBy(m_FileSorter).ToList();
                        }
                        m_randomFileNumbers?.Clear();
                    }
                    catch (Exception ex)
                    {
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
            foreach (FileInfo fi in files)
            {
                if (RunInfo.StopRequested)
                {
                    yield break;
                }
                RunInfo.CancellationToken.ThrowIfCancellationRequested();
                yield return fi;
            }
            if (m_Parameters.Recursive)
            {
                DirectoryInfo[] subdirs = new DirectoryInfo[0];
                try
                {
                    subdirs = directoryInfo.GetDirectories();
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
                if (m_DirectorySorter != null)
                {
                    try
                    {
                        if (m_Reverse)
                        {
                            subdirs = subdirs.OrderByDescending(m_DirectorySorter).ToArray();
                        }
                        else
                        {
                            subdirs = subdirs.OrderBy(m_DirectorySorter).ToArray();
                        }
                        m_randomDirectoryNumbers?.Clear();
                    }
                    catch (Exception ex)
                    {
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                    }
                }
                foreach (DirectoryInfo subdir in subdirs)
                {
                    if (RunInfo.StopRequested)
                    {
                        yield break;
                    }
                    RunInfo.CancellationToken.ThrowIfCancellationRequested();
                    bool proceed = false;
                    try
                    {
                        proceed = (m_Parameters.IncludeHidden || !subdir.Attributes.HasFlag(FileAttributes.Hidden)) &&
                        (m_Parameters.IncludeSystem || !subdir.Attributes.HasFlag(FileAttributes.System));
                    }
                    catch (Exception ex)
                    {
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                    }
                    if (proceed)
                    {
                        foreach (FileInfo fi in SearchDirectory(subdir))
                        {
                            if (RunInfo.StopRequested)
                            {
                                yield break;
                            }
                            RunInfo.CancellationToken.ThrowIfCancellationRequested();
                            yield return fi;
                        }
                    }
                }
            }
        }

        public override void LocalInit()
        {
            m_Reverse = false;
            SearchOrder order = m_Parameters.SearchOrder;
            m_FilePatterns = m_Parameters.FilePattern.Split('|');
            if (m_Parameters.SkipDirectoriesMatchingRegex != null)
            {
                m_skipDirectoryRegex = new Regex(m_Parameters.SkipDirectoriesMatchingRegex);
            }
            switch (order)
            {
                case SearchOrder.SystemDefault:
                    m_FileSorter = null;
                    m_DirectorySorter = null;
                    break;
                case SearchOrder.Alphabetical:
                case SearchOrder.ReverseAlphabetical:
                    m_FileSorter = fi => fi.Name;
                    m_DirectorySorter = di => di.Name;
                    m_Reverse = order == SearchOrder.ReverseAlphabetical;
                    break;
                case SearchOrder.ModifiedDate:
                case SearchOrder.ReverseModifiedDate:
                    m_FileSorter = fi => fi.LastWriteTimeUtc;
                    m_DirectorySorter = di => di.LastWriteTimeUtc;
                    m_Reverse = order == SearchOrder.ReverseModifiedDate;
                    break;
                case SearchOrder.Random:
                    m_randomFileNumbers = new Dictionary<FileInfo, int>();
                    m_randomDirectoryNumbers = new Dictionary<DirectoryInfo, int>();
                    m_FileSorter = fi =>
                    {
                        int val;
                        if (!m_randomFileNumbers.ContainsKey(fi))
                        {
                            val = m_Random.Next();
                            m_randomFileNumbers[fi] = val;
                        }
                        else
                        {
                            val = m_randomFileNumbers[fi];
                        }
                        return val;
                    };
                    m_DirectorySorter = di =>
                    {
                        int val;
                        if (!m_randomDirectoryNumbers.ContainsKey(di))
                        {
                            val = m_Random.Next();
                            m_randomDirectoryNumbers[di] = val;
                        }
                        else
                        {
                            val = m_randomDirectoryNumbers[di];
                        }
                        return val;
                    };
                    break;
                default:
                    m_FileSorter = null;
                    m_DirectorySorter = null;
                    break;
            }
        }
    }
}
