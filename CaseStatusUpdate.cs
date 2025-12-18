using Microsoft.EntityFrameworkCore;
using L.R._Gcaleka__Co.Data;
using L.R._Gcaleka__Co.Models;

namespace L.R._Gcaleka__Co
{
    public class CaseStatusUpdate : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CaseStatusUpdate> _logger;

        public CaseStatusUpdate(IServiceScopeFactory scopeFactory, ILogger<CaseStatusUpdate>logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using(var scope= _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var currentDateTime = DateTime.Now;

                        var pendingCase = await context.Files
                            .Where(f => f.CaseStatus == CaseStatus.Pending && f.OpeningDate <= currentDateTime)
                            .ToListAsync();

                        foreach (var caseStatus in pendingCase)
                        {
                            caseStatus.CaseStatus = CaseStatus.Pending;
                        }
                        await context.SaveChangesAsync();

                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "An error occured while updating case statuses");
                }
            }
        }
    }
}
