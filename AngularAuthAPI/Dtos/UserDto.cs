using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AngularAuthAPI.Dtos
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }

    }
}
