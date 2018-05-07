// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections.Generic;

namespace FileSharper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static FileInfo[] StartupFiles { get; set; }

        // https://stackoverflow.com/a/13523188
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            COMException ex = e.Exception as COMException;
            if (ex != null && ex.ErrorCode == -2147221040)
                e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory);
            List<FileInfo> allFiles = new List<FileInfo>();
            foreach (string arg in e.Args)
            {
                try {
                    // handle arguments like "*.fsh"
                    allFiles.AddRange(dir.GetFiles(arg));
                }
                catch (Exception ex)
                {
                    try
                    {
                        // most common scenario: full path
                        allFiles.Add(new FileInfo(arg));
                    }
                    catch (Exception ex2)
                    {
                        // Oh I don't know.  Whatever.
                    }
                }
            }
            StartupFiles = allFiles.ToArray();
        }
    }
}
