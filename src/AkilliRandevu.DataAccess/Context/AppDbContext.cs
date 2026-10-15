using AkilliRandevu.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Context;

/// <summary>
/// Veritabanı bağlamı. ER diyagramındaki tablolar, anahtarlar ve ilişkiler burada tanımlanır.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Hasta> Hastalar => Set<Hasta>();
    public DbSet<Doktor> Doktorlar => Set<Doktor>();
    public DbSet<Bolum> Bolumler => Set<Bolum>();
    public DbSet<Belirti> Belirtiler => Set<Belirti>();
    public DbSet<BelirtiBolum> BelirtiBolumleri => Set<BelirtiBolum>();
    public DbSet<BelirtiKaydi> BelirtiKayitlari => Set<BelirtiKaydi>();
    public DbSet<BelirtiKaydiBelirti> BelirtiKaydiBelirtileri => Set<BelirtiKaydiBelirti>();
    public DbSet<Randevu> Randevular => Set<Randevu>();
    public DbSet<Bildirim> Bildirimler => Set<Bildirim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Kullanici>(e =>
        {
            e.Property(x => x.Ad).HasMaxLength(50).IsRequired();
            e.Property(x => x.Soyad).HasMaxLength(50).IsRequired();
            e.Property(x => x.Eposta).HasMaxLength(100).IsRequired();
            e.Property(x => x.Telefon).HasMaxLength(20);
            e.HasIndex(x => x.Eposta).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Kullanici_Rol", "Rol IN (1, 2, 3)"));
        });

        // 1-1: Kullanici - Hasta (FK Hasta.KullaniciId, benzersiz)
        modelBuilder.Entity<Hasta>(e =>
        {
            e.Property(x => x.TcKimlikNo).HasMaxLength(11).IsFixedLength().IsRequired();
            e.HasIndex(x => x.TcKimlikNo).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Hasta_TcKimlikNo", "length(TcKimlikNo) = 11"));
            e.HasOne(x => x.Kullanici)
             .WithOne(k => k.Hasta)
             .HasForeignKey<Hasta>(x => x.KullaniciId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // 1-1: Kullanici - Doktor, 1-N: Bolum - Doktor
        modelBuilder.Entity<Doktor>(e =>
        {
            e.Property(x => x.Unvan).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Kullanici)
             .WithOne(k => k.Doktor)
             .HasForeignKey<Doktor>(x => x.KullaniciId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Bolum)
             .WithMany(b => b.Doktorlar)
             .HasForeignKey(x => x.BolumId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bolum>(e =>
        {
            e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Ad).IsUnique();
        });

        modelBuilder.Entity<Belirti>(e =>
        {
            e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Ad).IsUnique();
        });

        // N-N: Belirti - Bolum (ara tablo, birleşik PK)
        modelBuilder.Entity<BelirtiBolum>(e =>
        {
            e.HasKey(x => new { x.BelirtiId, x.BolumId });
            e.ToTable(t => t.HasCheckConstraint("CK_BelirtiBolum_AgirlikPuani", "AgirlikPuani BETWEEN 1 AND 10"));
            e.HasOne(x => x.Belirti).WithMany(b => b.BelirtiBolumleri).HasForeignKey(x => x.BelirtiId);
            e.HasOne(x => x.Bolum).WithMany(b => b.BelirtiBolumleri).HasForeignKey(x => x.BolumId);
        });

        // 1-N: Hasta - BelirtiKaydi, 1-N: Bolum - BelirtiKaydi (önerilen bölüm, opsiyonel)
        modelBuilder.Entity<BelirtiKaydi>(e =>
        {
            e.Property(x => x.Aciklama).HasMaxLength(1000);
            // Hasta geçmişi ekranı: "bu hastanın kayıtları, en yeniden eskiye"
            e.HasIndex(x => new { x.HastaId, x.OlusturmaTarihi });
            e.HasOne(x => x.Hasta)
             .WithMany(h => h.BelirtiKayitlari)
             .HasForeignKey(x => x.HastaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.OnerilenBolum)
             .WithMany()
             .HasForeignKey(x => x.OnerilenBolumId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // N-N: BelirtiKaydi - Belirti (ara tablo, birleşik PK)
        modelBuilder.Entity<BelirtiKaydiBelirti>(e =>
        {
            e.HasKey(x => new { x.BelirtiKaydiId, x.BelirtiId });
            e.ToTable(t => t.HasCheckConstraint("CK_BelirtiKaydiBelirti_Siddet", "Siddet BETWEEN 1 AND 10"));
            e.HasOne(x => x.BelirtiKaydi).WithMany(k => k.Belirtiler).HasForeignKey(x => x.BelirtiKaydiId);
            e.HasOne(x => x.Belirti)
             .WithMany(b => b.BelirtiKaydiBelirtileri)
             .HasForeignKey(x => x.BelirtiId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // 1-N: Hasta - Randevu, 1-N: Doktor - Randevu, 1-0..1: BelirtiKaydi - Randevu
        modelBuilder.Entity<Randevu>(e =>
        {
            e.Property(x => x.DoktorNotu).HasMaxLength(2000);
            // Takvim ekranı: "bu doktorun şu tarihteki randevuları" ve çakışma kontrolü
            e.HasIndex(x => new { x.DoktorId, x.BaslangicZamani });
            // Hasta geçmişi: "bu hastanın randevuları, tarihe göre"
            e.HasIndex(x => new { x.HastaId, x.BaslangicZamani });
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Randevu_Zaman", "BitisZamani > BaslangicZamani");
                t.HasCheckConstraint("CK_Randevu_Durum", "Durum BETWEEN 1 AND 5");
            });
            e.HasOne(x => x.Hasta)
             .WithMany(h => h.Randevular)
             .HasForeignKey(x => x.HastaId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doktor)
             .WithMany(d => d.Randevular)
             .HasForeignKey(x => x.DoktorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.BelirtiKaydi)
             .WithOne(k => k.Randevu)
             .HasForeignKey<Randevu>(x => x.BelirtiKaydiId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // 1-N: Kullanici - Bildirim
        modelBuilder.Entity<Bildirim>(e =>
        {
            e.Property(x => x.Baslik).HasMaxLength(150).IsRequired();
            e.Property(x => x.Mesaj).HasMaxLength(1000).IsRequired();
            // "okunmamış bildirimlerim" sorgusu
            e.HasIndex(x => new { x.KullaniciId, x.OkunduMu });
            e.HasOne(x => x.Kullanici)
             .WithMany(k => k.Bildirimler)
             .HasForeignKey(x => x.KullaniciId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        SentetikVeri.Ekle(modelBuilder);
    }
}
