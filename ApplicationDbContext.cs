using L.R._Gcaleka__Co.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;

namespace L.R._Gcaleka__Co.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext()
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Document>()
                .HasOne(d => d.Files)
                .WithMany(f => f.Documents)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClientFiles>()
                .HasOne(cf => cf.ClientDocument)
                .WithMany(cd => cd.ClientFiles)
                .HasForeignKey(cf => cf.ClientDocumentId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<City>()
                .HasOne(c => c.Province)
                .WithMany()
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Suburb>()
                .HasOne(s => s.City)
                .WithMany()
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostalCode>()
                .HasOne(p => p.Suburb)
                .WithMany()
                .HasForeignKey(p => p.SuburbId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Clientele>()
                .HasOne(c => c.Province)
                .WithMany()
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Clientele>()
               .HasOne(c => c.City)
               .WithMany()
               .HasForeignKey(c => c.CityId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Clientele>()
               .HasOne(c => c.Suburb)
               .WithMany()
               .HasForeignKey(c => c.SuburbId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Clientele>()
               .HasOne(c => c.PostalCode)
               .WithMany()
               .HasForeignKey(c => c.PostalCodeId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientDocuments>()
                .HasOne(cd => cd.Clients)
                .WithMany()
                .HasForeignKey(cd => cd.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientDocuments>()
                .HasOne(cd => cd.CaseFile)
                .WithMany()
                .HasForeignKey(cd => cd.CaseFileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientDocuments>()
                .HasOne(cd => cd.ReviewedBy)
                .WithMany()
                .HasForeignKey(cd => cd.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.LoggedInClient)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<Province> Province { get; set; }
        public DbSet<Suburb> Suburb { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<PostalCode> PostalCode { get; set; }
        public DbSet<Clientele> Clientele { get; set; }
        public DbSet<Diarise> Diarise { get; set; }
        public DbSet<ClientDocuments> ClientDocuments { get; set; }
        public DbSet<ClientFiles> ClientFiles { get; set; }

    }

}
