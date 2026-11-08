using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class ToodedContext : DbContext
{
    public ToodedContext() { }
    public ToodedContext(DbContextOptions<ToodedContext> options) : base(options) { }
    public DbSet<KataloogiSeaded> KataloogiSeaded => Set<KataloogiSeaded>();
    public DbSet<PostiSeaded> PostiSeaded
    {
        get
        {
            return Set<PostiSeaded>();
        }
    }

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
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("TOODED_CONNECTION_STRING")
                ?? @"Server=(localdb)\MSSQLLocalDB;Database=Tooded_DB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15;Pool Blocking Period=NeverBlock;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
        modelBuilder.Entity<Kategooria>().Property(k => k.ValineId).HasMaxLength(100);
        modelBuilder.Entity<Kategooria>().HasIndex(k => k.ValineId).IsUnique().HasFilter("[ValineId] IS NOT NULL");
        modelBuilder.Entity<Kategooria>().HasOne(k => k.Ylem).WithMany().HasForeignKey(k => k.YlemId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Toode>().Property(t => t.ValineId).HasMaxLength(100);
        modelBuilder.Entity<Toode>().HasIndex(t => t.ValineId).IsUnique().HasFilter("[ValineId] IS NOT NULL");
        modelBuilder.Entity<Toode>().Property(t => t.Uhik).HasMaxLength(40).HasDefaultValue("tk");
        modelBuilder.Entity<Toode>().Property(t => t.Margis).HasMaxLength(40);
        modelBuilder.Entity<KataloogiSeaded>().Property(s => s.Id).ValueGeneratedNever();
        modelBuilder.Entity<KataloogiSeaded>().Property(s => s.Url).HasMaxLength(2048);
        modelBuilder.Entity<KataloogiSeaded>().Property(s => s.KataloogiId).HasMaxLength(100);
        modelBuilder.Entity<KataloogiSeaded>().Property(s => s.ETag).HasMaxLength(1024);
        modelBuilder.Entity<KataloogiSeaded>().Property(s => s.SisuHash).HasMaxLength(64);
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
