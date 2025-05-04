using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using TextFileParser;

namespace WordsCounterAP
{
    public class CounterViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WordCount> WordCounts { get; set; }

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

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public ConcurrentDictionary<string, int> _wordCountsDict;

        private TextParser _textParser;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CounterViewModel()
        {
            WordCounts = new ObservableCollection<WordCount>();
        }

        public ConcurrentDictionary<string, int> CountWords(Progress<double> progress)
        {
            CancellationTokenSource = new CancellationTokenSource();
            return _textParser.ProcessTextLines(CancellationTokenSource.Token, progress);
        }

        public ConcurrentDictionary<string, int> CountWords()
        {
            CancellationTokenSource = new CancellationTokenSource();
            return _textParser.ProcessTextLines(CancellationTokenSource.Token);
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
            int bufferSize = 1024;

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
                _textParser = TextParser.Initialize(filePath, bufferSize);
                List<string> infoLines = _textParser.TextFileAnalyzer.GetFileReport();
                WriteToLog(infoLines, true);
                result = true;
            }

            return result;
        }

        private void WriteToLog(List<string> infoLines, bool clearLog)
        {
            if (clearLog) LogText = "";
            foreach (string line in infoLines)
            {
                LogText += line + Environment.NewLine;
            }
        }
    }
}
