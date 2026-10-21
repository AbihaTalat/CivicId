
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace CivicID.Data
{
    public static class DatabaseService
    {
        private static readonly string DatabaseFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "CivicID");

        private static readonly string DatabasePath =
            Path.Combine(DatabaseFolder, "civicid.db");

        private static string ConnectionString =>
            $"Data Source={DatabasePath}";

        public static void Initialize()
        {
            Directory.CreateDirectory(DatabaseFolder);

            using var connection =
                new SqliteConnection(ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS Citizens (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ApplicationReference TEXT NOT NULL UNIQUE,
                    Cnic TEXT NOT NULL UNIQUE,
                    FullName TEXT NOT NULL,
                    DateOfBirth TEXT NOT NULL,
                    Gender TEXT NOT NULL,
                    Address TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    ApplicationStatus TEXT NOT NULL DEFAULT 'Pending',
                    CreatedAt TEXT NOT NULL
                );
                """;

            command.ExecuteNonQuery();
        }

        public static SqliteConnection GetConnection()
        {
            var connection =
                new SqliteConnection(ConnectionString);

            connection.Open();
            return connection;
        }
    }
}
