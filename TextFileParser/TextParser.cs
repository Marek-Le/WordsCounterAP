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

        private TextParser()
        {

        }

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

        public ConcurrentDictionary<string, int> ProcessTextLines()
        {
            string[] allLines = File.ReadAllLines(TextFileAnalyzer.FilePath);
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

        public ConcurrentDictionary<string, int> ProcessTextLines(CancellationToken cancellationToken)
        {
            string[] allLines = File.ReadAllLines(TextFileAnalyzer.FilePath);
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            Parallel.ForEach(allLines, options, line =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    return;
                }
                

                string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                }
            });
            return wordCounts;
        }
    }
}
