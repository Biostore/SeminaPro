using SeminaPro.Services.Interfaces;

namespace SeminaPro.Services.Implementations
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Vérifier toutes les heures

        public ReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service de rappel démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var reminderService = scope.ServiceProvider
                            .GetRequiredService<IReminderService>();

                        // Envoyer les rappels de séminaires
                        await reminderService.SendSeminarRemindersAsync();

                        // Nettoyer les anciennes notifications (une fois par jour à minuit)
                        if (DateTime.Now.Hour == 0 && DateTime.Now.Minute < 5)
                        {
                            await reminderService.CleanupOldNotificationsAsync();
                        }

                        _logger.LogInformation("Tâche de rappel exécutée avec succès");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'exécution de la tâche de rappel");
                }

                // Attendre avant la prochaine exécution
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Service de rappel arrêté");
        }
    }
}
