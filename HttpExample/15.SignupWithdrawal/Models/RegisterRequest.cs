using System.ComponentModel.DataAnnotations;

namespace SignupWithdrawal.Models;

public class RegisterRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    [MinLength(4)]
    public string Password { get; set; }

    public string Role { get; set; } = "User"; // 기본값
}
