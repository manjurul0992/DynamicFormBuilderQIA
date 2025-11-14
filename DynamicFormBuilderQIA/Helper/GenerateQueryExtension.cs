using Microsoft.Data.SqlClient;
using System.Data;

namespace DynamicFormBuilderQIA.Helper;

public static class GenerateQueryExtension
{
    public static SqlParameter CreateParameter(string name, object value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    public static SqlParameter CreateOutputParameter(string name, SqlDbType type)
    {
        return new SqlParameter(name, type) { Direction = ParameterDirection.Output };
    }

    public static async Task ExecuteReaderAsync(string connectionString, string procedureName, SqlParameter[] parameters, Func<SqlDataReader, Task> readerAction)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };

        if (parameters != null) command.Parameters.AddRange(parameters);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        await readerAction(reader);
    }

    public static async Task ExecuteNonQueryAsync(string connectionString, string procedureName, SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };

        if (parameters != null) command.Parameters.AddRange(parameters);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
}
