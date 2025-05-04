using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public BackgroundWorker Worker { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        private ConcurrentDictionary<string, int> _wordCountsDict;

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

        public void CountWords()
        {
            CancellationTokenSource = new CancellationTokenSource();           
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                char[] punctuationAndWhitespace = Enumerable.Range(0, 65536)
                .Select(i => (char)i)
                .Where(c => char.IsPunctuation(c) || char.IsWhiteSpace(c))
                .ToArray();

                _wordCountsDict = _textParser.ProcessTextLines(CancellationTokenSource.Token, punctuationAndWhitespace);
                stopwatch.Stop();
                LogText += $"Time elapsed: {stopwatch.Elapsed.TotalSeconds:F2} seconds" + Environment.NewLine;
                //_textParser.ProcessTextLines()
            }
            catch (Exception ex)
            {
                if(CancellationTokenSource.Token.IsCancellationRequested)
                {
                    LogText += "Counting was cancelled." + Environment.NewLine;
                    _wordCountsDict = null;
                }
                else
                {
                    LogText += ex.ToString() + Environment.NewLine;
                }
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
                _textParser = TextParser.Initialize(filePath, bufferSize);
                List<string> infoLines = _textParser.TextFileAnalyzer.GetFileReport();
                WriteToLog(infoLines, true);
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
