using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextFileParser
{
    public class TextParser
    {
        public TextFileAnalyzer TextFileAnalyzer { get; set; }

        private TextParser() { }

        public static TextParser Initialize(string filePath)
        {
            TextParser parser = new TextParser();
            parser.TextFileAnalyzer = TextFileAnalyzer.Initialize(filePath);
            return parser;
        }

        public static TextParser Initialize(string filePath, int bufferSize)
        {
            TextParser parser = new TextParser();
            parser.TextFileAnalyzer = TextFileAnalyzer.Initialize(filePath, bufferSize);
            return parser;
        }

        /// <summary>
        /// Counts words in text looping lines from File.ReadLines(string path) without cancellation, default split by space
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, int> ProcessTextLines()
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(allLines, options, line =>
            {
                string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                }
            });
            return wordCounts;
        }

        /// <summary>
        /// Counts words in text looping lines from File.ReadLines(string path) without cancellation
        /// </summary>
        /// <param name="splitChars">characters to split words, if null words are split by space</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, int> ProcessTextLines(char[] splitChars)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(allLines, options, line =>
            {
                string[] words = line.Split(splitChars ?? (char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                }
            });

            return wordCounts;
        }

        /// <summary>
        /// Counts words in text looping lines from File.ReadLines(string path) with cancellation option, default split by space
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            Parallel.ForEach(allLines, options, line =>
            {               
                string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                }
            });
            return wordCounts;
        }

        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken, bool isCaseSensitive)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(comparer);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            Parallel.ForEach(allLines, options, line =>
            {
                string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    string processedWord = isCaseSensitive ? word : word.ToLowerInvariant();
                    wordCounts.AddOrUpdate(processedWord, 1, (_, current) => current + 1);
                }
            });

            return wordCounts;
        }

        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken, char[] splitChars)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            Parallel.ForEach(allLines, options, line =>
            {
                string[] words = line.Split(splitChars ?? (char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                }
            });
            return wordCounts;
        }

        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken, bool isCaseSensitive = false, bool ignorePunctuation = true)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath);
            var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(comparer);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            Parallel.ForEach(allLines, options, line =>
            {
                string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    string processedWord = word;

                    if (ignorePunctuation)
                    {
                        processedWord = new string(processedWord.Where(c => !char.IsPunctuation(c)).ToArray());
                    }

                    if (!isCaseSensitive)
                    {
                        processedWord = processedWord.ToLowerInvariant();
                    }

                    if (!string.IsNullOrWhiteSpace(processedWord))
                    {
                        wordCounts.AddOrUpdate(processedWord, 1, (_, current) => current + 1);
                    }
                }
            });

            return wordCounts;
        }
    }
}
