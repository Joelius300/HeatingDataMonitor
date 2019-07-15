using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataHandler;
using DataHandler.Exceptions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using DataHistory;

namespace WebCore
{
    public class Program
    {
        public static Config Config { get; private set; }

        public static void Main(string[] args)
        {
            if (!TrySetConfig())
            {
                Console.WriteLine("Bitte beheben Sie den/die Fehler und starten Sie die App erneut.");
                Console.ReadKey(true);
                return;
            }

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<HeatingDataContext>();

                db.Database.EnsureCreated();
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();

                    string localhost = $"http://localhost:{Config.Port}";
                    if (Config.HostIP == "localhost" || Config.HostIP == "127.0.0.1") {
                        webBuilder.UseUrls(localhost);
                    }
                    else {
                        webBuilder.UseUrls(localhost, $"http://{Config.HostIP}:{Config.Port}");
                    }
                });

        private static bool TrySetConfig()
        {
            try
            {
                Config = Config.Deserialize();
                Config.CheckConfig();

                return true;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Die Konfigurationsdatei ({e.FileName}) konnte nicht gefunden werden.");
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"Es gab ein Fehler beim Laden der Konfiguration. Wahrscheinlich wurde ein Buchstabenwert in ein Zahlenfeld angegeben.");
            }
            catch (InvalidIPAddressException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NonExistingSerialPortException e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }
    }
}
