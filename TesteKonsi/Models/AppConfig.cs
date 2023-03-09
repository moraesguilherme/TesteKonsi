using Microsoft.Extensions.Configuration;
using System.IO;

namespace TesteKonsi.Models
{
    public class AppConfig
    {
        private static IConfigurationRoot configuration;

        static AppConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            configuration = builder.Build();
        }

        public static string Usuario { get; set; }
        public static string Senha { get; set; }

    }
}
