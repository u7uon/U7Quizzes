namespace U7Quizzes.Caching
{
    public interface ICachingService
    {
        Task<T> Get<T>(string key);

        Task Set<T> (T data ,string key);


        Task Remove(string key);

        string GenerateKey<T>(T filter); 

    }
}
