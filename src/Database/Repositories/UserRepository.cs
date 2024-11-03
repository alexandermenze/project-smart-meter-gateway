using Dapper;
using Database.Connections;
using Domain.Models;

namespace Database.Repositories;

// This class is responsible for all database operations regarding the users table.
public class UserRepository(DatabaseConnectionFactory connectionFactory)
{
    // Gets a user by it's name
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

    // Updates the password hash for a user
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
}
