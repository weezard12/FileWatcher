using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FileWatcher
{
    public static class ConsoleHelper
    {
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

        public static void ClearConsole()
        {
            Console.Clear();
            WriteLineWithColors("CYAN 🔍 File Watcher - by RED weezard12 CYAN 🔁");
        }

        public static void ShowProgressBar(int progress, int total)
        {
            const int barLength = 30;
            float percentage = (float)progress / total;
            int filledLength = (int)(barLength * percentage);

            Console.Write("\r[");
            Console.Write(new string('█', filledLength));
            Console.Write(new string('░', barLength - filledLength));
            Console.Write($"] {percentage:P0}");
        }

        public static void ShowMenu()
        {
            ClearConsole();
            WriteLineWithColors("WHITE === File Watcher Menu ===");
            WriteLineWithColors("WHITE 1. Start Watching");
            WriteLineWithColors("WHITE 2. Settings");
            WriteLineWithColors("WHITE 3. Clear Console");
            WriteLineWithColors("WHITE 4. Exit");
            WriteLineWithColors("WHITE ======================");
            WriteLineWithColors("YELLOW Enter your choice (1-4): ");
        }

        public static int ShowMenuWithArrows(string title, List<string> options)
        {
            int selectedIndex = 0;
            bool done = false;

            while (!done)
            {
                ClearConsole();
                WriteLineWithColors($"WHITE === {title} ===");

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        WriteLineWithColors($"GREEN > {options[i]}");
                    }
                    else
                    {
                        WriteLineWithColors($"WHITE   {options[i]}");
                    }
                }

                WriteLineWithColors("WHITE ======================");
                WriteLineWithColors("YELLOW Use ↑↓ arrows to navigate, Enter to select");

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Count;
                        break;
                    case ConsoleKey.Enter:
                        done = true;
                        break;
                }
            }

            return selectedIndex;
        }

        public static string GetInputWithArrows(string prompt, string defaultValue = "")
        {
            ClearConsole();
            Console.Write($"{prompt}: ");
            string input = defaultValue;
            int cursorPosition = Console.CursorLeft;
            
            Console.Write(input);
            Console.CursorLeft = cursorPosition + input.Length;

            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input[..^1];
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.CursorLeft--;
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return defaultValue;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        public static bool GetYesNoWithArrows(string prompt, bool defaultValue = true)
        {
            ClearConsole();
            int selectedIndex = defaultValue ? 0 : 1;
            bool done = false;

            while (!done)
            {
                Console.Write($"\r{prompt} (y/n): ");
                Console.Write(selectedIndex == 0 ? "Yes" : "No");
                Console.Write(new string(' ', 10)); // Clear any remaining text

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                        selectedIndex = 1 - selectedIndex;
                        break;
                    case ConsoleKey.Enter:
                        done = true;
                        break;
                    case ConsoleKey.Y:
                        selectedIndex = 0;
                        done = true;
                        break;
                    case ConsoleKey.N:
                        selectedIndex = 1;
                        done = true;
                        break;
                }
            }

            Console.WriteLine();
            return selectedIndex == 0;
        }

        public static void ShowInteractiveSettingsMenu(Settings settings)
        {
            var options = new List<(string label, Func<Settings, string> getValue, Action<Settings, int> changeValue, Func<Settings, bool> isEnabled)>
            {
                ("Show progress bar", s => s.ShowProgressBar ? "Yes" : "No", (s, dir) => s.ShowProgressBar = dir > 0 ? true : false, s => true),
                ("Auto-start watching", s => s.AutoStart ? "Yes" : "No", (s, dir) => s.AutoStart = dir > 0 ? true : false, s => true),
                ("Auto-clear console (timer)", s => s.AutoClearConsole ? "Yes" : "No", (s, dir) => s.AutoClearConsole = dir > 0 ? true : false, s => true),
                ("Auto-clear interval (seconds)", s => s.AutoClearInterval.ToString(), (s, dir) => s.AutoClearInterval = Math.Max(1, s.AutoClearInterval + dir), s => s.AutoClearConsole),
                ("Auto-clear on file change", s => s.AutoClearOnChange ? "Yes" : "No", (s, dir) => s.AutoClearOnChange = dir > 0 ? true : false, s => true),
                ("Clear after how many changes?", s => s.AutoClearChangeCount.ToString(), (s, dir) => s.AutoClearChangeCount = Math.Max(1, s.AutoClearChangeCount + dir), s => s.AutoClearOnChange),
            };

            int selected = 0;
            bool done = false;
            while (!done)
            {
                ClearConsole();
                WriteLineWithColors("WHITE === Settings ===");
                for (int i = 0; i < options.Count; i++)
                {
                    var (label, getValue, _, isEnabled) = options[i];
                    string value = getValue(settings);
                    bool enabled = isEnabled(settings);
                    if (i == selected)
                    {
                        if (enabled)
                            WriteLineWithColors($"GREEN > {label}: {value}");
                        else
                            WriteLineWithColors($"GREEN > {label}: {value}");
                    }
                    else
                    {
                        if (enabled)
                            WriteLineWithColors($"WHITE   {label}: {value}");
                        else
                            WriteLineWithColors($"WHITE   {label}: {value}");
                    }
                }
                WriteLineWithColors("WHITE ======================");
                WriteLineWithColors("YELLOW Use ↑↓ to move, ←→ to change, Enter to save and exit");

                var key = Console.ReadKey(true);
                var (_, _, changeValue, isEnabledCurrent) = options[selected];
                bool enabledCurrent = isEnabledCurrent(settings);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = (selected - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Count;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (enabledCurrent)
                        {
                            if ((selected == 3 || selected == 5) && (key.Modifiers & ConsoleModifiers.Shift) != 0)
                                changeValue(settings, -5);
                            else
                                changeValue(settings, -1);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (enabledCurrent)
                        {
                            if ((selected == 3 || selected == 5) && (key.Modifiers & ConsoleModifiers.Shift) != 0)
                                changeValue(settings, 5);
                            else
                                changeValue(settings, 1);
                        }
                        break;
                    case ConsoleKey.Enter:
                        done = true;
                        break;
                }
            }
        }
    }
} 