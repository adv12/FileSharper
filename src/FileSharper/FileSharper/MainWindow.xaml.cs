// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.ComponentModel;
using System.Windows;

namespace FileSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            FileSharperUtil.LoadAssemblies();
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                ((MainViewModel)DataContext)?.Settings?.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error saving settings: " + ex.ToString());
            }
        }
    }
}
