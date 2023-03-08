using TesteKonsi.Models;

namespace TesteKonsi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Carrega as variáveis de ambiente a partir do arquivo appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configura o objeto AppConfig com as variáveis de ambiente
            AppConfig.Usuario = config["AppSettings:USUARIO"];
            AppConfig.Senha = config["AppSettings:SENHA"];

            // Chama o método para iniciar a API
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:5001");
                });
    }
}
