using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_0
using PeterKottas.DotNetCore.WindowsService;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
#endif

namespace Lighthouse
{
    public partial class Program
    {
#if NETCOREAPP2_0
        public static void Main(string[] args)
        {
            if (args.Length == 0) LighthouseApplication.Run(args);

            ServiceRunner<WindowsServiceWrapper>.Run(config =>
            {
                config.SetName("Lighthouse");
                config.SetDisplayName("Lighthouse");
                config.SetDescription("Performs cluster management across multiple Actor Systems.");

                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) =>
                    {
                        return new WindowsServiceWrapper(controller);
                    });

                    serviceConfig.OnStart((service, extraArguments) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnInstall(service =>
                    {
                        Console.WriteLine("Service {0} installed", name);
                    });

                    serviceConfig.OnUnInstall(service =>
                    {
                        Console.WriteLine("Service {0} uninstalled", name);
                    });

                    serviceConfig.OnPause(service =>
                    {
                        Console.WriteLine("Service {0} paused", name);
                    });

                    serviceConfig.OnContinue(service =>
                    {
                        Console.WriteLine("Service {0} continued", name);
                    });

                    serviceConfig.OnShutdown(service =>
                    {
                        Console.WriteLine("Service {0} shutdown", name);
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });
        }

        public class WindowsServiceWrapper : IMicroService
        {
            private readonly IMicroServiceController _controller;

            public WindowsServiceWrapper(IMicroServiceController controller)
            {
                _controller = controller;
            }

            public void Start()
            {
                LighthouseApplication.Run(new string[] { }, true);
            }

            public void Stop()
            {
                LighthouseApplication.Stop();
            }
        }

        public static class LighthouseApplication
        {
            private static LighthouseService _lighthouseService;

            public static void Run(string[] args, bool runAsService = false)
            {
                _lighthouseService = new LighthouseService();
                _lighthouseService.Start();

                if (runAsService) return;

                Console.WriteLine("Press Control + C to terminate.");
                Console.CancelKeyPress += async (sender, eventArgs) =>
                {
                    await _lighthouseService.StopAsync();
                };
                _lighthouseService.TerminationHandle.Wait();
            }

            public static async void Stop()
            {
                await _lighthouseService.StopAsync();
                _lighthouseService.TerminationHandle.Wait();
            }
        }
#endif
    }
}