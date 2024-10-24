using System.Globalization;
using System.Net;
using System.Text;
using ftp_client;
using Renci.SshNet;
using Renci.SshNet.Sftp;

class Program
{
    public static async Task Main(string[] args) // Изменено на Task
    {
        Console.WriteLine("Программа запущена.");
        await DownloadFilesFromRemoteDirectoryAsync(CancellationToken.None); // Используем CancellationToken.None
    }

    private static async Task DownloadFilesFromRemoteDirectoryAsync(CancellationToken cancellationToken) // Изменено на static
    {
        var config = new SftpConfig
        {
            Host = AppSettings.BeltelSftp.Host,
            Port = AppSettings.BeltelSftp.Port,
            UserName = CryptographyHelper.DecryptFromBase64(AppSettings.BeltelSftp.UserName, Encoding.ASCII),
            Password = CryptographyHelper.DecryptFromBase64(AppSettings.BeltelSftp.Password, Encoding.ASCII),
            Source = AppSettings.BeltelSftp.Source
        };

        using (var client = new SftpClient(config.Host, config.Port, config.UserName, config.Password))
        {
            try
            {
                await client.ConnectAsync(cancellationToken);
                var files = client.ListDirectory(config.Source);
                var counter = 0;

                foreach (var file in files)
                {
                    counter++;

                    if (!file.IsDirectory && !file.IsSymbolicLink)
                    {
                        var isParseSuccess = DateTime.TryParseExact(file.Name.Substring(0, 10), "yyyyMMddHH", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime parsedDataTime);

                        if (isParseSuccess)
                        {
                            var elements = DownloadFile(client, file);
                        }
                        else
                        {
                            throw new Exception($"Ошибка преобразования названия файла в тип DateTime.{file.FullName}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Disconnect();
                client.Dispose();
            }
        }
    }

    private static string DownloadFile(SftpClient client, ISftpFile file) // Изменено на static
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                client.DownloadFile(file.FullName, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var textReader = new StreamReader(memoryStream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}