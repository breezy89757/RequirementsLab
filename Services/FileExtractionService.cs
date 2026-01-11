using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RequirementsLab.Services;

public class FileExtractionService
{
    private readonly string _baseOutputPath;

    // Regex to find code blocks with filenames
    // Supports:
    // ```python:main.py
    // ```javascript main.js
    // ```main.py
    private static readonly Regex _fileRegex = new Regex(
        @"(?:^|\n)```(?:[\w\+\-\.#]+[ \t:]+)*([\w\-\./\\]+\.\w+)\r?\n(.*?)```", 
        RegexOptions.Singleline | RegexOptions.Compiled);

    public FileExtractionService()
    {
        // Save to a "Generated" folder in the project root
        _baseOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Generated");
        if (!Directory.Exists(_baseOutputPath))
        {
            Directory.CreateDirectory(_baseOutputPath);
        }
    }

    public async Task<List<string>> ExtractAndSaveFilesAsync(string content, string sessionId)
    {
        var savedFiles = new List<string>();
        var sessionPath = Path.Combine(_baseOutputPath, sessionId);
        
        if (!Directory.Exists(sessionPath))
        {
            Directory.CreateDirectory(sessionPath);
        }
        
        var matches = _fileRegex.Matches(content);

        foreach (Match match in matches)
        {
            var filename = match.Groups[1].Value;
            var code = match.Groups[2].Value;

            if (!string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(code))
            {
                // Sanitize filename to prevent Path Traversal
                filename = Path.GetFileName(filename);
                
                var fullPath = Path.Combine(sessionPath, filename);
                
                // Double check ensuring path is within session directory
                if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(sessionPath), StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip suspicious paths
                }
                
                // Ensure directory exists (flat structure within session, or safe subdirs if we allowed them, but let's enforce flat for safety)
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                await File.WriteAllTextAsync(fullPath, code.Trim());
                savedFiles.Add(fullPath);
            }
        }

        return savedFiles;
    }

    public string GetSessionPath(string sessionId)
    {
        return Path.Combine(_baseOutputPath, sessionId);
    }

    public void OpenGeneratedFolder(string sessionId)
    {
        var path = GetSessionPath(sessionId);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        // Use UseShellExecute to reliably open the folder in Windows Explorer
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}
