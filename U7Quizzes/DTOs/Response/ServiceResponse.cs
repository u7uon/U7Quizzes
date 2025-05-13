namespace U7Quizzes.DTOs.Share
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }
        public bool IsFailure => !IsSuccess;

        private ServiceResponse(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }


        public static ServiceResponse<T> Success(T value, string message) => new ServiceResponse<T>(true, value, message);
        public static ServiceResponse<T> Failure(string error) => new ServiceResponse<T>(false, default, error);
    }
}
