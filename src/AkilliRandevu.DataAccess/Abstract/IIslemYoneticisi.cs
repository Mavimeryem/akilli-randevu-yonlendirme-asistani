namespace AkilliRandevu.DataAccess.Abstract;

/// <summary>
/// Birden çok veritabanı işlemini tek bir transaction içinde çalıştırır: hepsi başarılı olursa
/// kalıcı olur (COMMIT), biri hata verirse hepsi geri alınır (ROLLBACK).
/// </summary>
public interface IIslemYoneticisi
{
    Task<T> TransactionIcindeAsync<T>(Func<Task<T>> islem);
}
