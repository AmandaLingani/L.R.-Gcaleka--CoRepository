using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using L.R._Gcaleka__Co.Data;


namespace L.R._Gcaleka__Co.Models
{
    public class Files
    {
        [Key]
        public string FileId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FileName { get; set; }
        public Clientele? Client { get; set; }
        [Required]
        public string ClienteleId { get; set; }
        [Required]
        public DateTime OpeningDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public bool IsClosed { get; set; } = false;
        public string? CloseNote { get; set; }
        public List<Document>? Documents { get; set; } = new List<Document>();
        [Required]
        public string CaseType { get; set; }
        public string? CaseUpdateNotification { get; set; }
        [Required]
        public CaseStatus CaseStatus { get; set; }
        public string? EmployeeId { get; set; }
        public virtual ApplicationUser? Employee { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public virtual List<Diarise> DiarisingEntries { get; set; } = new List<Diarise>();
    }
    public enum CaseStatus
    {
        Pending,
        Closed
    }
}
