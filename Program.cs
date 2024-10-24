using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using sftp_client;

class Startup
{
    static async Task Main(string[] args)
    {
        // Настройка логирования
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Настройка хоста и сервисов
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Настройка конфигурации для SFTP
                var sftpConfig = new SftpConfig
                {
                    Host = AppSettings.Host,
                    Port = AppSettings.Port,
                    UserName = AppSettings.UserName,
                    Password = AppSettings.Password,
                    Source = AppSettings.Source
                };

                // Регистрация сервисов
                services.AddSingleton(sftpConfig);
                services.AddTransient<IFileUploadService, FileUploadService>();
                services.AddTransient<IFileDownloadService, FileDownloadService>();
            })
            .Build();

        // Вызов сервисов
        var uploadService = host.Services.GetRequiredService<IFileUploadService>();
        await uploadService.UploadFilesAsync(CancellationToken.None);

        var downloadService = host.Services.GetRequiredService<IFileDownloadService>();
        await downloadService.DownloadFilesAsync(CancellationToken.None);

        Log.CloseAndFlush(); // Завершение логирования
    }
}
