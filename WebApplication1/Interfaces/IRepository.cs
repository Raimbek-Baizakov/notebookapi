using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace WebApplication1.Interfaces
{
    public interface IRepository
    {
        Task SaveChangesAsync();
        Task<EntityEntry<T>> CreateAsync<T>(T model) where T : class;
        Task<EntityEntry<T>> UpdateAsync<T>(T model, int Id) where T : class, IEntity;
        Task<bool> DeleteAsync<T>(int Id) where T : class, IEntity;
        Task<IEnumerable<T>>GetAllAsync<T>(Expression<Func<T, bool>> filter = null)
            where T : class;
        Task<T> GetAsync<T>(Expression<Func<T, bool>> filter = null)
            where T : class;

    }
}
