using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public class QueryCommand : IQuery<SqliteDataReader>
{
    private IDatabasService _dbservice;

    public QueryCommand(IDatabasService dbService)
    {
        _dbservice = dbService;
    }

    public SqliteDataReader Execute(string query, Dictionary<string, object> parameters = null)
    {
        var command = _dbservice.GetCommand();

        command.CommandText = query;

        if(parameters != null)
            foreach(var item in parameters)
                command.Parameters.AddWithValue(item.Key, item.Value);

        return command.ExecuteReader();
    }
}