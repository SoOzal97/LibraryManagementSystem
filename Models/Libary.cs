using System.ComponentModel.DataAnnotations;

namespace LibraryManagemenytSystem.Models
{
    public class Library
    {
        [Key]
        public int LibraryId { get; set; }

        [Required(ErrorMessage = "Library name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [Display(Name = "Library Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters.")]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Operating hours are required.")]
        [StringLength(200, ErrorMessage = "Operating hours cannot exceed 200 characters.")]
        [Display(Name = "Operating Hours")]
        public string? OperatingHours { get; set; }

        [Required(ErrorMessage = "Contact details are required.")]
        [StringLength(200, ErrorMessage = "Contact details cannot exceed 200 characters.")]
        [Display(Name = "Contact Details")]
        public string? ContactDetails { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Website")]
        [Url(ErrorMessage = "Invalid URL.")]
        public string? Website { get; set; }

        [Display(Name = "Date Established")]
        [DataType(DataType.Date)]
        public DateTime? DateEstablished { get; set; }
    }
}