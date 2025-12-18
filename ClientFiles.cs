using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L.R._Gcaleka__Co.Models
{
    public class ClientFiles
    {
        [Key]
        public string ClientFileId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FileUrl{ get; set; }
        public string ClientDocumentId { get; set; }
        [ForeignKey("ClientDocumentId")]
        public virtual ClientDocuments ClientDocument { get; set; }
        [Required]
        public DateTime SubmissionDate = DateTime.Now;
    }
}
