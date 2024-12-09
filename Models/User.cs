using System.ComponentModel.DataAnnotations;

namespace Mahmoud_Restaurant.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAdmin { get; set; } = false; // Default to false
    }

}
