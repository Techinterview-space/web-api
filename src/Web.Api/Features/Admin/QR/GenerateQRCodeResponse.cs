namespace Web.Api.Features.Admin.QR;

public record GenerateQRCodeResponse
{
    public GenerateQRCodeResponse(
        string imageBase64)
    {
        ImageBase64 = imageBase64;
    }

    public string ImageBase64 { get; }
}