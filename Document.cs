using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L.R._Gcaleka__Co.Models
{
    public class Document
    {
        [Key]
        public string DocumentId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual Files? Files{ get; set; }
        [Required]
        public string FileUrl { get; set; }
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
