namespace Sorter.BigFiles.Generator
{
    internal class BigFileGenerator
    {
        private const int startLinesCount = 1000;
        private readonly long _expectedSize;
        private readonly StringLineGenerator _linesGenerator;
        private readonly string _filePath;
        private long _addedLines = 0;

        public BigFileGenerator(string outputFilePath, long outputSizeInBytes, StringLineGenerator linesGenerator)
        {
            _expectedSize = outputSizeInBytes;
            _linesGenerator = linesGenerator;
            _filePath = outputFilePath;
        }

        public void CreateBigFile(out long addedLinesCount)
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            Console.WriteLine();
            Console.Write(GetProgressBar(0, _expectedSize));

            // Here we need to add a certain number of string lines
            // to the file in order to calculate approximately the remaining
            // number to create a file of the required size.
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
            int percent = (int)((double)progress / max * 100);
            if(percent > 100) percent = 100;
            int numFilled = (int)((double)progress / max * 50);
            string progressBar = $"\r|{new string('=', numFilled)}{new string(' ', 50 - numFilled)}| {percent}%";
            return progressBar;
        }
    }
}
