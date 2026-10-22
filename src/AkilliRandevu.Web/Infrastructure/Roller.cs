using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Web.Infrastructure;

/// <summary>[Authorize(Roles = ...)] içinde kullanılan rol adları. Rol enum'undan türetilir.</summary>
public static class Roller
{
    public const string Hasta = nameof(Rol.Hasta);
    public const string Doktor = nameof(Rol.Doktor);
    public const string Yonetici = nameof(Rol.Yonetici);

    /// <summary>Personel: doktor veya yönetici (virgül = "veya").</summary>
    public const string Personel = Doktor + "," + Yonetici;
}
