using L.R._Gcaleka__Co.Data;
using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co.Models.ViewModels
{
    public class FileViewModel
    {
        [Key]
        public string FileId { get; set; }
        public string FileName { get; set; }
        public Clientele? Client { get; set; }
        public string ClienteleId { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public List<IFormFile>? Documents { get; set; } 
        public string CaseType { get; set; }
        public string? CaseUpdateNotification { get; set; }
        public virtual ApplicationUser Employee { get; set; }
        public string EmployeeId { get; set; }
        public CaseStatus CaseStatus { get; set; }
        public List<string>? FileUrls { get; set; }
        public DateTime? UploadedAt { get; set; } = DateTime.Now;

    }
}
