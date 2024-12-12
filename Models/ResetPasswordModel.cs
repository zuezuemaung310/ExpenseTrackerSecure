using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required, DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        //public string Email { get; set; }
    }
}
