using System.ComponentModel.DataAnnotations;

namespace ContactManager.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool Married { get; set; }
        [Required]
        [StringLength(50)]
        public string Phone { get; set; } = string.Empty;
        [Range(0, 999999999)]
        public decimal Salary { get; set; }
    }
}