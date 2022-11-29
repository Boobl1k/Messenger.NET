﻿using Domain;
using Microsoft.EntityFrameworkCore;

namespace EventBusHandler.Context;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql("Host=db;Port=5432;Username=testuser;Password=testpass;Database=testDb;");

    public DbSet<Message> Messages { get; set; } = null!;
}