using System;
using System.Collections.Concurrent;
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
        /// Counts words in textfile looping lines from File.ReadLines(string path) without cancellation, default: split by space, ignore case
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

        /// <summary>
        /// Counts words in text looping lines from File.ReadLines(string path) with cancellation option and progress bar value update, default split by space, ignore case
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Process file content by chunks of characters with cancellation option
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ConcurrentDictionary<string, int> ProcessBlobFile(CancellationToken cancellationToken)
        {
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            const int bufferSize = 1024 * 1024;
            long fileSize = new FileInfo(TextFileAnalyzer.FilePath).Length;

            var reader = new StreamReader(TextFileAnalyzer.FilePath, Encoding.Default, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize);

            char[] buffer = new char[bufferSize];
            string leftover = "";

            while (!reader.EndOfStream)
            {

                int readCount = reader.Read(buffer, 0, bufferSize);

                string chunk = leftover + new string(buffer, 0, readCount);

                int lastSplit = chunk.LastIndexOfAny(new[] { ' ', '\n', '\r', '\t' });
                if (lastSplit == -1)
                {
                    leftover += chunk;
                    continue;
                }

                string processPart = chunk.Substring(0, lastSplit + 1);
                leftover = chunk.Substring(lastSplit + 1);

                string[] words = processPart.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                };
                try
                {
                    Parallel.ForEach(words, options, word =>
                    {
                        wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                    });
                }
                catch (Exception)
                {

                }
            }

            if (!string.IsNullOrWhiteSpace(leftover))
            {
                string[] words = leftover.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                Parallel.ForEach(words, word =>
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                });
            }

            return wordCounts;
        }
         
        /// <summary>
        /// Process file content by chunks of characters with cancellation option and progress reporting
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public ConcurrentDictionary<string, int> ProcessBlobFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            ConcurrentDictionary<string, int> wordCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            const int bufferSize = 1024 * 1024;
            long fileSize = new FileInfo(TextFileAnalyzer.FilePath).Length;
            long totalRead = 0;

            var reader = new StreamReader(TextFileAnalyzer.FilePath, Encoding.Default, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize);

            char[] buffer = new char[bufferSize];
            string leftover = "";

            while (!reader.EndOfStream)
            {
                //cancellationToken.ThrowIfCancellationRequested();

                int readCount = reader.Read(buffer, 0, bufferSize);
                totalRead += readCount;

                string chunk = leftover + new string(buffer, 0, readCount);

                int lastSplit = chunk.LastIndexOfAny(new[] { ' ', '\n', '\r', '\t' });
                if (lastSplit == -1)
                {
                    leftover += chunk;
                    continue;
                }

                string processPart = chunk.Substring(0, lastSplit + 1);
                leftover = chunk.Substring(lastSplit + 1);

                string[] words = processPart.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                };
                try
                {
                    Parallel.ForEach(words, options, word =>
                    {
                        wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                    });
                }
                catch (Exception)
                {

                }

                if (totalRead % (5 * 1024 * 1024) < bufferSize || reader.EndOfStream)
                {
                    progress?.Report((double)totalRead / fileSize);
                }
            }

            if (!string.IsNullOrWhiteSpace(leftover))
            {
                string[] words = leftover.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                Parallel.ForEach(words, word =>
                {
                    wordCounts.AddOrUpdate(word, 1, (_, current) => current + 1);
                });
                progress?.Report(1.0);
            }

            return wordCounts;
        }

    }
}
