using L.R._Gcaleka__Co.Data;
using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models.ViewModels
{
    public class AppointmentViewModel
    {
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public AppointmentType AppointmentType { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public DateTime Time { get; set; }
        public string ClientId { get; set; }
        public virtual ApplicationUser Clientele{ get; set; }
    }
}
