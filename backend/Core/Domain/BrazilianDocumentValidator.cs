// <copyright file="BrazilianDocumentValidator.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using System.Linq;

namespace EleveRats.Core.Domain;

/// <summary>
/// Provides static validation methods for Brazilian personal documents and contact data.
/// </summary>
/// <remarks>
/// <para>
/// All methods in this class operate on <strong>sanitized, digits-only strings</strong>.
/// Formatting characters (dots, dashes, parentheses, spaces) must be stripped by the caller
/// at the Application or Presentation layer before invoking these methods.
/// </para>
/// <para>
/// This class is intentionally zero-dependency: no external libraries, no regex,
/// no I/O. It belongs in the Domain layer and must stay pure.
/// </para>
/// </remarks>
internal static class BrazilianDocumentValidator
{
    /// <summary>
    /// Determines whether a sanitized Brazilian phone number string is valid.
    /// </summary>
    /// <param name="phone">
    /// Digits-only phone string, without the country code (DDI +55).
    /// Valid lengths: 10 (landline: DDD + 8 digits) or 11 (mobile: DDD + 9-prefix + 8 digits).
    /// </param>
    /// <returns><see langword="true"/> if the phone is valid; otherwise, <see langword="false"/>.</returns>
    internal static bool IsValidPhone(string phone)
    {
        return !string.IsNullOrWhiteSpace(phone)
            && phone.All(char.IsDigit)
            && (phone.Length == 10 || (phone.Length == 11 && phone[2] == '9'));
    }

    /// <summary>
    /// Determines whether a sanitized Brazilian CPF string passes the official check-digit algorithm.
    /// </summary>
    /// <param name="cpf">Digits-only CPF string. Must be exactly 11 characters.</param>
    /// <returns><see langword="true"/> if the CPF is structurally valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method rejects both malformed input (non-digits, wrong length) and
    /// trivially invalid sequences (e.g., "00000000000"), as well as CPFs with
    /// incorrect check digits.
    /// </para>
    /// <para>
    /// Algorithm:
    /// <list type="number">
    ///   <item>Multiply the first 9 digits by descending weights 10..2, sum the products.</item>
    ///   <item>First check digit = (sum × 10 mod 11), clamped to 0 if the result is &gt;= 10.</item>
    ///   <item>Repeat with the first 10 digits and weights 11..2 for the second check digit.</item>
    /// </list>
    /// </para>
    /// </remarks>
    internal static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
        {
            return false;
        }

        // Reject trivially invalid sequences like "00000000000", "11111111111", etc.
        if (cpf.All(c => c == cpf[0]))
        {
            return false;
        }

        return IsValidCpfCheckDigits(cpf);
    }

    private static bool IsValidCpfCheckDigits(string cpf)
    {
        int firstDigit = CalculateCpfCheckDigit(cpf, startWeight: 10, digitCount: 9);
        if (firstDigit != (cpf[9] - '0'))
        {
            return false;
        }

        int secondDigit = CalculateCpfCheckDigit(cpf, startWeight: 11, digitCount: 10);
        return secondDigit == (cpf[10] - '0');
    }

    private static int CalculateCpfCheckDigit(string cpf, int startWeight, int digitCount)
    {
        int sum = Enumerable.Range(0, digitCount).Sum(i => (cpf[i] - '0') * (startWeight - i));

        int remainder = (sum * 10) % 11;
        return remainder >= 10 ? 0 : remainder;
    }
}
