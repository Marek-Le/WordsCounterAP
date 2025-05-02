using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace TextFileParser
{
    public class TextFileAnalyzer
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string FileExtension { get; private set; }
        public long FileSizeInBytes { get; private set; }
        public bool ContainsSpaces { get; private set; }
        public bool ContainsNewLines { get; private set; }
        public bool ContainsNonAnsiCharacters { get; private set; }
        public bool ContainsPunctuationChars { get; private set; }

        private TextFileAnalyzer() { }

        internal static TextFileAnalyzer Initialize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string content = File.ReadAllText(filePath, Encoding.Default);

            return new TextFileAnalyzer
            {
                FileName = fileInfo.Name,
                FileExtension = fileInfo.Extension,
                FileSizeInBytes = fileInfo.Length,
                ContainsSpaces = content.Contains(' '),
                ContainsNewLines = content.Contains('\n') || content.Contains('\r'),
                ContainsNonAnsiCharacters = content.Any(c => c > 255),
                ContainsPunctuationChars = content.Any(char.IsPunctuation)
            };
        }

        internal static TextFileAnalyzer Initialize(string filePath, int bufferSize)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            char[] buffer = new char[bufferSize];

            StreamReader reader = new StreamReader(filePath, Encoding.Default, detectEncodingFromByteOrderMarks: false);
            int readCount = reader.Read(buffer, 0, bufferSize);
            string preview = new string(buffer, 0, readCount);

            return new TextFileAnalyzer
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileExtension = fileInfo.Extension,
                FileSizeInBytes = fileInfo.Length,
                ContainsSpaces = preview.Contains(' '),
                ContainsNewLines = preview.Contains('\n') || preview.Contains('\r'),
                ContainsNonAnsiCharacters = preview.Any(c => c > 255),
                ContainsPunctuationChars = preview.Any(char.IsPunctuation)
            };
        }

        public List<string> GetFileReport()
        {
            List<string> fileReport = new List<string>();
            fileReport.Add($"Filename: {FileName}");
            fileReport.Add($"Filesize: {FileSizeInBytes / 1024} KBytes");
            if (ContainsNonAnsiCharacters) fileReport.Add("Warning: text contains non ANSI characters!");
            if (!ContainsSpaces) fileReport.Add("Warning: text does not contain spaces!");
            if (ContainsPunctuationChars) fileReport.Add("Warning: text contains punctuation characters!");

            return fileReport;
        }
    }
}
