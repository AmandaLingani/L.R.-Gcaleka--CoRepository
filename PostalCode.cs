using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class PostalCode
    {
        [Key]
        public int PostalCodeId { get; set; }
        public string? ZipCode { get; set; }
        public int SuburbId { get; set; }
        public Suburb Suburb { get; set; }
    }
}
