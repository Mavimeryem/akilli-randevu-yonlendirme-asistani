using System.ComponentModel.DataAnnotations;

namespace AkilliRandevu.Web.Models;

public class BelirtiSecimi
{
    public int BelirtiId { get; set; }
    public string Ad { get; set; } = "";
    public bool Secili { get; set; }

    [Range(1, 10, ErrorMessage = "Şiddet 1 ile 10 arasında olmalıdır.")]
    public int Siddet { get; set; } = 5;
}

public class BelirtiGirisViewModel
{
    public List<BelirtiSecimi> Belirtiler { get; set; } = [];

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    [Display(Name = "Eklemek istedikleriniz (isteğe bağlı)")]
    public string? Aciklama { get; set; }
}
