using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Dtos;

public record KayitIstegi(
    string Ad,
    string Soyad,
    string Eposta,
    string Sifre,
    string? Telefon,
    string TcKimlikNo,
    DateOnly DogumTarihi,
    Cinsiyet Cinsiyet);

public record KayitSonucu(bool Basarili, IReadOnlyList<string> Hatalar)
{
    public static KayitSonucu Tamam() => new(true, []);
    public static KayitSonucu Hata(params string[] hatalar) => new(false, hatalar);
}

public enum GirisDurumu
{
    Basarili,
    HataliBilgi,
    HesapPasif,
    Kilitli
}

public record GirisSonucu(GirisDurumu Durum, Kullanici? Kullanici = null, TimeSpan? KalanKilitSuresi = null);
