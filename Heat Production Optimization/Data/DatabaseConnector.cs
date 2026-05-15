using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public class DatabaseConnector
{
    private readonly string _connectionString;

    public DatabaseConnector()
    {
        var databasePath = ResolveDatabasePath();
        _connectionString = $"Data Source={databasePath}";
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private static string ResolveDatabasePath()
    {
        var candidates = new List<string>
        {
            Path.Combine(Environment.CurrentDirectory, "Data", "database.db"),
            Path.Combine(Environment.CurrentDirectory, "database.db"),
            Path.Combine(AppContext.BaseDirectory, "Data", "database.db"),
            Path.Combine(AppContext.BaseDirectory, "database.db")
        };

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && directory != null; i++)
        {
            candidates.Add(Path.Combine(directory.FullName, "Data", "database.db"));
            candidates.Add(Path.Combine(directory.FullName, "Heat Production Optimization", "Data", "database.db"));
            directory = directory.Parent;
        }

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Fall back to output-relative location for first run.
        return Path.Combine(AppContext.BaseDirectory, "Data", "database.db");
    }
}