using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string? EmailVerificationToken { get; set; }

        public bool EmailVerified { get; set; } = false;

        public string? ResetToken { get; set; }

        public bool RememberMe { get; set; }

        public string Role { get; set; } = "User";
        public string? Phone { get; set; } 

        public string? Address { get; set; } 

        public string? Note { get; set; }

        public string? ImagePath { get; set; } 

        [NotMapped]
        public IFormFile? Image { get; set; } 

        public ICollection<Transaction>? Transactions { get; set; }

    }

}
