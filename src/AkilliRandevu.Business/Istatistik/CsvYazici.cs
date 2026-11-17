using System.Text;

namespace AkilliRandevu.Business.Istatistik;

/// <summary>
/// Dışa aktarma için CSV üretir. Türkçe Excel'in beklediği biçim: ayraç ";" ve UTF-8 (BOM ile).
/// Güvenlik: "=", "+", "-", "@" ile başlayan hücreler Excel'de formül olarak çalışabilir (CSV enjeksiyonu);
/// başlarına "'" eklenerek düz metne çevrilir.
/// </summary>
public static class CsvYazici
{
    public const char Ayrac = ';';

    public static string Yaz(IEnumerable<IReadOnlyList<string?>> satirlar)
    {
        var sb = new StringBuilder();
        foreach (var satir in satirlar)
            sb.Append(string.Join(Ayrac, satir.Select(Hucre))).Append("\r\n");
        return sb.ToString();
    }

    public static byte[] Bayt(string csv) => Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

    public static string Hucre(string? deger)
    {
        var s = deger ?? "";
        if (s.Length > 0 && "=+-@\t\r".Contains(s[0]))
            s = "'" + s;
        bool tirnak = s.IndexOfAny([Ayrac, '"', '\n', '\r']) >= 0;
        return tirnak ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }
}
