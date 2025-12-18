using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class Suburb
    {
        [Key]
        public int SuburbId { get; set; }
        public string? SuburbName { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
    }
}
