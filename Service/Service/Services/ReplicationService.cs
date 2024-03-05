
using Service.Modules;

namespace Service.Services
{
    public class ReplicationService : IHostedService
    {
        private readonly IConfiguration _config;
        private Timer _checkTimer;

        public ReplicationService(IConfiguration config)
        {
            _config = config;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _checkTimer = new Timer(Replicate, null, 1000, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _checkTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void Replicate(object state)
        {
            try
            {
                // _roundRobinManager.CheckUrls(_settings().Urls);
                DbUtils.ReplicateData(_config);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _checkTimer?.Change(5 * 1000, Timeout.Infinite);
            }
        }
    }

}

