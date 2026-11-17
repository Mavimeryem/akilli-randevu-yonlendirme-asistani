using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Business.Abstract;

public record BelirtiKaydiDetayi(BelirtiKaydi Kayit, YonlendirmeSonucu Sonuc);

public interface IBelirtiService
{
    /// <summary>Belirti kataloğu (giriş ekranı için).</summary>
    Task<List<Belirti>> BelirtileriGetirAsync();

    /// <summary>
    /// Belirtileri doğrular, yönlendirmeyi hesaplar ve belirti kaydı olarak saklar. Kayıt Id'si döner.
    /// yapayZekaOnayi true ise (hastanın açık onayı) açıklama metni yapay zekâ ile de değerlendirilir.
    /// </summary>
    Task<IslemSonucu<int>> DegerlendirAsync(
        int hastaId, IReadOnlyList<SecilenBelirti> belirtiler, string? aciklama, bool yapayZekaOnayi = false, CancellationToken ct = default);

    /// <summary>Yapay zekâ değerlendirmesi şu an kullanılabilir mi (anahtar ayarlı ve etkin)?</summary>
    bool YapayZekaKullanilabilir { get; }

    /// <summary>Kayıt ve yönlendirme sonucu. Hasta yalnızca kendi kaydını görebilir.</summary>
    Task<IslemSonucu<BelirtiKaydiDetayi>> KayitGetirAsync(int kayitId, Islemci kim);

    Task<List<BelirtiKaydi>> HastaKayitlariAsync(int hastaId);
}
