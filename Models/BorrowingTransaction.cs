using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagemenytSystem.Models
{
    public class BorrowingTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Display(Name = "Borrow Date")]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; } = "Borrowed";

        [Display(Name = "Fine Amount")]
        [DataType(DataType.Currency)]
        public decimal FineAmount { get; set; } = 0;

        [Display(Name = "Renewed Count")]
        public int RenewedCount { get; set; } = 0;

        [Display(Name = "Is Renewed")]
        public bool IsRenewed { get; set; } = false;
    }
}