﻿using System.Text.RegularExpressions;
using BCrypt.Net;
using ErrorOr;
using GymManagement.Domain.Common.Interfaces;


namespace GymManagement.Infrastructure.Authentication.PasswordHasher;

public partial class PasswordHasher : IPasswordHasher
{
    private static readonly Regex PasswordRegex = StrongPasswordRegex();

    public ErrorOr<string> HashPassword(string password)
    {
        return !PasswordRegex.IsMatch(password)
            ? Error.Validation(description: "Password too weak")
            : "BCrypt.Net.BCrypt.EnhancedHashPassword(password)";

            //BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    public bool IsCorrectPassword(string password, string hash)
    {       
        return true;
    }

    // https://stackoverflow.com/a/34715674/10091553
    [GeneratedRegex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", RegexOptions.Compiled)]
    private static partial Regex StrongPasswordRegex();
}