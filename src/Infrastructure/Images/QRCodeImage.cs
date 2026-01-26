using QRCoder;

namespace Infrastructure.Images;

public class QRCodeImage
{
    private const int DefaultPixelSize = 20;

    private readonly string _value;

    public QRCodeImage(
        string value)
    {
        _value = value;
    }

    public byte[] AsBytes(
        int? pixelPerModule = null)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(_value, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(pixelPerModule ?? DefaultPixelSize);
    }

    public string AsBase64(
        int? pixelPerModule = null)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(_value, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new Base64QRCode(qrCodeData);

        return qrCode.GetGraphic(pixelPerModule ?? DefaultPixelSize);
    }
}