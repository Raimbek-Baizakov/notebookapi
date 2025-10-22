using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using WebApplication1.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication1
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _db;

        public Repository(AppDbContext db)
        {
            _db = db;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<EntityEntry<T>> CreateAsync<T>(T model) where T : class
        {
            return await _db.Set<T>().AddAsync(model);
        }

        public async Task<bool> DeleteAsync<T>(int Id) where T : class, IEntity
        {
            if (Id > 0)
            {
                var oldModel = await _db.Set<T>()
                    .FirstOrDefaultAsync(f => f.Id == Id);

                if (oldModel != null)
                {
                    _db.Set<T>().Remove(oldModel);
                    return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> filter = null) where T : class
        {
            IQueryable<T> query = _db.Set<T>(); //_db.Teams;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> filter = null) where T : class
        {
            IQueryable<T> query = _db.Set<T>(); //_db.Teams;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }


        public async Task<EntityEntry<T>> UpdateAsync<T>(T model, int Id) where T : class, IEntity
        {
            if (Id > 0)
            {
                var oldModel = await _db.Set<T>().FirstOrDefaultAsync(f => f.Id == Id);

                if (oldModel != null)
                {
                    model.Id = Id;
                    return _db.Set<T>().Update(model);
                }
            }
            return null;
        }
    }
}
