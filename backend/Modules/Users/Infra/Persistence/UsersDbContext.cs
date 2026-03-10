// <copyright file="UserDbContext.cs" company="PlaceholderCompany">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// </copyright>

using Microsoft.EntityFrameworkCore;
using EleveRats.Modules.Users.Infra.Persistence.Models;

namespace EleveRats.Modules.Users.Infra.Persistence;

/// <summary>
/// Database context scoped specifically to the Users module.
/// </summary>
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Represents the users table in the database.
    /// </summary>
    public DbSet<UserDbRecord> Users => Set<UserDbRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // This single line acts as a scout. It scans the current assembly, finds our 
        // UserDbRecordConfiguration (and any others we create later), and applies them all.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}