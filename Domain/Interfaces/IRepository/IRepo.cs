
using System.Linq.Expressions;



namespace Domain.Interfaces.IRepository
{
    public interface IRepo<T> where T : class
    {
        Task SaveChangesAsync();

        IQueryable<T> GetAll();
        // Get operations
        Task<T> GetByIdAsync(Guid id);
        

        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Pagination
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null ,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);

        // Add operations
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        // Update operations
        Task UpdateAsync(T entity);
        Task<T> PatchAsync(Guid id, Action<T> patchAction);

        // Delete operations
        Task DeleteAsync(Guid id);
        Task DeleteAsync(T entity);

        // Check existence
        Task<bool> ExistsAsync(Guid id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Count
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        IQueryable<T> Sort(IQueryable<T> query, string columnName, bool isAscending);
        IQueryable<T> Search(Expression<Func<T, bool>> search);
    }
}
