using Dapper;
using Database.Connections;
using Domain.Models;

namespace Database.Repositories;

// This class is responsible for all database operations regarding the users table.
public class UserRepository(DatabaseConnectionFactory connectionFactory)
{
    public async Task<User?> GetByUsername(string username)
    {
        await using var connection = await connectionFactory.OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(
            """
            SELECT * FROM users 
            WHERE username = @Username
            """,
            new { Username = username });
    }

    public async Task UpdatePasswordHash(User user, string passwordHash)
    {
        await using var connection = await connectionFactory.OpenConnection();

        await connection.ExecuteAsync(
            """
            UPDATE users 
            SET password_hash = @PasswordHash 
            WHERE id = @Id
            """,
            new { PasswordHash = passwordHash, user.Id });
    }

    // In a production ready system, which is out of scope for this project, methods for password reset should be implemented.
}