using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Business.Abstract;

public record BelirtiKaydiDetayi(BelirtiKaydi Kayit, YonlendirmeSonucu Sonuc);

public interface IBelirtiService
{
    /// <summary>Belirti kataloğu (giriş ekranı için).</summary>
    Task<List<Belirti>> BelirtileriGetirAsync();

    /// <summary>Belirtileri doğrular, yönlendirmeyi hesaplar ve belirti kaydı olarak saklar. Kayıt Id'si döner.</summary>
    Task<IslemSonucu<int>> DegerlendirAsync(int hastaId, IReadOnlyList<SecilenBelirti> belirtiler, string? aciklama);

    /// <summary>Kayıt ve yönlendirme sonucu. Hasta yalnızca kendi kaydını görebilir.</summary>
    Task<IslemSonucu<BelirtiKaydiDetayi>> KayitGetirAsync(int kayitId, Islemci kim);

    Task<List<BelirtiKaydi>> HastaKayitlariAsync(int hastaId);
}
