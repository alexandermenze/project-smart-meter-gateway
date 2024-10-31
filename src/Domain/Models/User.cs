namespace Domain.Models;

public record User(
    int Id,
    string Username,
    // Passwords should never be stored in clear text. This is indicated by the PasswordHash property.
    // This way, even if the database is compromised, the passwords are not.
    string PasswordHash);