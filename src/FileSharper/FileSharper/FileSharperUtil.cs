// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FileSharper
{
    public class FileSharperUtil
    {
        public static void LoadAssemblies()
        {
            HashSet<string> existingDlls = new HashSet<string>();
            List<string> pathsToSearch = new List<string>();
            pathsToSearch.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configDirPath = Path.Combine(appDataPath, "FileSharper");
                Directory.CreateDirectory(configDirPath);
                string configFilePath = Path.Combine(configDirPath, "pluginDirs.conf");
                if (File.Exists(configFilePath))
                {
                    pathsToSearch.AddRange(File.ReadAllLines(configFilePath));
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error loading plugins");
            }
            if (pathsToSearch.Count > 1)
            {
                foreach (string path in pathsToSearch)
                {
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        foreach (FileInfo dll in dir.GetFiles("*.dll"))
                        {
                            if (!existingDlls.Contains(dll.Name))
                            {
                                Assembly.LoadFrom(dll.FullName);
                                existingDlls.Add(dll.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error searching DLLs from {path}: {ex.ToString()}");
                    }
                }
            }
        }
    }
}
