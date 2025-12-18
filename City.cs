using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int ProvinceId { get; set; }
        public Province Province { get; set; }
    }
}
