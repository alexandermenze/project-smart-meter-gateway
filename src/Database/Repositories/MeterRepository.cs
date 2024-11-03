using System.Collections.Immutable;
using Dapper;
using Database.Connections;
using Domain.Models;

namespace Database.Repositories;

// This class is responsible for all database operations regarding the meters table.
public class MeterRepository(DatabaseConnectionFactory connectionFactory)
{
    // Gets all meters assigned to a specific user with userId
    public async Task<ImmutableArray<Meter>> GetByUserId(int userId)
    {
        await using var connection = await connectionFactory.OpenConnection();
        
        var meters =
            await connection.QueryAsync<Meter>(
                """
                SELECT * FROM meters 
                WHERE user_id = @UserId
                """,
                new { UserId = userId });

        return [..meters];
    }
}
