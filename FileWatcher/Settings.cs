using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace FileWatcher
{
    public class Settings
    {
        public bool ShowProgressBar { get; set; } = true;
        public bool AutoStart { get; set; } = false;
        public bool AutoClearConsole { get; set; } = false;
        public int AutoClearInterval { get; set; } = 30; // seconds
        public bool AutoClearOnChange { get; set; } = false;
        public int AutoClearChangeCount { get; set; } = 10;
        public List<WatchedPath> SavedPaths { get; set; } = new();

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileWatcher",
            "settings.json");

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColors($"RED Error loading settings: WHITE {ex.Message}");
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath) ?? string.Empty;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
                ConsoleHelper.WriteLineWithColors("GREEN Settings saved successfully!");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColors($"RED Error saving settings: WHITE {ex.Message}");
            }
        }

        public void AddWatchedPath(string name, string inputPath, string outputPath)
        {
            SavedPaths.Add(new WatchedPath
            {
                Name = name,
                InputPath = inputPath,
                OutputPath = outputPath
            });
            Save();
        }

        public void RemoveWatchedPath(int index)
        {
            if (index >= 0 && index < SavedPaths.Count)
            {
                SavedPaths.RemoveAt(index);
                Save();
            }
        }
    }

    public class WatchedPath
    {
        public string Name { get; set; } = string.Empty;
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
    }
} 