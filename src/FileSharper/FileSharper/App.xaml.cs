// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace FileSharper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // https://stackoverflow.com/a/13523188
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            COMException ex = e.Exception as COMException;
            if (ex != null && ex.ErrorCode == -2147221040)
                e.Handled = true;
        }
    }
}
