// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows.Controls;
using FileSharperCore;

namespace FileSharperUI
{
    /// <summary>
    /// Interaction logic for SearchViewModelControl.xaml
    /// </summary>
    public partial class SearchViewModelControl : UserControl
    {
        public SearchViewModelControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchDocument doc = DataContext as SearchDocument;
            if (doc != null)
            {
                doc.SearchViewModel?.OpenFileCommand?.Execute(resultsListView.SelectedItems);
            }
        }
    }
}
