namespace Infrastructure.Services.Files;

#pragma warning disable SA1313
public record FileData(
    byte[] Data,
    string Filename,
    string ContentType)
{
    public const string PDfContentType = "application/pdf";
}
#pragma warning restore SA1313