namespace Infrastructure.Services.Files;

public interface IPdf
{
    FileData Render(
        string htmlContent,
        string filename,
        string contentType);
}