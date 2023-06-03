namespace Sorter.BigFiles.Generator
{
    public interface IFileGenerator
    {
        public void CreateFileWithStrings(out long addedLinesCount);
    }

    public class BigFileGenerator : IFileGenerator
    {
        private const int startLinesCount = 1000;
        private readonly long _expectedSize;
        private readonly IStringLineGenerator _linesGenerator;
        private readonly string _filePath;
        private long _addedLines = 0;

        public BigFileGenerator(string outputFilePath, long outputSizeInBytes, IStringLineGenerator linesGenerator)
        {
            _expectedSize = outputSizeInBytes;
            _linesGenerator = linesGenerator;
            _filePath = outputFilePath;
        }

        public void CreateFileWithStrings(out long addedLinesCount)
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            Console.WriteLine();
            Console.Write(GetProgressBar(0, _expectedSize));

            // Here we need to add a certain number of string lines
            // to the file in order to calculate approximately the remaining
            // number of lines to create a file of the required size.
            AddLines(startLinesCount);

            long currentSize = GetCurrentFileSize;
            while (currentSize < _expectedSize)
            {
                Console.Write(GetProgressBar(currentSize, _expectedSize));

                long avrBytesInLine = currentSize / _addedLines; //bytes per line
                long avrRemainLinesCount = (_expectedSize - currentSize) / avrBytesInLine;
                long halfOfRemainLines = avrRemainLinesCount / 2;
                if (halfOfRemainLines < 1000)
                    halfOfRemainLines = 1000;

                AddLines(halfOfRemainLines);
                currentSize = GetCurrentFileSize;
            }

            Console.WriteLine(GetProgressBar(currentSize, _expectedSize));
            addedLinesCount = _addedLines;
        }

        private long GetCurrentFileSize => new FileInfo(_filePath).Length;
        private void AddLines(long linesCount)
        {
            using var sw = new StreamWriter(_filePath, true, System.Text.Encoding.UTF8);
            for (int i = 0; i < linesCount; i++)
            {
                sw.WriteLine(_linesGenerator.GenerateRandomString);
                _addedLines++;
            }
        }
        private string GetProgressBar(long progress, long max)
        {
            if (progress > max) progress = max;
            int percent = (int)((double)progress / max * 100);
            int numFilled = (int)((double)progress / max * 50);
            string progressBar = $"\r|{new string('=', numFilled)}{new string(' ', 50 - numFilled)}| {percent}%";
            return progressBar;
        }
    }
}
