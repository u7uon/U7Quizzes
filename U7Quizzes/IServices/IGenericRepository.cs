namespace U7Quizzes.IServices
{
    public interface IGenericRepository<T> where T : class
    {

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task SaveChangesAsync(); 

    }
}
