using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class VerifyOTPViewModel
    {
        [Required(ErrorMessage = "OTP code is required.")]
        [StringLength(6, ErrorMessage = "OTP code must be 6 digits.", MinimumLength = 6)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OTP code must be numeric.")]
        public string OTP { get; set; }

        public string Email { get; set; }

        public bool IsOTPValid { get; set; }
    }
}
