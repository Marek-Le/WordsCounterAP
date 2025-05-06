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

        public bool IsFileBlob { get; set; } = false;

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public ConcurrentDictionary<string, int> WordsCountDict { get; set; }

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
            if (IsFileBlob)
            {
                return _textParser.ProcessBlobFile(CancellationTokenSource.Token, progress);
            }
            else
            {
                return _textParser.ProcessTextLines(CancellationTokenSource.Token, progress);
            }           
        }

        public ConcurrentDictionary<string, int> CountWords()
        {
            CancellationTokenSource = new CancellationTokenSource();
            if(IsFileBlob)
            {
                return _textParser.ProcessBlobFile(CancellationTokenSource.Token);
            }
            else
            {
                return _textParser.ProcessTextLines(CancellationTokenSource.Token);
            }
        }

        public void UpdateDataGrid()
        {
            if (WordsCountDict == null || WordsCountDict.IsEmpty) return;
            foreach (KeyValuePair<string, int> kvp in WordsCountDict.OrderByDescending(kvp => kvp.Value))
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
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    _textParser = TextParser.Initialize(filePath, bufferSize);
                    List<string> infoLines = _textParser.TextFileAnalyzer.GetFileReport();
                    WriteToLog(infoLines, true);
                    if (_textParser.TextFileAnalyzer.IsBlobFile())
                    {
                        LogText += "Detected Blob file, switching calculation method." + Environment.NewLine;
                        IsFileBlob = true;
                    }
                    else
                    {
                        IsFileBlob = false;
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LogText += ex.ToString();
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
