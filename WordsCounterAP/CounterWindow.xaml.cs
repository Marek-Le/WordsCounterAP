using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace WordsCounterAP
{
    /// <summary>
    /// Interaction logic for CounterWindow.xaml
    /// </summary>
    public partial class CounterWindow : Window
    {
        public CounterViewModel CounterViewModel { get; set; }

        public CounterWindow()
        {
            InitializeComponent();
            CounterViewModel = new CounterViewModel();
            DataContext = CounterViewModel;
        }

        private void LoadFile_BtnClick(object sender, RoutedEventArgs e)
        {
            if(CounterViewModel.LoadFile()) CountWordsBtn.IsEnabled = true;
        }

        private async void CountWords_BtnClick(object sender, RoutedEventArgs e)
        {
            CounterProgress.Visibility = Visibility.Visible;
            
            CancelBtn.IsEnabled = true;
            CounterViewModel.WordCounts.Clear();
            SearchBox.Clear();

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                ConcurrentDictionary<string, int> result = null;
                if (IndeterminateProgressChckBox.IsChecked == true)
                {
                    CounterProgress.IsIndeterminate = true;
                    result = await Task.Run(() => CounterViewModel.CountWords());
                }
                else
                {
                    CounterProgress.IsIndeterminate = false;
                    Progress<double> progress = new Progress<double>(value => { CounterProgress.Value = value * 100; });
                    result = await Task.Run(() => CounterViewModel.CountWords(progress));
                }
                CounterViewModel._wordCountsDict = result;
            }
            catch (OperationCanceledException ex)
            {
                if(ex.CancellationToken.IsCancellationRequested)
                {
                    CounterViewModel.LogText += "Counting was cancelled." + Environment.NewLine;
                    CounterViewModel._wordCountsDict = null;
                }
            }
            finally
            {
                stopwatch.Stop();
                if(!CounterViewModel.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    CounterViewModel.LogText += $"Time elapsed: {stopwatch.Elapsed.TotalSeconds:F2} seconds" + Environment.NewLine;
                    CounterViewModel.UpdateDataGrid();
                }
                else
                {
                    CounterViewModel.LogText += "Counting was cancelled." + Environment.NewLine;
                    CounterViewModel._wordCountsDict = null;
                }
                
                CounterProgress.Visibility = Visibility.Hidden;
                CounterProgress.Value = 0;
                CancelBtn.IsEnabled = false;            
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CounterViewModel.CancellationTokenSource?.Cancel();
            CounterProgress.Visibility = Visibility.Collapsed;
            CancelBtn.IsEnabled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogInfo.ScrollToEnd();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(SearchBox.Text == String.Empty)
            {
                MainDataGrid.Items.Filter = null;
            }
            else
            {
                Predicate<object> filter = new Predicate<object>(item => (item as WordCount).Word.ToUpper().Contains(SearchBox.Text.ToUpper()));
                MainDataGrid.Items.Filter = filter;
            }
        }
    }
}
