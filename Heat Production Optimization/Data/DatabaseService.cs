using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;


public sealed class DatabaseService : IDatabasService
{
    private DatabaseService() {}
    private readonly string _connectionString = "Data Source=Data/database.db";
    private SqliteConnection _conn;
    private static DatabaseService _instance;

    public static DatabaseService DBService
    {
        get
        {
            if (_instance == null)
                _instance = new DatabaseService();
            return _instance;
        }
    }

    public SqliteCommand GetCommand()
    {
        if (_conn == null)
            _conn = new SqliteConnection(_connectionString);
    
        if(_conn.State != System.Data.ConnectionState.Open)
            _conn.Open();

        return _conn.CreateCommand();
    }
}