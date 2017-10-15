// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using FileSharperCore.Editors;

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
        public bool UseRegex { get; set; } = false;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public string Regex { get; set; } = ".*";
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool RegexCaseSensitive { get; set; } = false;
    }

    public class DirectorySearchFileSource : FileSourceBase
    {
        private FilenamePatternParameters m_Parameters = new FilenamePatternParameters();

        public override object Parameters => m_Parameters;

        public override string Name => "Directory Search";

        public override string Description => "Searches through the specified directory " +
            "for files matching the specified pattern and optional regular expression.";

        public override IEnumerable<FileInfo> Files {
            get
            {
                Regex regex = null;
                if (m_Parameters.UseRegex)
                {
                    regex = new Regex(m_Parameters.Regex, m_Parameters.RegexCaseSensitive ?
                        RegexOptions.None : RegexOptions.IgnoreCase);
                }
                foreach (FileInfo fi in SearchDirectory(new DirectoryInfo(m_Parameters.Directory), regex))
                {
                    yield return fi;
                }
            }            
        }

        private IEnumerable<FileInfo> SearchDirectory(DirectoryInfo directoryInfo, Regex regex)
        {
            FileInfo[] files = new FileInfo[0];
            try
            {
                files = directoryInfo.GetFiles(m_Parameters.FilePattern);
            }
            catch (UnauthorizedAccessException)
            {

            }
            if (m_Parameters.UseRegex)
            {
                foreach (FileInfo fi in files)
                {
                    if (regex.IsMatch(fi.Name))
                    {
                        yield return fi;
                    }
                }
            }
            else
            {
                foreach (FileInfo fi in files)
                {
                    yield return fi;
                }
            }
            if (m_Parameters.Recursive)
            {
                DirectoryInfo[] subdirs = new DirectoryInfo[0];
                try
                {
                    subdirs = directoryInfo.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                }
                foreach (DirectoryInfo subdir in subdirs)
                {
                    foreach (FileInfo fi in SearchDirectory(subdir, regex))
                    {
                        yield return fi;
                    }
                }
            }
        }
    }
}
