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
using System.Collections.Generic;
using System.Linq;
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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);

        byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
        byte[] hash = GenerateHash(password, salt, _iterations, _memorySize, _degreeOfParallelism);

        // PHC format: $argon2id$v=19$m=65536,t=3,p=2$salt$hash
        return $"$argon2id$v=19$m={_memorySize},t={_iterations},p={_degreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashString)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(hashString);

        // Check if it is the new PHC format
        if (hashString.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            return VerifyPhcPassword(password, hashString);
        }

        // Fallback to legacy format salt:hash
        return VerifyLegacyPassword(password, hashString);
    }

    private static bool VerifyPhcPassword(string password, string hashString)
    {
        string[] parts = hashString.Split('$');
        if (parts.Length != 6)
        {
            return false;
        }

        // parts[1] is "argon2id", parts[2] is "v=19"
        // parts[3] contains m=65536,t=3,p=2
        var paramMap = parts[3]
            .Split(',')
            .Select(p => p.Split('='))
            .Where(kv => kv.Length == 2)
            .ToDictionary(kv => kv[0], kv => kv[1], StringComparer.Ordinal);

        int memory =
            paramMap.TryGetValue("m", out string? mStr) && int.TryParse(mStr, out int m)
                ? m
                : _memorySize;
        int iterations =
            paramMap.TryGetValue("t", out string? tStr) && int.TryParse(tStr, out int t)
                ? t
                : _iterations;
        int parallelism =
            paramMap.TryGetValue("p", out string? pStr) && int.TryParse(pStr, out int p)
                ? p
                : _degreeOfParallelism;

        byte[] salt = Convert.FromBase64String(parts[4]);
        byte[] expectedHash = Convert.FromBase64String(parts[5]);

        byte[] actualHash = GenerateHash(password, salt, iterations, memory, parallelism);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static bool VerifyLegacyPassword(string password, string hashString)
    {
        string[] parts = hashString.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            // Legacy uses hardcoded constants
            byte[] actualHash = GenerateHash(
                password,
                salt,
                _iterations,
                _memorySize,
                _degreeOfParallelism
            );

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] GenerateHash(
        string password,
        byte[] salt,
        int iterations,
        int memorySize,
        int degreeOfParallelism
    )
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = degreeOfParallelism,
            Iterations = iterations,
            MemorySize = memorySize,
        };

        return argon2.GetBytes(_hashSize);
    }
}
