using HotelListing.Api.Models.Pagination;
using System.Linq.Expressions;

namespace HotelListing.Api.Repositories.Interface;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedResult<T>> GetAllPagedAsync(PaginationParameters queryParameters);
    Task<PagedResult<TResult>> GetAllPagedAsync<TResult>(PaginationParameters queryParameters);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}