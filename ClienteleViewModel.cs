using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models.ViewModels
{
    public class ClienteleViewModel
    {
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
        [Required, EmailAddress]
        public string? EmailAddress { get; set; }

        public int SelectedProvinceId { get; set; }
        public int SelectedCityId { get; set; }
        public int SelectedPostalCodeId { get; set; }
        public int SelectedSuburbId { get; set; }
        public string? StreetAddress { get; set; }
        public ClientType ClientType { get; set; }
        public string? AspNetUserId { get; set; }
        public string EmployeeId { get; set; }
    }
}
