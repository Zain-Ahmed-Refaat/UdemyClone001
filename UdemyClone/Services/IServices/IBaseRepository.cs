namespace UdemyClone.Services.IServices
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> SearchAsync(string keyword);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
