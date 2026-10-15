using System.Linq.Expressions;
using AkilliRandevu.Entities.Common;

namespace AkilliRandevu.DataAccess.Abstract;

/// <summary>
/// Tüm tablolar için ortak CRUD işlemleri (Create, Read, Update, Delete).
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> WhereAsync(Expression<Func<T, bool>> kosul);
    Task<bool> AnyAsync(Expression<Func<T, bool>> kosul);

    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
