using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Concrete;

public class RandevuService : IRandevuService
{
    private readonly IRandevuRepository _randevular;
    private readonly IDoktorRepository _doktorlar;
    private readonly IBolumRepository _bolumler;
    private readonly IRepository<Hasta> _hastalar;
    private readonly IRepository<BelirtiKaydi> _belirtiKayitlari;
    private readonly IRepository<Bildirim> _bildirimler;
    private readonly IIslemYoneticisi _islem;
    private readonly TimeProvider _zaman;

    public RandevuService(
        IRandevuRepository randevular,
        IDoktorRepository doktorlar,
        IBolumRepository bolumler,
        IRepository<Hasta> hastalar,
        IRepository<BelirtiKaydi> belirtiKayitlari,
        IRepository<Bildirim> bildirimler,
        IIslemYoneticisi islem,
        IOptions<TakvimAyarlari> ayarlar,
        TimeProvider zaman)
    {
        _randevular = randevular;
        _doktorlar = doktorlar;
        _bolumler = bolumler;
        _hastalar = hastalar;
        _belirtiKayitlari = belirtiKayitlari;
        _bildirimler = bildirimler;
        _islem = islem;
        _zaman = zaman;
        Ayarlar = ayarlar.Value;
    }

    public TakvimAyarlari Ayarlar { get; }

    // Randevu saatleri poliklinik yerel saatiyle tutulur.
    public DateTime Simdi => _zaman.GetLocalNow().DateTime;

    public Task<List<Randevu>> HastaGecmisiAsync(int hastaId) =>
        _randevular.GetHastaRandevulariAsync(hastaId);

    public Task<List<Randevu>> DoktorGunlukAsync(int doktorId, DateOnly gun) =>
        _randevular.GetDoktorGunlukRandevulariAsync(doktorId, gun);

    public Task<List<Randevu>> GunlukTumuAsync(DateOnly gun, int? bolumId) =>
        _randevular.GetGunlukTumuAsync(gun, bolumId);

    public async Task<List<Slot>> SlotlariGetirAsync(int doktorId, DateOnly gun)
    {
        var dolu = (await _randevular.GetDoktorGunlukRandevulariAsync(doktorId, gun))
            .Select(r => (r.BaslangicZamani, r.BitisZamani));
        return SlotUretici.Uret(gun, Ayarlar, Simdi, dolu);
    }

    public async Task<List<Bolum>> BolumleriDoktorlariylaAsync()
    {
        var bolumler = await _bolumler.GetDoktorlarIleAsync();
        foreach (var b in bolumler)
            b.Doktorlar = b.Doktorlar.Where(d => d.Kullanici.AktifMi).OrderBy(d => d.Kullanici.Ad).ToList();
        return bolumler.Where(b => b.Doktorlar.Count > 0).ToList();
    }

