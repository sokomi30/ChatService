using System.ComponentModel.DataAnnotations;

namespace ChatService.Api.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 128 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}
