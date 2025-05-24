# File Watcher

A simple and efficient file watching utility that monitors changes to a specified file and copies it to a target directory.

## Features

- 🔍 Monitor changes to a specific file
- 📋 Copy file changes to a target directory
- ⚡ Progress bar for large file operations
- ⚙️ Configurable settings
- 💾 Persistent settings storage
- 🎨 Colorful console interface
- 🛡️ Robust error handling

## Usage

1. Run the application
2. Configure the settings:
   - Input file path: The file you want to monitor
   - Output directory: Where to copy the file when changes are detected
   - Show progress bar: Enable/disable progress bar for file operations
   - Auto-start watching: Automatically start watching when the application launches

3. Start watching:
   - Select option 1 from the main menu
   - The application will begin monitoring the specified file
   - Press 'q' to stop watching

## Menu Options

1. Start Watching - Begin monitoring the configured file
2. Settings - Configure application settings
3. Clear Console - Clear the console screen
4. Exit - Close the application

## Error Handling

The application includes comprehensive error handling:
- Validates input and output paths
- Checks file and directory permissions
- Logs errors to 'error.log'
- Provides clear error messages in the console

## Settings Storage

Settings are stored in a JSON file located at:
```
%APPDATA%\FileWatcher\settings.json
```

## Requirements

- Windows operating system
- .NET 6.0 or later

## Author

Created by weezard12