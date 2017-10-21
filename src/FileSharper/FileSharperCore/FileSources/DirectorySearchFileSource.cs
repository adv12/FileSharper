// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FileSharperCore.Editors;
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
    }

    public class DirectorySearchFileSource : FileSourceBase
    {
        private FilenamePatternParameters m_Parameters = new FilenamePatternParameters();

        private Func<FileInfo, IComparable> m_FileSorter;
        private Func<DirectoryInfo, IComparable> m_DirectorySorter;
        private bool m_Reverse = false;
        private Random m_Random = new Random();

        public override object Parameters => m_Parameters;

        public override string Name => "Directory Search";

        public override string Description => "Searches through the specified directory " +
            "for files matching the specified pattern.";

        public override IEnumerable<FileInfo> Files {
            get
            {
                foreach (FileInfo fi in SearchDirectory(new DirectoryInfo(m_Parameters.Directory)))
                {
                    yield return fi;
                }
            }            
        }

        private IEnumerable<FileInfo> SearchDirectory(DirectoryInfo directoryInfo)
        {
            FileInfo[] files = new FileInfo[0];
            try
            {
                files = directoryInfo.GetFiles(m_Parameters.FilePattern);
                if (m_FileSorter != null)
                {
                    if (m_Reverse)
                    {
                        files = files.OrderByDescending(m_FileSorter).ToArray();
                    }
                    else
                    {
                        files = files.OrderBy(m_FileSorter).ToArray();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
            foreach (FileInfo fi in files)
            {
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
                    foreach (FileInfo fi in SearchDirectory(subdir))
                    {
                        yield return fi;
                    }
                }
            }
        }

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            m_Reverse = false;
            SearchOrder order = m_Parameters.SearchOrder;
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
