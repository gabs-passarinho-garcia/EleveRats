// <copyright file="ResponsibleContact.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Core.Domain;
using EleveRats.Modules.Users.Domain.Enums;

namespace EleveRats.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Represents a legal guardian or responsible contact for a minor warrior.
/// This is a Value Object: identity is determined by its data, not a database ID.
/// It is attached to the <see cref="Entities.Profile"/> aggregate root.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Input Contract:</strong> This Value Object expects sanitized, digits-only input.
/// The responsibility of stripping formatting characters (parentheses, dashes, dots, spaces)
/// and the country code (DDI) belongs to the Application/Presentation layer before calling <see cref="Create"/>.
/// </para>
/// <para>
/// Phone must be 10 digits (landline: DDD + 8 digits) or 11 digits (mobile: DDD + 9-prefix + 8 digits).
/// The Brazilian DDI (+55) is intentionally <strong>not stored</strong>: it is a derived value
/// that infrastructure layers (e.g., SMS gateway, WhatsApp API) can prepend on demand.
/// </para>
/// <para>
/// CPF, when provided, must be exactly 11 digits and pass the Brazilian check-digit algorithm.
/// </para>
/// </remarks>
internal sealed class ResponsibleContact
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResponsibleContact"/> class.
    /// Private constructor to enforce the use of the <see cref="Create"/> factory method.
    /// </summary>
    private ResponsibleContact(string fullName, Kinship kinship, string phone, string? documentId)
    {
        FullName = fullName;
        Kinship = kinship;
        Phone = phone;
        DocumentId = documentId;
    }

    /// <summary>
    /// Gets the full name of the responsible contact.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Gets the familial or legal relationship to the minor.
    /// </summary>
    public Kinship Kinship { get; }

    /// <summary>
    /// Gets the contact phone number as sanitized digits only (10 or 11 chars).
    /// </summary>
    public string Phone { get; }

    /// <summary>
    /// Gets the optional CPF of the contact, stored as sanitized digits only (11 chars).
    /// </summary>
    public string? DocumentId { get; }

    /// <summary>
    /// Factory method that creates a validated <see cref="ResponsibleContact"/> instance.
    /// </summary>
    /// <param name="fullName">The full name of the responsible contact. Must be at least 3 characters.</param>
    /// <param name="kinship">The legal or familial relationship to the minor.</param>
    /// <param name="phone">
    /// Contact phone as digits only (e.g., "11999998888"). Must be 10 or 11 digits.
    /// Formatting must be stripped by the caller before passing in.
    /// </param>
    /// <param name="documentId">
    /// Optional CPF as digits only (e.g., "12345678909"). Must pass the check-digit algorithm.
    /// Formatting must be stripped by the caller before passing in.
    /// </param>
    /// <returns>A new, validated <see cref="ResponsibleContact"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any required field is invalid.</exception>
    public static ResponsibleContact Create(
        string fullName,
        Kinship kinship,
        string phone,
        string? documentId = null
    )
    {
        ValidateFullName(fullName);
        ValidatePhone(phone);

        if (documentId is not null)
        {
            ValidateCpf(documentId);
        }

        return new ResponsibleContact(
            fullName: fullName.Trim(),
            kinship: kinship,
            phone: phone,
            documentId: documentId
        );
    }

    /// <summary>
    /// Factory method to RECONSTITUTE an existing ResponsibleContact from persistence.
    /// Bypasses domain validation rules to ensure that changes in validation logic
    /// do not prevent loading existing, previously valid data.
    /// </summary>
    /// <param name="fullName">The full name stored in persistence.</param>
    /// <param name="kinship">The kinship stored in persistence.</param>
    /// <param name="phone">The phone stored in persistence.</param>
    /// <param name="documentId">The optional document ID stored in persistence.</param>
    /// <returns>A <see cref="ResponsibleContact"/> instance.</returns>
    public static ResponsibleContact Reconstitute(
        string fullName,
        Kinship kinship,
        string phone,
        string? documentId
    ) => new(fullName, kinship, phone, documentId);

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException(
                "Responsible contact full name cannot be empty.",
                nameof(fullName)
            );
        }

        if (fullName.Trim().Length < 3)
        {
            throw new ArgumentException(
                "Responsible contact full name must be at least 3 characters long.",
                nameof(fullName)
            );
        }
    }

    /// <summary>
    /// Validates a sanitized (digits-only) Brazilian phone number, without the country code (DDI).
    /// Accepts 10 digits (landline: DDD + 8) or 11 digits (mobile: DDD + 9-prefix + 8).
    /// </summary>
    /// <remarks>
    /// The DDI (+55) is not part of this value. It is a derived constant that infrastructure
    /// layers should prepend when communicating with external services (e.g., "55" + phone).
    /// </remarks>
    private static void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException(
                "Responsible contact phone number cannot be empty.",
                nameof(phone)
            );
        }

        if (!BrazilianDocumentValidator.IsValidPhone(phone))
        {
            throw new ArgumentException(
                "Responsible contact phone must be digits only, 10 chars (landline) or 11 chars (mobile).",
                nameof(phone)
            );
        }
    }

    /// <summary>
    /// Validates a sanitized (digits-only) Brazilian CPF using the official check-digit algorithm.
    /// Rejects trivially invalid CPFs such as "00000000000" or "11111111111".
    /// </summary>
    private static void ValidateCpf(string cpf)
    {
        if (!BrazilianDocumentValidator.IsValidCpf(cpf))
        {
            throw new ArgumentException(
                "CPF document ID must be exactly 11 digits and pass the check-digit algorithm.",
                nameof(cpf)
            );
        }
    }
}
