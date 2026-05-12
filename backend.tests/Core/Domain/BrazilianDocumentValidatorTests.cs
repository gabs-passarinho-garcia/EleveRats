// <copyright file="BrazilianDocumentValidatorTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using AwesomeAssertions;
using EleveRats.Core.Domain;

namespace EleveRats.Tests.Core.Domain;

/// <summary>
/// Unit tests for the <see cref="BrazilianDocumentValidator"/> static utility.
/// </summary>
public class BrazilianDocumentValidatorTests
{
    // ──────────────────────────── CPF ────────────────────────────

    [Theory]
    [InlineData("52998224725")] // Valid, frequently used in test suites
    [InlineData("11144477735")] // Another well-known valid CPF
    [InlineData("01234567890")] // Leading zero variant
    public void IsValidCpf_WithValidCpf_ShouldReturnTrue(string cpf) =>
        _ = BrazilianDocumentValidator.IsValidCpf(cpf).Should().BeTrue();

    [Theory]
    [InlineData("52998224726")] // Last digit off by one
    [InlineData("11144477736")] // Second digit tampered
    [InlineData("12345678900")] // Sequential, wrong check digits
    public void IsValidCpf_WithInvalidCheckDigits_ShouldReturnFalse(string cpf) =>
        _ = BrazilianDocumentValidator.IsValidCpf(cpf).Should().BeFalse();

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("99999999999")]
    public void IsValidCpf_WithAllSameDigits_ShouldReturnFalse(string cpf) =>
        // These pass the check-digit math but are structurally rejected by Brazilian law.
        _ = BrazilianDocumentValidator.IsValidCpf(cpf).Should().BeFalse();

    [Theory]
    [InlineData("1234567890")] // 10 digits (too short)
    [InlineData("123456789012")] // 12 digits (too long)
    [InlineData("")]
    public void IsValidCpf_WithWrongLength_ShouldReturnFalse(string cpf) =>
        _ = BrazilianDocumentValidator.IsValidCpf(cpf).Should().BeFalse();

    [Theory]
    [InlineData("529.982.247-25")] // Formatted — caller must sanitize
    [InlineData("5299822472A")] // Contains a letter
    [InlineData("52998224 25")] // Contains a space
    public void IsValidCpf_WithNonDigitCharacters_ShouldReturnFalse(string cpf) =>
        _ = BrazilianDocumentValidator.IsValidCpf(cpf).Should().BeFalse();

    // ──────────────────────────── Phone ────────────────────────────

    [Theory]
    [InlineData("1133334444")] // 10 digits — landline (DDD + 8)
    [InlineData("11999998888")] // 11 digits — mobile  (DDD + 9-prefix + 8)
    [InlineData("1122223333")] // São Paulo landline
    public void IsValidPhone_WithValidPhone_ShouldReturnTrue(string phone) =>
        _ = BrazilianDocumentValidator.IsValidPhone(phone).Should().BeTrue();

    [Theory]
    [InlineData("999998888")] // 9 digits — missing DDD
    [InlineData("119999988881")] // 12 digits — DDI accidentally included
    [InlineData("11899998888")] // 11 digits — mobile but doesn't start with '9' at index 2
    [InlineData("11000000000")] // 11 digits — clearly invalid
    public void IsValidPhone_WithWrongFormatOrLength_ShouldReturnFalse(string phone) =>
        _ = BrazilianDocumentValidator.IsValidPhone(phone).Should().BeFalse();

    [Theory]
    [InlineData("(11)99999-8888")] // Formatted — caller must sanitize
    [InlineData("1199999888A")] // Contains a letter
    [InlineData("1199 99 888")] // Contains spaces
    public void IsValidPhone_WithNonDigitCharacters_ShouldReturnFalse(string phone) =>
        _ = BrazilianDocumentValidator.IsValidPhone(phone).Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsValidPhone_WithNullOrEmpty_ShouldReturnFalse(string? phone) =>
        _ = BrazilianDocumentValidator.IsValidPhone(phone!).Should().BeFalse();
}
