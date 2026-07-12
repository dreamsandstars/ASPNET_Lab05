using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AspNetWeek3.Mvc.Controllers;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class AuditLogsController : Controller
{
    private readonly IWebHostEnvironment _env;

    public AuditLogsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IActionResult Index(string? level, string? search, string? date)
    {
        var logDir = Path.Combine(_env.ContentRootPath, "logs");
        var logEntries = new List<LogEntry>();

        if (Directory.Exists(logDir))
        {
            var files = Directory.GetFiles(logDir, "lab05-*.txt");

            if (!string.IsNullOrEmpty(date))
            {
                var formattedDate = date.Replace("-", ""); // e.g. "2026-06-12" -> "20260612"
                files = files.Where(f => f.Contains($"lab05-{formattedDate}")).ToArray();
            }

            foreach (var file in files)
            {
                try
                {
                    using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var entry = ParseLogLine(line);
                        if (entry != null)
                        {
                            logEntries.Add(entry);
                        }
                    }
                }
                catch
                {
                    // Ignore lock exceptions
                }
            }
        }

        // Apply filters
        if (!string.IsNullOrEmpty(level))
        {
            logEntries = logEntries.Where(e => e.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(search))
        {
            logEntries = logEntries.Where(e => e.Message.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        logEntries = logEntries.OrderByDescending(e => e.Timestamp).ToList();

        ViewBag.Levels = new List<string> { "INF", "WRN", "ERR", "DBG" };
        ViewBag.SelectedLevel = level;
        ViewBag.Search = search;
        ViewBag.SelectedDate = date;

        return View(logEntries);
    }

    private LogEntry? ParseLogLine(string line)
    {
        // Pattern 1: yyyy-MM-dd HH:mm:ss.fff zzz [LVL] message
        var match = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s[+-]\d{2}:\d{2}\s\[(\w{3})\]\s(.*)$");
        if (match.Success)
        {
            if (DateTime.TryParse(match.Groups[1].Value, out var dt))
            {
                return new LogEntry
                {
                    Timestamp = dt,
                    Level = match.Groups[2].Value,
                    Message = match.Groups[3].Value
                };
            }
        }

        // Pattern 2: ISO datetime followed by [LVL]
        var altMatch = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+.*?)\s\[(\w+)\]\s(.*)$");
        if (altMatch.Success)
        {
            if (DateTime.TryParse(altMatch.Groups[1].Value, out var dt))
            {
                return new LogEntry
                {
                    Timestamp = dt,
                    Level = altMatch.Groups[2].Value.Substring(0, Math.Min(3, altMatch.Groups[2].Value.Length)),
                    Message = altMatch.Groups[3].Value
                };
            }
        }

        // Pattern 3: simplified format (yyyy-MM-dd HH:mm:ss [LVL] message)
        var simpleMatch = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s\[(\w+)\]\s(.*)$");
        if (simpleMatch.Success)
        {
            if (DateTime.TryParse(simpleMatch.Groups[1].Value, out var dt))
            {
                return new LogEntry
                {
                    Timestamp = dt,
                    Level = simpleMatch.Groups[2].Value.Substring(0, Math.Min(3, simpleMatch.Groups[2].Value.Length)),
                    Message = simpleMatch.Groups[3].Value
                };
            }
        }

        return null;
    }
}
