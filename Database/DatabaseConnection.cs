using System;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    /// <summary>
    /// Public class for MySQL database connectivity.
    /// Update Server, Database, UserID, and Password to match your XAMPP / MySQL Workbench setup.
    /// </summary>
    public class DatabaseConnection
    {
        // ── Connection settings ────────────────────────────────────────
        private static readonly string Server   = "localhost";
        private static readonly string Database = "acadsystem";
        private static readonly string UserID   = "root";
        private static readonly string Password = "";          // XAMPP default is blank; change if needed
        private static readonly string Port     = "3308";

        private static readonly string ConnectionString =
            $"Server={Server};Port={Port};Database={Database};Uid={UserID};Pwd={Password};";

        // ── Get an open connection ─────────────────────────────────────
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        // ── Test connectivity ─────────────────────────────────────────
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                    return conn.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
