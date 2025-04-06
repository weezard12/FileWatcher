
public class FileWatcherService
{
    private readonly string _inputFilePath;
    private readonly string _outputDirectory;
    private FileSystemWatcher _watcher;

    public FileWatcherService(string inputFilePath, string outputDirectory)
    {
        _inputFilePath = inputFilePath;
        _outputDirectory = outputDirectory;
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
        WriteLineWithColors($"YELLOW 🕵️ Watching file: WHITE {_inputFilePath}");
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        WriteLineWithColors($"MAGENTA 📡 Change detected: WHITE {e.FullPath}");
        WaitForFileReady(e.FullPath);

        try
        {
            string fileName = Path.GetFileName(e.FullPath);
            string destinationPath = Path.Combine(_outputDirectory, fileName);
            File.Copy(e.FullPath, destinationPath, true);
            WriteLineWithColors($"GREEN ✅ Copied to: WHITE {destinationPath}");
        }
        catch (IOException ex)
        {
            WriteLineWithColors($"RED ❗ Failed to copy file: WHITE {ex.Message}");
        }
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
            catch
            {
                Thread.Sleep(delayMs);
            }
        }

        WriteLineWithColors("RED ⚠️ Timeout waiting for file to become ready.");
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
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }

    private void WriteLineWithColors(string message)
    {
        string[] parts = message.Split(new[] { ' ' }, StringSplitOptions.None);
        var originalColor = Console.ForegroundColor;

        foreach (var part in parts)
        {
            if (Enum.TryParse(typeof(ConsoleColor), part, true, out object? color))
            {
                Console.ForegroundColor = (ConsoleColor)color!;
            }
            else
            {
                Console.Write(part + " ");
            }
        }

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }
}
