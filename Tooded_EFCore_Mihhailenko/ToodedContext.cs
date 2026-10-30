using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class ToodedContext : DbContext
{
    public DbSet<Tellimus> Tellimused
    {
        get
        {
            return Set<Tellimus>();
        }
    }

    public DbSet<TellimuseRida> TellimuseRead
    {
        get
        {
            return Set<TellimuseRida>();
        }
    }

    public DbSet<Kasutaja> Kasutajad
    {
        get
        {
            return Set<Kasutaja>();
        }
    }

    public DbSet<Toode> Toodetabel
    {
        get
        {
            return Set<Toode>();
        }
    }

    public DbSet<Kategooria> Kategooriatabel
    {
        get
        {
            return Set<Kategooria>();
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Tooded_DB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15;Pool Blocking Period=NeverBlock;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
        modelBuilder.Entity<Toode>().ToTable("Toodetabel");
        modelBuilder.Entity<Toode>().HasOne(t => t.Kategooria).WithMany(k => k.Tooded).HasForeignKey(t => t.KategooriaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Tellimus>().Property(t => t.Number).HasMaxLength(80);
        modelBuilder.Entity<Tellimus>().HasOne(t => t.Kasutaja).WithMany(k => k.Tellimused).HasForeignKey(t => t.KasutajaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TellimuseRida>().HasOne(r => r.Tellimus).WithMany(t => t.Read).HasForeignKey(r => r.TellimusId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TellimuseRida>().HasOne(r => r.Toode).WithMany().HasForeignKey(r => r.ToodeId).OnDelete(DeleteBehavior.Restrict);
        // Algandmed: ilma kategooriateta ei saa toodet lisada
        modelBuilder.Entity<Kategooria>().HasData(new Kategooria { Id = 1, Kategooria_nimetus = "Joogid" }, new Kategooria { Id = 2, Kategooria_nimetus = "Puu- ja köögiviljad" }, new Kategooria { Id = 3, Kategooria_nimetus = "Maiustused" });
    }
}
