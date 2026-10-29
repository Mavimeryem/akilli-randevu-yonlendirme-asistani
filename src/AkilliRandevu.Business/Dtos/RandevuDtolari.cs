using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Dtos;

/// <summary>İşlemi yapan kişi. Web katmanı oturumdan doldurur; sahiplik kontrolü buna göre yapılır.</summary>
public record Islemci(Rol Rol, int? HastaId = null, int? DoktorId = null);

public record IslemSonucu(bool Basarili, string? Hata = null)
{
    public static IslemSonucu Tamam() => new(true);
    public static IslemSonucu Hatali(string hata) => new(false, hata);
}

public record IslemSonucu<T>(bool Basarili, T? Veri = default, string? Hata = null)
{
    public static IslemSonucu<T> Tamam(T veri) => new(true, veri);
    public static IslemSonucu<T> Hatali(string hata) => new(false, Hata: hata);
}
