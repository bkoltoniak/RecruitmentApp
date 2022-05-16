using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Models;

namespace RecruitmentApp.Data.Repositories;

public class ExchangeRateRepository : IRepository<ExchangeRate>
{

    protected readonly AppDbContext _context;

    public ExchangeRateRepository(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    public ExchangeRate? Find(object id)
    {
        return _context.Set<ExchangeRate>().Find(id);
    }

    public async Task<ExchangeRate?> FindAsync(object id)
    {
        return await _context.Set<ExchangeRate>().FindAsync(id);
    }

    public IQueryable<ExchangeRate> Get()
    {
        return _context.Set<ExchangeRate>();
    }

    public void Add(ExchangeRate entity)
    {
        _context.Add(entity);
    }

    public void Add(IEnumerable<ExchangeRate> entities)
    {
        _context.AddRange(entities);
    }

    public void Delete(ExchangeRate entity)
    {
        _context.Remove(entity);
    }

    public void Delete(object id)
    {
        var entity = Find(id);

        if (entity is not null)
        {
            Delete(entity);
        }
    }

    public void Delete(IEnumerable<ExchangeRate> entities)
    {
        _context.RemoveRange(entities);
    }

    public void Update(ExchangeRate entity)
    {
        _context.Update(entity);
    }

    public void Update(IEnumerable<ExchangeRate> entities)
    {
        _context.UpdateRange(entities);
    }

    public void Save()
    {
        _context.SaveChanges();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
