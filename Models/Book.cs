using System.ComponentModel.DataAnnotations;

namespace LibraryManagemenytSystem.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters.")]
        [Display(Name = "Genre")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "ISBN is required.")]
        [RegularExpression(@"^\d{3}-\d{10}$", ErrorMessage = "ISBN format must be: 978-XXXXXXXXXX")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Date Added")]
        [DataType(DataType.Date)]
        public DateTime DateAdded { get; set; } = DateTime.Now;
        [Display(Name = "Total Copies")]
        [Range(1, 10, ErrorMessage = "Total copies must be between 1 and 10.")]
        public int TotalCopies { get; set; } = 1;

        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; } = 1;
    }
}