using System.Collections.Immutable;
using Dapper;
using Database.Connections;
using Domain.Models;

namespace Database.Repositories;

// This class is responsible for all database operations regarding the consumption_values table.
public class ConsumptionValueRepository(DatabaseConnectionFactory connectionFactory)
{
    public async Task<ImmutableArray<ConsumptionValue>> GetUnsent()
    {
        await using var connection = await connectionFactory.OpenConnection();

        var values =
            await connection.QueryAsync<ConsumptionValue>(
                """
                SELECT * FROM consumption_values 
                WHERE sent_to_operator = false
                """);

        return [..values];
    }

    public async Task<ImmutableArray<ConsumptionValue>> GetSentOlderThan(DateTime date)
    {
        await using var connection = await connectionFactory.OpenConnection();

        var values =
            await connection.QueryAsync<ConsumptionValue>(
                """
                SELECT * FROM consumption_values 
                WHERE sent_to_operator = true 
                  AND created < @Date and period_end < @Date
                """,
                new { Date = date });

        return [..values];
    }

    public async Task<ImmutableArray<ConsumptionValue>> GetForMeter(string meterId)
    {
        await using var connection = await connectionFactory.OpenConnection();
        
        var values =
            await connection.QueryAsync<ConsumptionValue>(
                """
                SELECT * FROM consumption_values 
                WHERE meter_id = @MeterId
                """,
                new { MeterId = meterId });
        
        return [..values];
    }
    
    public async Task Insert(ConsumptionValue value)
    {
        await using var connection = await connectionFactory.OpenConnection();

        // SentToOperator is explicitly left out here, because the default value is false and should never be set
        // to true in this context.
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO consumption_values (meter_id, created, period_value, period_start, period_end, signature)
            VALUES (@MeterId, @Created, @PeriodValue, @PeriodStart, @PeriodEnd, @Signature)        
            """,
            value));
    }

    public async Task SetSentToOperator(ConsumptionValue value)
    {
        await using var connection = await connectionFactory.OpenConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE consumption_values SET sent_to_operator = true WHERE id = @Id",
            value));
    }

    public async Task Delete(ConsumptionValue value)
    {
        await using var connection = await connectionFactory.OpenConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM consumption_values WHERE id = @Id",
            value));
    }
}