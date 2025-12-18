using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class Province
    {
        [Key]
        public int ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        
    }
}
