using L.R._Gcaleka__Co.Data;
using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class Appointment
    {
        [Key]
        public string AppointmentId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public AppointmentType AppointmentType { get; set; }
        [Required]
        public string ContactNumber{get;set;}
        public bool ReminderSent { get; set; } = false;
        public string ClientId { get; set; }
        public virtual ApplicationUser LoggedInClient { get; set; }
        
    }
    public enum AppointmentType
    {
        Consult,
        CaseUpdate
    }
}
