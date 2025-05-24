using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FileWatcher
{
    class Program
    {
        private static Settings _settings = new();
        private static FileWatcherService? _watcher;
        private static readonly List<string> MainMenuOptions = new()
        {
            "Start Watching",
            "Manage Saved Paths",
            "Settings",
            "Exit"
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "File Watcher";
            _settings = Settings.Load();

            if (_settings.AutoClearConsole)
            {
                StartAutoClearConsole();
            }

            while (true)
            {
                int choice = ConsoleHelper.ShowMenuWithArrows("File Watcher Menu", MainMenuOptions);

                switch (choice)
                {
                    case 0:
                        StartWatching();
                        break;
                    case 1:
                        ManageSavedPaths();
                        break;
                    case 2:
                        ShowSettings();
                        break;
                    case 3:
                        StopWatcher();
                        ConsoleHelper.WriteLineWithColors("DARKGRAY 👋 Goodbye!");
                        return;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static void StartWatching()
        {
            try
            {
                string inputPath;
                string outputPath;
                string pathName;

                if (_settings.SavedPaths.Count > 0)
                {
                    var options = new List<string> { "New Path" };
                    options.AddRange(_settings.SavedPaths.Select(p => p.Name));
                    
                    int choice = ConsoleHelper.ShowMenuWithArrows("Select Path", options);
                    
                    if (choice == 0)
                    {
                        // New path
                        inputPath = ConsoleHelper.GetInputWithArrows("Input file path");
                        outputPath = ConsoleHelper.GetInputWithArrows("Output directory");
                        pathName = ConsoleHelper.GetInputWithArrows("Enter a name for this path (optional)");

                        if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrWhiteSpace(outputPath))
                        {
                            ConsoleHelper.WriteLineWithColors("YELLOW ⚠️ Invalid paths. Operation cancelled.");
                            return;
                        }

                        // If no name provided, use "Unnamed"
                        if (string.IsNullOrWhiteSpace(pathName))
                        {
                            pathName = "Unnamed";
                        }

                        // Check if path name already exists
                        var existingPath = _settings.SavedPaths.FirstOrDefault(p => p.Name.Equals(pathName, StringComparison.OrdinalIgnoreCase));
                        if (existingPath != null)
                        {
                            if (!ConsoleHelper.GetYesNoWithArrows($"Path '{pathName}' already exists. Overwrite?"))
                            {
                                return;
                            }
                            _settings.SavedPaths.Remove(existingPath);
                        }

                        // Save the new path
                        _settings.AddWatchedPath(pathName, inputPath, outputPath);
                    }
                    else
                    {
                        // Use existing path
                        var selectedPath = _settings.SavedPaths[choice - 1];
                        inputPath = selectedPath.InputPath;
                        outputPath = selectedPath.OutputPath;
                    }
                }
                else
                {
                    // No saved paths, create new one
                    inputPath = ConsoleHelper.GetInputWithArrows("Input file path");
                    outputPath = ConsoleHelper.GetInputWithArrows("Output directory");
                    pathName = ConsoleHelper.GetInputWithArrows("Enter a name for this path (optional)");

                    if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrWhiteSpace(outputPath))
                    {
                        ConsoleHelper.WriteLineWithColors("YELLOW ⚠️ Invalid paths. Operation cancelled.");
                        return;
                    }

                    // If no name provided, use "Unnamed"
                    if (string.IsNullOrWhiteSpace(pathName))
                    {
                        pathName = "Unnamed";
                    }

                    // Save the new path
                    _settings.AddWatchedPath(pathName, inputPath, outputPath);
                }

                StopWatcher();
                _watcher = new FileWatcherService(inputPath, outputPath, _settings);
                _watcher.Start();

                ConsoleHelper.WriteLineWithColors("CYAN 🔄 Watching for changes. Press 'q' to stop.");
                while (Console.ReadKey(true).KeyChar != 'q') ;
                StopWatcher();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColors($"RED ❌ Error: WHITE {ex.Message}");
            }
        }

        private static void ManageSavedPaths()
        {
            if (_settings.SavedPaths.Count == 0)
            {
                ConsoleHelper.WriteLineWithColors("YELLOW ⚠️ No saved paths found.");
                return;
            }

            var options = _settings.SavedPaths.Select(p => $"{p.Name} ({p.InputPath} → {p.OutputPath})").ToList();
            options.Add("Back");

            while (true)
            {
                int choice = ConsoleHelper.ShowMenuWithArrows("Saved Paths", options);
                
                if (choice == options.Count - 1) // Back option
                    break;

                var pathOptions = new List<string> { "Start Watching", "Rename", "Remove", "Back" };
                int action = ConsoleHelper.ShowMenuWithArrows("Path Options", pathOptions);

                switch (action)
                {
                    case 0: // Start Watching
                        var selectedPath = _settings.SavedPaths[choice];
                        StopWatcher();
                        _watcher = new FileWatcherService(selectedPath.InputPath, selectedPath.OutputPath, _settings);
                        _watcher.Start();
                        ConsoleHelper.WriteLineWithColors("CYAN 🔄 Watching for changes. Press 'q' to stop.");
                        while (Console.ReadKey(true).KeyChar != 'q') ;
                        StopWatcher();
                        break;
                    case 1: // Rename
                        var pathToRename = _settings.SavedPaths[choice];
                        string newName = ConsoleHelper.GetInputWithArrows("Enter new name", pathToRename.Name);
                        
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            // Check if new name already exists
                            var existingPath = _settings.SavedPaths.FirstOrDefault(p => 
                                p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && 
                                p != pathToRename);
                            
                            if (existingPath != null)
                            {
                                ConsoleHelper.WriteLineWithColors($"RED ❌ Path name '{newName}' already exists.");
                            }
                            else
                            {
                                pathToRename.Name = newName;
                                _settings.Save();
                                options[choice] = $"{newName} ({pathToRename.InputPath} → {pathToRename.OutputPath})";
                            }
                        }
                        break;
                    case 2: // Remove
                        if (ConsoleHelper.GetYesNoWithArrows("Are you sure you want to remove this path?"))
                        {
                            _settings.RemoveWatchedPath(choice);
                            options.RemoveAt(choice);
                            if (options.Count == 1) // Only "Back" remains
                                break;
                        }
                        break;
                }
            }
        }

        private static void ShowSettings()
        {
            ConsoleHelper.ClearConsole();
            ConsoleHelper.WriteLineWithColors("WHITE === Settings ===");

            _settings.ShowProgressBar = ConsoleHelper.GetYesNoWithArrows("Show progress bar", _settings.ShowProgressBar);
            _settings.AutoStart = ConsoleHelper.GetYesNoWithArrows("Auto-start watching", _settings.AutoStart);
            _settings.AutoClearConsole = ConsoleHelper.GetYesNoWithArrows("Auto-clear console", _settings.AutoClearConsole);
            
            if (_settings.AutoClearConsole)
            {
                string interval = ConsoleHelper.GetInputWithArrows("Auto-clear interval (seconds)", _settings.AutoClearInterval.ToString());
                if (int.TryParse(interval, out int newInterval) && newInterval > 0)
                {
                    _settings.AutoClearInterval = newInterval;
                }
            }

            _settings.Save();
        }

        private static void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher = null;
            }
        }

        private static void StartAutoClearConsole()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_settings.AutoClearInterval * 1000);
                    if (_settings.AutoClearConsole)
                    {
                        ConsoleHelper.ClearConsole();
                    }
                }
            });
        }
    }
}
