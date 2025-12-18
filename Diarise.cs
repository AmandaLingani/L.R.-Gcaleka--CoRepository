using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models
{
    public class Diarise
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileId { get; set; }
        public virtual Files File { get; set; }
        [Required]
        public DateTime ScheduledDate { get; set; }
        [Required]
        public bool IsCourtDate { get; set; }
        public DateTime? CourtDate { get; set; } = DateTime.Now;
        public string? CourtName { get; set; } 
        public bool IsCompleted { get; set; }
        public bool IsUnreviewed { get; set; } = false;

    }
}
