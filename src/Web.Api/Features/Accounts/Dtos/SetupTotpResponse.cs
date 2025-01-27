using Domain.Entities.Users;
using Infrastructure.Images;

namespace Web.Api.Features.Accounts.Dtos;

public record SetupTotpResponse
{
    public const string Issuer = "Techinterviewer.space";

    public SetupTotpResponse(
        User user)
    {
        TotpMfaUrl = $"otpauth://totp/{Issuer}:{user.Email}?secret={user.TotpSecret}&issuer={Issuer}";
        TotpSetupQRBase64 = new QRCodeImage(TotpMfaUrl).ToString();
    }

    public string TotpMfaUrl { get; set; }

    public string TotpSetupQRBase64 { get; set; }
}