using System.ComponentModel.DataAnnotations;

namespace LibraryManagemenytSystem.Models
{
    public class BorrowingConfig
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        [Display(Name = "Loan Duration (Days)")]
        [Range(1, 365, ErrorMessage = "Loan duration must be between 1 and 365 days.")]
        public int LoanDurationDays { get; set; } = 14;

        [Required]
        [Display(Name = "Renewal Limit")]
        [Range(0, 10, ErrorMessage = "Renewal limit must be between 0 and 10.")]
        public int RenewalLimit { get; set; } = 2;

        [Required]
        [Display(Name = "Overdue Penalty Per Day ($)")]
        [Range(0, 100, ErrorMessage = "Penalty must be between $0 and $100.")]
        public decimal OverduePenaltyPerDay { get; set; } = 0.50m;

        [Required]
        [Display(Name = "Max Borrowable Items")]
        [Range(1, 10, ErrorMessage = "Max borrowable items must be between 1 and 10.")]
        public int MaxBorrowableItems { get; set; } = 3;
    }
}