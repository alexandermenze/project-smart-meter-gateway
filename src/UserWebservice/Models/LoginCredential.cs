using System.ComponentModel.DataAnnotations;

namespace UserWebservice.Models;

// This class defines the structure of the login request.
// Since login requests are made over the network without authentication, the input must be validated.
// The limitation of 32 characters for the username and 128 characters for the password should be sufficient.
public record LoginCredential(
    [MaxLength(32)] string Username,
    [MaxLength(128)] string Password);