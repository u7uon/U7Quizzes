using System.ComponentModel.DataAnnotations;
using U7Quizzes.Models;

namespace U7Quizzes.DTOs.Auth
{
    public class RegisterDTO
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password{ get; set; }

        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
