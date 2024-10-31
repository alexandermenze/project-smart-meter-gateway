
using Microsoft.AspNetCore.Identity;

var user = "kunde1";
var password = "sanfter wind über drei stille berge";

var hashedPassword = new PasswordHasher<string>().HashPassword(user, password);

Console.WriteLine(hashedPassword);