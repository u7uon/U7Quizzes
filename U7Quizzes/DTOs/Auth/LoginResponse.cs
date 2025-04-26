using U7Quizzes.DTOs.Share;

namespace U7Quizzes.DTOs.Auth
{
    public class LoginResponse : ServiceResponse
    {
        public TokenDTO Token { get; set; }

    }
}
