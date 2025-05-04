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
        /// Counts words in text looping lines from File.ReadLines(string path) without cancellation, default split by space, ignore case
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
        /// Counts words in text looping lines from File.ReadLines(string path) with cancellation option, default split by space, ignore case
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

            try
            {
                Parallel.ForEach(allLines, options, line =>
                {
                    string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in words)
                    {
                        wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                    }
                });
            }
            catch (Exception)
            {

            }

            return wordCounts;
        }

        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var allLines = File.ReadLines(TextFileAnalyzer.FilePath).ToList();
            int totalLines = allLines.Count;
            int processedLines = 0;

            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            try
            {
                Parallel.ForEach(allLines, options, line =>
                {
                    string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in words)
                    {
                        wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                    }

                    int done = Interlocked.Increment(ref processedLines);
                    if (done % 10000 == 0 || done == totalLines)
                    {
                        progress?.Report((double)done / totalLines);
                    }
                });
            }
            catch (Exception)
            {
                
            }
            return wordCounts;
        }

    }
}
