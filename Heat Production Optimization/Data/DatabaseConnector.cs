using Microsoft.Data.Sqlite;

public class DatabaseConnector
{
    private string _connectionString = "Data/database.db";
    
    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}