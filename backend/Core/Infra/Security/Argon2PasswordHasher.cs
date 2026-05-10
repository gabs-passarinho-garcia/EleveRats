// <copyright file="Argon2PasswordHasher.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

using System;
using System.Security.Cryptography;
using System.Text;
using EleveRats.Core.Application.Interfaces.Security;
using Konscious.Security.Cryptography;

namespace EleveRats.Core.Infra.Security;

/// <summary>
/// Argon2id implementation for password hashing, providing robust defense against GPU attacks.
/// </summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    private const int _saltSize = 16;
    private const int _degreeOfParallelism = 2;
    private const int _iterations = 3;
    private const int _memorySize = 65536; // 64 MB
    private const int _hashSize = 32;

    public string HashPassword(string password)
    {
        byte[] salt = new byte[_saltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = GenerateHash(password, salt);

        // Formats the output as salt:hash using Base64
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashString)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(hashString);

        string[] parts = hashString.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        byte[] actualHash = GenerateHash(password, salt);

        // FixedTimeEquals prevents timing attacks by always taking the same amount of time to compare
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] GenerateHash(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = _degreeOfParallelism,
            Iterations = _iterations,
            MemorySize = _memorySize,
        };

        return argon2.GetBytes(_hashSize);
    }
}
