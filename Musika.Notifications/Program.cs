using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Musika.Notifications
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                LogManager.GetCurrentClassLogger().Error(eventArgs.ExceptionObject as Exception);
            };

            HostFactory.Run(x =>
            {
                x.Service<BonanzaService>(s =>
                {
                    s.WhenStarted(tc => tc.Start())
                        .BeforeStartingService(a => a.RequestAdditionalTime(TimeSpan.FromMinutes(3)));
                    s.WhenStopped(tc =>
                    {
                        tc.Stop();
                        ServiceFactory.Release(tc);
                    });
                    s.ConstructUsing(sf => ServiceFactory.Create<BonanzaService>());
                });

                x.SetDisplayName("VacationRoost.Bonanza.Service");
                x.SetServiceName("VacationRoost.Bonanza.Service");
                x.SetDescription("Hosts the Map Service");
                x.RunAsLocalSystem();

                x.UseNLog();
            });
        }
    }
}
