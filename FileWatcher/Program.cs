
class Program
{
    private const string SaveFileName = "filewatcher_paths.txt";

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "File Watcher";
        WriteLineWithColors("CYAN 🔍 File Watcher - by RED weezard12 CYAN 🔁");

        string inputPath = "";
        string outputPath = "";
        string savePath = Path.Combine(Path.GetTempPath(), SaveFileName);

        if (File.Exists(savePath))
        {
            WriteLineWithColors("YELLOW 💾 Found saved paths. Load them? (y/n): ");
            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.KeyChar == 'y')
            {
                var lines = File.ReadAllLines(savePath);
                if (lines.Length >= 2)
                {
                    inputPath = lines[0];
                    outputPath = lines[1];
                    WriteLineWithColors($"GREEN 📥 Input path: WHITE {inputPath}");
                    WriteLineWithColors($"GREEN 📤 Output path: WHITE {outputPath}");
                }
                else
                {
                    WriteLineWithColors("RED ⚠️ Save file is corrupted. WHITE Proceeding with manual input.");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Write("📁 Enter the full path to the input file: ");
            inputPath = Console.ReadLine() ?? string.Empty;

            Console.Write("📁 Enter the full path to the output file: ");
            outputPath = Console.ReadLine() ?? string.Empty;

            File.WriteAllLines(savePath, new[] { inputPath, outputPath });
            WriteLineWithColors($"GRAY 💾 Paths saved to temp: WHITE {savePath}");
        }

        if (!File.Exists(inputPath))
        {
            WriteLineWithColors("RED ❌ Input file does not exist.");
            return;
        }

        WriteLineWithColors("CYAN 🔄 Watching for changes. Press 'q' to quit.");
        var watcher = new FileWatcherService(inputPath, outputPath);
        watcher.Start();

        while (Console.ReadKey().KeyChar != 'q') ;

        watcher.Stop();
        WriteLineWithColors("DARKGRAY \n👋 File watcher stopped.");
    }

    public static void WriteLineWithColors(string message)
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
