using System.Linq.Expressions;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

/// <summary>
/// IRepository'nin EF Core ile gerçeklemesi. EF Core LINQ ifadelerini SQL'e çevirir (ORM).
/// </summary>
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public EfRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    // SELECT * FROM Tablo WHERE Id = @id LIMIT 1
    public Task<T?> GetByIdAsync(int id) => Set.FirstOrDefaultAsync(x => x.Id == id);

    // Sadece okuma yapıldığı için değişiklik takibi kapalı (AsNoTracking) -> daha hızlı
    public Task<List<T>> GetAllAsync() => Set.AsNoTracking().ToListAsync();

    public Task<List<T>> WhereAsync(Expression<Func<T, bool>> kosul) =>
        Set.AsNoTracking().Where(kosul).ToListAsync();

    public Task<bool> AnyAsync(Expression<Func<T, bool>> kosul) => Set.AnyAsync(kosul);

    // INSERT INTO Tablo (...) VALUES (...)
    public async Task<T> AddAsync(T entity)
    {
        await Set.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    // UPDATE Tablo SET ... WHERE Id = @id
    public async Task UpdateAsync(T entity)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync();
    }

    // DELETE FROM Tablo WHERE Id = @id
    public async Task DeleteAsync(T entity)
    {
        Set.Remove(entity);
        await Context.SaveChangesAsync();
    }
}
