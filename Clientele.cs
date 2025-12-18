using System.ComponentModel.DataAnnotations;
using L.R._Gcaleka__Co.Data;

namespace L.R._Gcaleka__Co.Models
{
    public class Clientele
    {
        [Key]
        public string ClienteleId { get; set; }
        [RequiredIf("ClientType", ClientType.Individual, ErrorMessage = "First Name is required.")]
        public string? FirstName { get; set; }
        [RequiredIf("ClientType", ClientType.Individual, ErrorMessage = "Last Name is required.")]
        public string? LastName { get; set; }
        [RequiredIf("ClientType", ClientType.Individual, ErrorMessage = "Identity Number is required.")]
        public string? IdentityNumber { get; set; }
        [RequiredIf("ClientType", ClientType.Individual, ErrorMessage = "Date of Birth is required.")]
        public DateTime? DateOfBirth { get; set; }
        [RequiredIf("ClientType", ClientType.Company, ErrorMessage = "Company Name is required.")]
        public string? CompanyName { get; set; }
        [Required]
        public string? ContactNumber { get; set; }
        public string? AltContactNumber { get; set; }
        [Required,EmailAddress]
        public string? EmailAddress { get; set; }

        public int ProvinceId { get; set; }
        public int CityId { get; set; }
        public int PostalCodeId { get; set; }
        public int SuburbId { get; set; }

        public Province Province { get; set; }
        public City City { get; set; }
        public Suburb Suburb { get; set; }
        public PostalCode PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public string? AspNetUserId { get; set; }
        public ClientType ClientType { get; set; }
        public virtual ApplicationUser Employee { get; set; }
        public string EmployeeId { get; set; }
    }

    public enum ClientType
    {
        Individual,
        Company
    }
}
