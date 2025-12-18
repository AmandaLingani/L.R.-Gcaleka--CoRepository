using System.ComponentModel.DataAnnotations;
using L.R._Gcaleka__Co.Data;

namespace L.R._Gcaleka__Co.Models
{
    public class ClientDocuments
    {
        [Key]
        public string ClientDocumentId { get; set; } = Guid.NewGuid().ToString(); //This is my primary key the identifier yea? 
       
        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.Now; //The date the documents are submitted 
        public bool IsReceived { get; set; } = false; //This applies to the staff member side
        [Required]
        public string ClientId { get; set; } //Logged in user
        public virtual ApplicationUser Clients { get; set; } //This is whoever's logged in
        public string? CaseFileId { get; set; } //This is for file assigning and reviewing
        public virtual Files? CaseFile { get; set; } //This too
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending; //This reflects on the client side but depending on what happens on the staff side
       
        public DateTime? ReviewedAt { get; set; } //THIS TOO
        public string? ReviewedById { get; set; } //Logged In Employee
        public virtual ApplicationUser? ReviewedBy { get; set; }
        public List<ClientFiles>? ClientFiles { get; set; } = new List<ClientFiles>();
        
    }
    public enum DocumentStatus
    {
        Pending,
        Reviewed,
        Rejected,
    }
}
