namespace sftp_client
{
    public interface IFileDownloadService
    {
        Task DownloadFilesAsync(CancellationToken cancellationToken);
    }
}
