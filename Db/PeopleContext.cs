using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BlazorServerApp.Db;

public class PeopleContext : DbContext
{
    public string DbPath { get; } = @"database\people.db";

    public DbSet<PersonModel> People { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ensure the database directory exists
        var dir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}