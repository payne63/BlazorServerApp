using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Db;

public class DataContext : DbContext
{
    public string DbPath = @"Db\DBcontacts.db";

    public DbSet<ContactModel> Contacts
    {
        get;
        set;
    }

    public DbSet<CompagnyModel> Compagnies
    {
        get;
        set;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
}