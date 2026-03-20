using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public interface IDatabasService
{
    public SqliteCommand GetCommand();
}