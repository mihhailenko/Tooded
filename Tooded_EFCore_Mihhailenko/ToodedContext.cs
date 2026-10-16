using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class ToodedContext : DbContext
{
    public DbSet<Kasutaja> Kasutajad => Set<Kasutaja>();
    public DbSet<Toode> Toodetabel => Set<Toode>();
    public DbSet<Kategooria> Kategooriatabel => Set<Kategooria>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=(localdb)\MSSQLLocalDB;Database=Tooded_DB;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
        modelBuilder.Entity<Toode>().ToTable("Toodetabel");

        // Algandmed: ilma kategooriateta ei saa toodet lisada
        modelBuilder.Entity<Kategooria>().HasData(
            new Kategooria { Id = 1, Kategooria_nimetus = "Joogid" },
            new Kategooria { Id = 2, Kategooria_nimetus = "Puu- ja köögiviljad" },
            new Kategooria { Id = 3, Kategooria_nimetus = "Maiustused" });
    }
}
