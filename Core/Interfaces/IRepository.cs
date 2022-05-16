using RecruitmentApp.Core.Models;

namespace RecruitmentApp.Core.Interfaces
{
    public interface IRepository<TEntity>
    {
        void Add(TEntity entity);

        void Add(IEnumerable<TEntity> entities);

        void Delete(TEntity entity);

        void Delete(IEnumerable<TEntity> entities);

        void Delete(object id);

        TEntity? Find(object id);

        Task<TEntity?> FindAsync(object id);

        IQueryable<TEntity> Get();

        void Update(TEntity entity);

        void Update(IEnumerable<TEntity> entities);

        void Save();

        Task SaveAsync();
    }
}