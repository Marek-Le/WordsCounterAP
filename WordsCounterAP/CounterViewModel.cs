using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using TextFileParser;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace WordsCounterAP
{
    public class CounterViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WordCount> WordCounts { get; set; }

        public TextParser TextParser { get; set; }

        public string FilePath { get; set; }

        private string _logText;
        public string LogText
        {
            get => _logText;
            set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged(nameof(LogText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BackgroundWorker Worker { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        private ConcurrentDictionary<string, int> _wordCountsDict;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CounterViewModel()
        {
            WordCounts = new ObservableCollection<WordCount>();
        }

        public void CountWords()
        {
            CancellationTokenSource = new CancellationTokenSource();           
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                _wordCountsDict = TextParser.ProcessTextLines(CancellationTokenSource.Token);
                stopwatch.Stop();
                LogText += $"Time elapsed: {stopwatch.Elapsed.TotalSeconds:F2} seconds" + Environment.NewLine;
            }
            catch (Exception)
            {
                LogText += "Counting was cancelled." + Environment.NewLine;
                _wordCountsDict = null;
            }
        }

        public void UpdateDataGrid()
        {
            if (_wordCountsDict == null || _wordCountsDict.IsEmpty) return;
            foreach (KeyValuePair<string, int> kvp in _wordCountsDict.OrderByDescending(kvp => kvp.Value))
            {
                WordCount wordCount = new WordCount() { Word = kvp.Key, Count = kvp.Value };
                WordCounts.Add(wordCount);
            }
        }

        public bool LoadFile()
        {
            bool result = false;
            WordCounts.Clear();
            int bufferSize = 1024; // checking only portion of file content

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select text file:",
                Filter = "Text files (*.txt)|*.txt",
                DefaultExt = ".txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                TextParser = TextParser.Initialize(filePath, bufferSize);
                List<string> infoLines = TextParser.TextFileAnalyzer.GetFileReport();
                WriteToLog(infoLines, true);
                FilePath = TextParser.TextFileAnalyzer.FileName;
                result = true;
            }

            return result;
        }

        public void WriteToLog(List<string> infoLines, bool clearLog)
        {
            if (clearLog) LogText = "";
            foreach (string line in infoLines)
            {
                LogText += line + Environment.NewLine;
            }
        }
    }
}
