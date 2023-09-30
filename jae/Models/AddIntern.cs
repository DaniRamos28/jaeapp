

using System.ComponentModel.DataAnnotations;

namespace jae.Models
{
    public class addIntern
    {
        public DateTime? Response { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        public string? Course { get; set; }

        [Required(ErrorMessage = "School is required.")]
        public string? School { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "Phone number must be 11 digits and start with 09.")]
        [StringLength(11, ErrorMessage = "Phone number must be 11 digits.", MinimumLength = 11)]
        public string? Number { get; set; }

        [Required(ErrorMessage = "Please indicate if you are willing to work from home.")]
        public string? wfhft { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? Datestart { get; set; }

        [Required(ErrorMessage = "Render Hours Needed input is required.")]
        public string? Renderhrs { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Resume is required.")]
        public string? Resume { get; set; }

        public string? Status { get; set; }
    }
}
