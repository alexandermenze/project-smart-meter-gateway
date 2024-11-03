using System.Collections.Immutable;
using Dapper;
using Database.Connections;
using Domain.Models;

namespace Database.Repositories;

// This class is responsible for all database operations regarding the logs table.
public class LogRepository(DatabaseConnectionFactory connectionFactory)
{
    // Gets all logs that have been created in the context of the user with the userId (such as logins)
    public async Task<ImmutableArray<LogEntry>> GetForUser(int userId)
    {
        await using var connection = await connectionFactory.OpenConnection();

        var logs = await connection.QueryAsync<LogEntry>(new CommandDefinition(
            """
            SELECT * FROM logs
            WHERE user_id = @UserId
            """,
            new { UserId = userId }));

        return [..logs];
    }

    // Gets all logs that have been created in the context of the meter with meterId
    public async Task<ImmutableArray<LogEntry>> GetForMeter(string meterId)
    {
        await using var connection = await connectionFactory.OpenConnection();

        var logs = await connection.QueryAsync<LogEntry>(new CommandDefinition(
            """
            SELECT * FROM logs
            WHERE meter_id = @MeterId
            """,
            new { MeterId = meterId }));

        return [..logs];
    }

    // Appends a new log entry to the table
    public async Task Insert(LogEntry logEntry)
    {
        await using var connection = await connectionFactory.OpenConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO logs (created, level, message, source, meter_id, user_id)
            VALUES (@Created, @Level, @Message, @Source, @MeterId, @UserId)
            """,
            logEntry));
    }
}
