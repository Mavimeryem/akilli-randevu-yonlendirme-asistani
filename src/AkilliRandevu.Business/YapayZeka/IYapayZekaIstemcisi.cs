using System.Text.Json;

namespace AkilliRandevu.Business.YapayZeka;

/// <summary>Modelden gelen ham yanıt (henüz doğrulanmamış).</summary>
public record YzHamYanit(string? Metin, bool Reddedildi, string Model, string? DurmaNedeni);

/// <summary>Model çağrısı yapılamadı (zaman aşımı, ağ, 429, 5xx, yanlış anahtar…). Mesaj anahtar içermez.</summary>
public class YzIstemciHatasi : Exception
{
    public YzIstemciHatasi(string mesaj, Exception? ic = null) : base(mesaj, ic)
    {
    }
}

/// <summary>Dil modeli istemcisi. Testlerde sahte istemciyle değiştirilir (gerçek API'ye gidilmez, ücret çıkmaz).</summary>
public interface IYapayZekaIstemcisi
{
    bool Yapilandirildi { get; }

    string Model { get; }

    /// <param name="sistem">Sistem istemi (görev ve kurallar).</param>
    /// <param name="kullanici">Kullanıcı mesajı (hastanın belirtileri ve metni).</param>
    /// <param name="sema">Yanıtın uyması gereken JSON şeması (yapılandırılmış çıktı).</param>
    Task<YzHamYanit> TamamlaAsync(string sistem, string kullanici, IReadOnlyDictionary<string, JsonElement> sema, CancellationToken ct);
}
