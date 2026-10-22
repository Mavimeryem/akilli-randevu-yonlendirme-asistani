using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Business.Abstract;

public interface IKullaniciService
{
    /// <summary>Yeni hasta kaydı: Kullanici (Rol = Hasta) ve Hasta profili birlikte oluşturulur.</summary>
    Task<KayitSonucu> HastaKaydetAsync(KayitIstegi istek);

    Task<GirisSonucu> GirisYapAsync(string eposta, string sifre);

    Task<List<Kullanici>> TumKullanicilarAsync();
}
