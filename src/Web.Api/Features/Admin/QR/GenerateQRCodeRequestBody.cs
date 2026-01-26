namespace Web.Api.Features.Admin.QR;

public record GenerateQRCodeRequestBody
{
    public string Value { get; init; }

    public int? PixelSize { get; init; }
}