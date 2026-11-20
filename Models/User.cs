using System.ComponentModel.DataAnnotations;

namespace BeachSAWeb.Models
{
    public class User
    {
        public int Id { get; set; }
        public string IdType { get; set; }
        public string FullName { get; set; }
        public int PhoneNumber { get; set; }
        public string Address { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Role { get; set; }  
    }
}
