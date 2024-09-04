using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Data.SQLite;

namespace Forte
{
    internal class DatabaseManager
    {
        private readonly string dbFilePath;

        public DatabaseManager(string dbFilePath)
        {
            this.dbFilePath = dbFilePath;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
        }

        public void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string createTableQuery = @"
                            CREATE TABLE IF NOT EXISTS Forte (
                                TermMonths INTEGER,
                                sum DECIMAL,
                                month1 DECIMAL,
                                month2 DECIMAL,
                                month3 DECIMAL
                            )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