    public async Task<IslemSonucu<Randevu>> OlusturAsync(int hastaId, int doktorId, DateTime baslangic, int? belirtiKaydiId = null)
    {
        var simdi = Simdi;
        var gun = DateOnly.FromDateTime(baslangic);

        // --- Doğrulama: zaman kuralları ---
        if (baslangic <= simdi)
            return Hata("Geçmiş bir saate randevu alınamaz.");
        if (gun > DateOnly.FromDateTime(simdi).AddDays(Ayarlar.EnFazlaIleriGun))
            return Hata($"En fazla {Ayarlar.EnFazlaIleriGun} gün sonrasına randevu alınabilir.");

        // Saat, çalışma düzenindeki bir slotun başlangıcı olmalı (mesai dışı, öğle arası, hafta sonu, 09:07 gibi saatler elenir).
        var slot = SlotUretici.Araliklar(gun, Ayarlar).FirstOrDefault(s => s.Baslangic == baslangic);
        if (slot == default)
            return Hata("Seçilen saat poliklinik çalışma saatlerine uygun değil.");

        // --- Doğrulama: ilişkili kayıtlar ---
        var doktor = await _doktorlar.GetDetayliAsync(doktorId);
        if (doktor is null || !doktor.Kullanici.AktifMi || !doktor.Bolum.AktifMi)
            return Hata("Seçilen doktor randevu kabul etmiyor.");

        var hasta = await _hastalar.GetByIdAsync(hastaId);
        if (hasta is null)
            return Hata("Hasta kaydı bulunamadı.");

        if (belirtiKaydiId is int kayitId
            && !await _belirtiKayitlari.AnyAsync(k => k.Id == kayitId && k.HastaId == hastaId))
            return Hata("Belirti kaydı bu hastaya ait değil.");

        // --- Çakışma kontrolü + kayıt: tek transaction (araya başka istek giremez) ---
        return await _islem.TransactionIcindeAsync(async () =>
        {
            if (await _randevular.CakismaVarMiAsync(doktorId, slot.Baslangic, slot.Bitis))
                return Hata("Bu saat dolu. Lütfen başka bir saat seçin.");
            if (await _randevular.HastaCakismaVarMiAsync(hastaId, slot.Baslangic, slot.Bitis))
                return Hata("Aynı saatte başka bir randevunuz var.");
            if (await _randevular.BolumdeAktifRandevuVarMiAsync(hastaId, doktor.BolumId, simdi))
                return Hata($"{doktor.Bolum.Ad} bölümünde zaten aktif bir randevunuz var. Yeni randevu için önce onu iptal edin.");

            var randevu = await _randevular.AddAsync(new Randevu
            {
                HastaId = hastaId,
                DoktorId = doktorId,
                BelirtiKaydiId = belirtiKaydiId,
                BaslangicZamani = slot.Baslangic,
                BitisZamani = slot.Bitis,
                Durum = RandevuDurumu.Beklemede
            });

            var ozet = $"{slot.Baslangic:dd.MM.yyyy HH:mm} {doktor.Bolum.Ad}";
            await BildirAsync(hasta.KullaniciId, "Randevu talebiniz alındı", $"{ozet} randevu talebiniz onay bekliyor.");
            await BildirAsync(doktor.KullaniciId, "Yeni randevu talebi", $"{ozet} için yeni randevu talebi var.");

            return IslemSonucu<Randevu>.Tamam(randevu);
        });
    }

    public async Task<IslemSonucu> DurumDegistirAsync(int randevuId, RandevuDurumu yeniDurum, Islemci kim)
    {
        var r = await _randevular.GetDetayliAsync(randevuId);
        if (r is null)
            return IslemSonucu.Hatali("Randevu bulunamadı.");

        if (RandevuDurumKurallari.Denetle(r, yeniDurum, kim, Simdi, Ayarlar) is string hata)
            return IslemSonucu.Hatali(hata);

        return await _islem.TransactionIcindeAsync(async () =>
        {
            r.Durum = yeniDurum;
            await _randevular.UpdateAsync(r);

            // İlişkili kayıtlar: durum değişikliği hasta ve doktora bildirim olarak düşer.
            var ozet = $"{r.BaslangicZamani:dd.MM.yyyy HH:mm} {r.Doktor.Bolum.Ad}";
            switch (yeniDurum)
            {
                case RandevuDurumu.Onaylandi:
                    await BildirAsync(r.Hasta.KullaniciId, "Randevu onaylandı", $"{ozet} randevunuz onaylandı.");
                    break;
                case RandevuDurumu.IptalEdildi:
                    var iptalEden = kim.Rol switch
                    {
                        Rol.Hasta => "hasta",
                        Rol.Doktor => "doktor",
                        _ => "yönetici"
                    };
                    if (kim.Rol != Rol.Hasta)
                        await BildirAsync(r.Hasta.KullaniciId, "Randevu iptal edildi", $"{ozet} randevunuz {iptalEden} tarafından iptal edildi.");
                    if (kim.Rol != Rol.Doktor)
                        await BildirAsync(r.Doktor.KullaniciId, "Randevu iptal edildi", $"{ozet} randevusu {iptalEden} tarafından iptal edildi.");
                    break;
            }
            return IslemSonucu.Tamam();
        });
    }

    public List<RandevuDurumu> IzinVerilenDurumlar(Randevu randevu, Islemci kim) =>
        RandevuDurumKurallari.IzinVerilenler(randevu, kim, Simdi, Ayarlar);

    private Task BildirAsync(int kullaniciId, string baslik, string mesaj) =>
        _bildirimler.AddAsync(new Bildirim { KullaniciId = kullaniciId, Baslik = baslik, Mesaj = mesaj });

    private static IslemSonucu<Randevu> Hata(string mesaj) => IslemSonucu<Randevu>.Hatali(mesaj);
}
