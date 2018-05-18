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
            List<FileInfo> files = new List<FileInfo>();
            if (m_skipDirectoryRegex != null)
            {
                if (m_skipDirectoryRegex.IsMatch(directoryInfo.Name))
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
                    FileInfo[] results = directoryInfo.GetFiles(filePattern);
                    foreach (FileInfo file in results)
                    {
                        if (RunInfo.StopRequested)
                        {
                            yield break;
                        }
                        RunInfo.CancellationToken.ThrowIfCancellationRequested();
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
                }
                if (m_FileSorter != null)
                {
                    if (m_Reverse)
                    {
                        files = files.OrderByDescending(m_FileSorter).ToList();
                    }
                    else
                    {
                        files = files.OrderBy(m_FileSorter).ToList();
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
                    if (m_DirectorySorter != null)
                    {
                        if (m_Reverse)
                        {
                            subdirs = subdirs.OrderByDescending(m_DirectorySorter).ToArray();
                        }
                        else
                        {
                            subdirs = subdirs.OrderBy(m_DirectorySorter).ToArray();
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                foreach (DirectoryInfo subdir in subdirs)
                {
                    if (RunInfo.StopRequested)
                    {
                        yield break;
                    }
                    RunInfo.CancellationToken.ThrowIfCancellationRequested();
                    if ((m_Parameters.IncludeHidden || !subdir.Attributes.HasFlag(FileAttributes.Hidden)) &&
                        (m_Parameters.IncludeSystem || !subdir.Attributes.HasFlag(FileAttributes.System)))
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

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
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
                    m_FileSorter = fi => m_Random.Next();
                    m_DirectorySorter = di => m_Random.Next();
                    break;
                default:
                    m_FileSorter = null;
                    m_DirectorySorter = null;
                    break;
            }
        }
    }
}
