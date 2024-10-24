namespace sftp_client
{
    public interface IFileUploadService
    {
        Task UploadFilesAsync(CancellationToken cancellationToken);
    }
}
