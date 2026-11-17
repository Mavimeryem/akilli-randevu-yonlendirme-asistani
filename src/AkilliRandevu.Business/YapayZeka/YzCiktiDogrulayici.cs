using System.Text.Json;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.YapayZeka;

/// <summary>Doğrulamadan geçmiş yapay zekâ önerisi.</summary>
public record YzOnerisi(int BolumId, string Gerekce, YzGuven Guven, bool AcilBelirtiVar, string? AcilGerekce);

/// <summary>
/// Model çıktısını körlemesine kullanmamak için doğrular. Şema zorunlu tutulsa bile (yapılandırılmış çıktı)
/// yanıt yine kontrol edilir: model kesilmiş, boş ya da listede olmayan bir bölüm döndürmüş olabilir.
/// Kural ihlalinde öneri KULLANILMAZ; sistem kural tabanlı sonuca döner.
/// </summary>
public static class YzCiktiDogrulayici
{
    public const int GerekceEnAz = 10;
    public const int GerekceEnFazla = 800;

    public static (YzOnerisi? Oneri, string? Hata) Dogrula(string? json, IReadOnlyCollection<int> izinliBolumler)
    {
        if (string.IsNullOrWhiteSpace(json))
            return (null, "Yanıt boş.");

        JsonElement kok;
        try
        {
            using var doc = JsonDocument.Parse(json);
            kok = doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return (null, "Yanıt geçerli JSON değil.");
        }

        if (kok.ValueKind != JsonValueKind.Object)
            return (null, "Yanıt bir JSON nesnesi değil.");

        // bolum_id: listedeki bölümlerden biri olmalı
        if (!kok.TryGetProperty("bolum_id", out var b) || b.ValueKind != JsonValueKind.Number || !b.TryGetInt32(out int bolumId))
            return (null, "bolum_id eksik ya da sayı değil.");
        if (!izinliBolumler.Contains(bolumId))
            return (null, $"bolum_id={bolumId} izin verilen bölümlerden biri değil.");

        // gerekce: boş olmamalı, makul uzunlukta olmalı
        if (!kok.TryGetProperty("gerekce", out var g) || g.ValueKind != JsonValueKind.String)
            return (null, "gerekce eksik.");
        var gerekce = g.GetString()!.Trim();
        if (gerekce.Length < GerekceEnAz)
            return (null, "gerekce çok kısa.");
        if (gerekce.Length > GerekceEnFazla)
            gerekce = gerekce[..GerekceEnFazla] + "…";

        // guven: dusuk | orta | yuksek
        if (!kok.TryGetProperty("guven", out var gv) || gv.ValueKind != JsonValueKind.String)
            return (null, "guven eksik.");
        YzGuven? guven = gv.GetString() switch
        {
            "dusuk" => YzGuven.Dusuk,
            "orta" => YzGuven.Orta,
            "yuksek" => YzGuven.Yuksek,
            _ => null
        };
        if (guven is null)
            return (null, $"guven değeri geçersiz: {gv.GetString()}");

        // acil işareti
        if (!kok.TryGetProperty("acil_belirti_var", out var a) || (a.ValueKind != JsonValueKind.True && a.ValueKind != JsonValueKind.False))
            return (null, "acil_belirti_var eksik.");
        bool acil = a.GetBoolean();
        string? acilGerekce = kok.TryGetProperty("acil_gerekce", out var ag) && ag.ValueKind == JsonValueKind.String
            ? ag.GetString()!.Trim()
            : null;
        if (string.IsNullOrEmpty(acilGerekce))
            acilGerekce = acil ? "Metinde acil değerlendirme gerektirebilecek bir ifade var." : null;
        else if (acilGerekce.Length > 400)
            acilGerekce = acilGerekce[..400] + "…";

        return (new YzOnerisi(bolumId, gerekce, guven.Value, acil, acil ? acilGerekce : null), null);
    }
}
