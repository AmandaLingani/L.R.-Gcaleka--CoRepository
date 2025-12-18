using L.R._Gcaleka__Co.Data;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using L.R._Gcaleka__Co;

namespace L.R._Gcaleka__Co
{
    public class UnreviewedFilesService : BackgroundService
    {
        private readonly UnreviewedFiles _unreviewdFiles;
        private readonly IServiceScopeFactory _scopeFactory;

        public UnreviewedFilesService(IServiceScopeFactory scopeFactory)
        {
            _unreviewdFiles = new UnreviewedFiles();
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now.Hour == 23 && now.Minute == 59)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var cutoff = DateTime.Today;

                        var missedReviews = await context.Diarise
                            .Where(d => d.ScheduledDate < cutoff && d.IsCompleted == false && d.IsUnreviewed == false)
                            .ToListAsync();

                        foreach(var entry in missedReviews)
                        {
                            entry.IsUnreviewed = true;
                        }

                        await context.SaveChangesAsync();
                    }

                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
