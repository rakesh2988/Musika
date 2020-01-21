using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Quartz;

namespace Musika.Notifications
{
    public class NotificationService
    {
        private IServiceBus _bus;
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        private readonly IScheduler _scheduler;
        private readonly IScheduleFactory _scheduleFactory;
        private readonly IServiceFactory _serviceFactory;

        public NotificationService(HttpSelfHostConfiguration config, IScheduler scheduler, IScheduleFactory scheduleFactory, IServiceFactory serviceFactory)
        {
            _config = config;
            _scheduler = scheduler;
            _scheduleFactory = scheduleFactory;
            _serviceFactory = serviceFactory;
        }

        public void Start()
        {
            AutoMapperConfiguration.Configure();

            _scheduleFactory.AddJobSchedules(_scheduler);
            _scheduler.Start();

            _bus = _serviceFactory.Create<IServiceBus>();

            _server = new HttpSelfHostServer(_config);
            _server.OpenAsync().ContinueWith(delegate
            {
                LogManager.GetCurrentClassLogger().Debug("Started WebAPI Service at {0}", _config.BaseAddress);
            });
        }

        public void Stop()
        {
            _bus.Dispose();
            _bus = null;

            _server.CloseAsync().Wait();
            _server.Dispose();

            _scheduler.Shutdown(false);
        }
    }
}
