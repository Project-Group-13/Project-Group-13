using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

internal static class ReplaceData
{
    internal static void Execute<T>(
        string tableName,
        string insertSql,
        Action<SqliteCommand> prepareParameters,
        Action<SqliteCommand, T> bindValues,
        IReadOnlyList<T> data)
    {
        using var conn = new DatabaseConnector().GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        using (var deleteCmd = conn.CreateCommand())
        {
            deleteCmd.Transaction = transaction;
            deleteCmd.CommandText = $"DELETE FROM {tableName};";
            deleteCmd.ExecuteNonQuery();
        }

        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = transaction;
        insertCmd.CommandText = insertSql;
        prepareParameters(insertCmd);

        foreach (var item in data)
        {
            bindValues(insertCmd, item);
            insertCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
