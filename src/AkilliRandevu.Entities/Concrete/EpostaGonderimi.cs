using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Bir bildirimin e-posta kanalından gönderim kaydı (giden kutusu / outbox).
/// Bildirim 1-N EpostaGonderimi. Arka plan servisi "Bekliyor" durumundakileri gönderir.
/// </summary>
public class EpostaGonderimi : BaseEntity
{
    public int BildirimId { get; set; }
    public string Alici { get; set; } = null!;
    public GonderimDurumu Durum { get; set; } = GonderimDurumu.Bekliyor;
    public int DenemeSayisi { get; set; }
    public string? SonHata { get; set; }
    /// <summary>Servisin döndürdüğü mesaj kimliği (başarılı gönderimde).</summary>
    public string? DisMesajId { get; set; }
    public DateTime? GonderimTarihi { get; set; }

    public Bildirim Bildirim { get; set; } = null!;
}
