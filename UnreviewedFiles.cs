using L.R._Gcaleka__Co.Data;

namespace L.R._Gcaleka__Co
{
    public class UnreviewedFiles
    {
        public void MarkUnreviewedFiles()
        {
            using (var context = new ApplicationDbContext())
            {
                var today = DateTime.UtcNow.Date;

                var unreviewedFiles = context.Diarise
                    .Where(d=>d.ScheduledDate < today && d.IsCompleted==false)
                    .ToList();

                foreach (var file in unreviewedFiles)
                {
                    file.IsUnreviewed = true;
                    Console.WriteLine($"Unreviewed Files:{file.File.FileName}");
                }
                context.SaveChanges();
            }
        }
    }
}
