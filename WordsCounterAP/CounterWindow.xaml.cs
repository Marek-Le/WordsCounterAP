using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void CountWords_BtnClick(object sender, RoutedEventArgs e)
        {
            CounterProgress.Visibility = Visibility.Visible;
            CancelBtn.IsEnabled = true;
            CounterViewModel.WordCounts.Clear();
            BackgroundWorker worker = new BackgroundWorker();
            CounterViewModel.Worker = worker;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;            
            worker.RunWorkerAsync();

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CounterViewModel.Worker.CancelAsync();
            CounterViewModel.CancellationTokenSource?.Cancel();
            CounterProgress.Visibility = Visibility.Collapsed;
            CancelBtn.IsEnabled = false;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CounterProgress.Visibility = Visibility.Collapsed;
            CancelBtn.IsEnabled = false;
            CounterViewModel.UpdateDataGrid();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            CounterViewModel.CountWords();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogInfo.ScrollToEnd();
        }
    }
}
