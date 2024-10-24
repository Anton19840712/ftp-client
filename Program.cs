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



//class Program
//{
//    public static byte[] DownloadFile(string url, string filePath, string user, string password)
//    {
//        try
//        {
//            // Отключаем проверку сертификатов (для тестовой среды)
//            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

//            // Формируем корректный URL для FTP
//            var ftpServerUrl = new Uri(new Uri(url), filePath);
//            var request = (FtpWebRequest)WebRequest.Create(ftpServerUrl);

//            // Указываем метод загрузки файла
//            request.Method = WebRequestMethods.Ftp.DownloadFile;

//            // Указываем учетные данные для авторизации
//            request.Credentials = new NetworkCredential(user, password);

//            // Пробуем изменить режим на активный/пассивный
//            request.UsePassive = true; // Пассивный режим передачи данных

//            // Включаем FTPS (SSL для команд управления)
//            request.EnableSsl = true; // Включаем TLS

//            // Отключаем шифрование канала данных (если сервер его не поддерживает)
//            request.EnableSsl = false; // Попробуйте без шифрования канала данных

//            // Получаем ответ от сервера и читаем поток данных
//            using (var response = (FtpWebResponse)request.GetResponse())
//            using (var responseStream = response.GetResponseStream())
//            using (var memoryStream = new MemoryStream())
//            {
//                if (responseStream != null)
//                {
//                    responseStream.CopyTo(memoryStream);
//                    Console.WriteLine($"Статус ответа: {response.StatusDescription}");
//                    return memoryStream.ToArray();
//                }
//                else
//                {
//                    throw new Exception("Не удалось получить поток данных с сервера.");
//                }
//            }
//        }
//        catch (WebException ex)
//        {
//            Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
//            if (ex.Response is FtpWebResponse ftpResponse)
//            {
//                Console.WriteLine($"Статус FTP: {ftpResponse.StatusDescription}");
//            }
//            throw;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Общая ошибка: {ex.Message}");
//            throw;
//        }
//    }

//    // Отключаем проверку сертификата
//    public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
//    {
//        // Принимаем любой сертификат (для тестирования)
//        return true;
//    }

//    static void Main(string[] args)
//    {
//        string url = "ftp://127.0.0.1"; // URL FTP-сервера
//        string filePath = "/documents1/test.html"; // Путь к файлу на FTP-сервере
//        string user = "antontest"; // Имя пользователя для входа на FTP-сервер
//        string password = "13"; // Пароль для входа на FTP-сервер

//        try
//        {
//            // Загрузка файла
//            byte[] fileData = DownloadFile(url, filePath, user, password);

//            // Сохранение загруженного файла на локальный диск
//            File.WriteAllBytes(@"C:\path\to\downloaded\file.txt", fileData);

//            Console.WriteLine("Файл успешно загружен.");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Ошибка в процессе работы программы: {ex.Message}");
//        }
//    }
//}




//static void Main(string[] args)
//{

//    DownloadFile()


//// Создаем FTP-клиент
//FtpClient client = new FtpClient("127.0.0.1", "antontest", "13");
//client.Port = 21; // Порт 21 для FTP

//// Указываем явное шифрование (FTPS)
//client.EncryptionMode = FtpEncryptionMode.Explicit;
//client.DataConnectionType = FtpDataConnectionType.PASV; // или FtpDataConnectionType.PASV

//// Устанавливаем проверку сертификата (если требуется FTPS)
//client.ValidateCertificate += new FtpSslValidation((control, e) =>
//{
//    e.Accept = true; // Принимаем любой сертификат (удобно для тестов)
//});

//try
//{
//    // Подключаемся к FTP серверу
//    client.Connect();

//    // Пример загрузки файла на сервер
//    //client.UploadFile(@"C:\Documents1\test2.html", "/documents1/test2.html");

//    // Пример скачивания файла с сервера
//    client.DownloadFile(@"C:\Documents1\test32.html", "/remote/test.html");

//    Console.WriteLine("Файлы успешно загружены и скачаны.");
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"Ошибка: {ex.Message}");
//}
//finally
//{
//    // Отключаемся от сервера
//    if (client.IsConnected)
//    {
//        client.Disconnect();
//    }
//}
//}
// }

