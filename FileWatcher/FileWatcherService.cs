using System;
using System.IO;
using System.Threading;

namespace FileWatcher
{
    public class FileWatcherService
    {
        private readonly string _inputFilePath;
        private readonly string _outputDirectory;
        private FileSystemWatcher _watcher;
        private readonly Settings _settings;

        public FileWatcherService(string inputFilePath, string outputDirectory, Settings settings)
        {
            _inputFilePath = inputFilePath;
            _outputDirectory = outputDirectory;
            _settings = settings;
            ValidatePaths();
        }

        private void ValidatePaths()
        {
            if (!File.Exists(_inputFilePath))
            {
                throw new FileNotFoundException($"Input file not found: {_inputFilePath}");
            }

            if (!Directory.Exists(_outputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_outputDirectory);
                }
                catch (Exception ex)
                {
                    throw new DirectoryNotFoundException($"Cannot create output directory: {ex.Message}");
                }
            }

            // Test write permissions
            string testFile = Path.Combine(_outputDirectory, "test.tmp");
            try
            {
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"No write permission in output directory: {ex.Message}");
            }
        }

        public void Start()
        {
            string inputDir = Path.GetDirectoryName(_inputFilePath) ?? throw new ArgumentException("Invalid file path.");
            string inputFileName = Path.GetFileName(_inputFilePath);

            _watcher = new FileSystemWatcher(inputDir)
            {
                Filter = inputFileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            ConsoleHelper.WriteLineWithColors($"YELLOW 🕵️ Watching file: WHITE {_inputFilePath}");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            ConsoleHelper.WriteLineWithColors($"MAGENTA 📡 Change detected: WHITE {e.FullPath}");
            WaitForFileReady(e.FullPath);

            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                string destinationPath = Path.Combine(_outputDirectory, fileName);
                
                if (_settings.ShowProgressBar)
                {
                    CopyFileWithProgress(e.FullPath, destinationPath);
                }
                else
                {
                    File.Copy(e.FullPath, destinationPath, true);
                }
                
                ConsoleHelper.WriteLineWithColors($"GREEN ✅ Copied to: WHITE {destinationPath}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColors($"RED ❗ Failed to copy file: WHITE {ex.Message}");
                // Log the error details
                File.AppendAllText("error.log", $"{DateTime.Now}: {ex}\n");
            }
        }

        private void CopyFileWithProgress(string sourcePath, string destinationPath)
        {
            using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            
            byte[] buffer = new byte[81920];
            long totalBytes = source.Length;
            long bytesRead = 0;
            int currentBlock;

            while ((currentBlock = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, currentBlock);
                bytesRead += currentBlock;
                ConsoleHelper.ShowProgressBar((int)bytesRead, (int)totalBytes);
            }
            Console.WriteLine();
        }

        private void WaitForFileReady(string path)
        {
            const int maxAttempts = 20;
            const int delayMs = 500;
            long lastSize = -1;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        Thread.Sleep(delayMs);
                        continue;
                    }

                    long size = new FileInfo(path).Length;

                    if (size == lastSize && IsFileUnlocked(path))
                    {
                        return;
                    }

                    lastSize = size;
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColors($"YELLOW ⚠️ Waiting for file: WHITE {ex.Message}");
                    Thread.Sleep(delayMs);
                }
            }

            ConsoleHelper.WriteLineWithColors("RED ⚠️ Timeout waiting for file to become ready.");
        }

        private bool IsFileUnlocked(string path)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return stream.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
        }
    }
}
