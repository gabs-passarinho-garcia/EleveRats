// <copyright file="ResponsibleContactTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using AwesomeAssertions;
using EleveRats.Modules.Users.Domain.Enums;
using EleveRats.Modules.Users.Domain.ValueObjects;

namespace EleveRats.Tests.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ResponsibleContact"/> value object.
/// </summary>
public class ResponsibleContactTests
{
    private const string _validName = "Maria Passarinho Garcia";
    private const string _validPhone = "11999998888"; // 11-digit mobile
    private const string _validLandlinePhone = "1133334444"; // 10-digit landline
    private const string _validCpf = "52998224725";
    private const Kinship _validKinship = Kinship.Mother;

    // ──────────────────────────── Happy path ────────────────────────────

    [Fact]
    public void Create_WithValidDataAndNoCpf_ShouldReturnContact()
    {
        // Act
        var contact = ResponsibleContact.Create(_validName, _validKinship, _validPhone);

        // Assert
        contact.Should().NotBeNull();
        contact.FullName.Should().Be(_validName);
        contact.Kinship.Should().Be(_validKinship);
        contact.Phone.Should().Be(_validPhone);
        contact.DocumentId.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidDataAndCpf_ShouldReturnContactWithDocumentId()
    {
        // Act
        var contact = ResponsibleContact.Create(_validName, _validKinship, _validPhone, _validCpf);

        // Assert
        contact.DocumentId.Should().Be(_validCpf);
    }

    [Fact]
    public void Create_WithLandlinePhone_ShouldReturnContact()
    {
        // Act
        var contact = ResponsibleContact.Create(_validName, _validKinship, _validLandlinePhone);

        // Assert
        contact.Phone.Should().Be(_validLandlinePhone);
    }

    [Fact]
    public void Create_WithFullNameWithLeadingAndTrailingSpaces_ShouldStoreTrimmedName()
    {
        // Act
        var contact = ResponsibleContact.Create("  Maria Garcia  ", _validKinship, _validPhone);

        // Assert
        contact.FullName.Should().Be("Maria Garcia");
    }

    [Fact]
    public void Create_WithAnyKinship_ShouldStoreCorrectly()
    {
        // Iterate over all defined Kinship values to avoid CS0051 (internal type as public param)
        // and ensure coverage is always in sync with the enum definition.
        foreach (Kinship kinship in Enum.GetValues<Kinship>())
        {
            var contact = ResponsibleContact.Create(_validName, kinship, _validPhone);
            contact.Kinship.Should().Be(kinship);
        }
    }

    // ──────────────────────────── FullName validation ────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyName_ShouldThrowArgumentException(string? name)
    {
        Action act = () => ResponsibleContact.Create(name!, _validKinship, _validPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*full name*");
    }

    [Theory]
    [InlineData("Ab")] // 2 chars — too short
    [InlineData("Jo")] // 2 chars after trim
    public void Create_WithNameTooShort_ShouldThrowArgumentException(string name)
    {
        Action act = () => ResponsibleContact.Create(name, _validKinship, _validPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*full name*");
    }

    // ──────────────────────────── Phone validation ────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyPhone_ShouldThrowArgumentException(string? phone)
    {
        Action act = () => ResponsibleContact.Create(_validName, _validKinship, phone!);
        act.Should().Throw<ArgumentException>().WithMessage("*phone*");
    }

    [Theory]
    [InlineData("999998888")] // 9 digits — missing DDD
    [InlineData("119999988881")] // 12 digits — DDI included by mistake
    [InlineData("(11)99999-8888")] // Formatted — caller forgot to sanitize
    [InlineData("1199999888A")] // Contains a letter
    public void Create_WithInvalidPhone_ShouldThrowArgumentException(string phone)
    {
        Action act = () => ResponsibleContact.Create(_validName, _validKinship, phone);
        act.Should().Throw<ArgumentException>().WithMessage("*phone*");
    }

    // ──────────────────────────── CPF validation ────────────────────────────

    [Theory]
    [InlineData("00000000000")] // All-same digits
    [InlineData("52998224726")] // Wrong check digit
    [InlineData("1234567890")] // Too short (10 digits)
    [InlineData("529.982.247-25")] // Formatted — caller forgot to sanitize
    public void Create_WithInvalidCpf_ShouldThrowArgumentException(string invalidCpf)
    {
        Action act = () =>
            ResponsibleContact.Create(_validName, _validKinship, _validPhone, invalidCpf);
        act.Should().Throw<ArgumentException>().WithMessage("*CPF*");
    }
}
