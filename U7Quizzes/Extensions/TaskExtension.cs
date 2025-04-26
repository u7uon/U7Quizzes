namespace U7Quizzes.Extensions
{
    public static class TaskExtension
    {
        public static async Task<T> ConfigureAwaitFalse<T>(this Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }

        public static async Task ConfigureAwaitFalse(this Task task)
        {
            await task.ConfigureAwait(false);
        }
    }
}
