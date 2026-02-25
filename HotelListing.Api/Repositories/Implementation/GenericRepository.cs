using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Extensions;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HotelListing.Api.Repositories.Implementation;

public class GenericRepository<T>(HotelListingDbContext db, IMapper mapper) : IGenericRepository<T> where T : class
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await db.Set<T>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PagedResult<T>> GetAllPagedAsync(PaginationParameters parameters)
    {
        return await db.Set<T>()
            .AsNoTracking()
            .ToPagedResultAsync(parameters);
    }

    public async Task<PagedResult<TResult>> GetAllPagedAsync<TResult>(PaginationParameters parameters)
    {
        var query = db.Set<T>()
        .AsNoTracking()
        .ProjectTo<TResult>(mapper.ConfigurationProvider);

        return await query.ToPagedResultAsync(parameters);
    }

    public async Task<PagedResult<TResult>> GetAllPagedAsync<TResult>(IQueryable<T> query, PaginationParameters parameters)
    {
        return await query
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(parameters);
    }

    public IQueryable<T> GetAllAsQueryable()
    {
        return db.Set<T>().AsNoTracking();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await db.Set<T>().FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await db.AddAsync(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        db.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);

        if (entity == null)
        {
            return;
        }

        db.Set<T>().Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await db.Set<T>().AnyAsync(predicate);
    }
}