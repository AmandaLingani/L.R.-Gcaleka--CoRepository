using L.R._Gcaleka__Co.Data;

namespace L.R._Gcaleka__Co.Models.ViewModels
{
    public class ClientDocumentViewModel
    {
        public List<string>? FileUrl { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public string ClientId { get; set; }
        public virtual ApplicationUser Clients { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public List<IFormFile>? ClientFiles { get; set; }
       
    }
}
