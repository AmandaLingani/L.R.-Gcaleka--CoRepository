using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using L.R._Gcaleka__Co.Models;

namespace L.R._Gcaleka__Co.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? EmployeeNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
