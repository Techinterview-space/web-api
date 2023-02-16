using System;
using Domain.Emails.Services;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Moq;

namespace TestUtils;

public class EmailServiceMock : Mock<IEmailService>
{
    public EmailServiceMock()
    {
        Setup(x => x.InvitationAsync(
            It.IsAny<Organization>(),
            It.IsAny<User>(),
            It.IsAny<User>()));

        Setup(x => x.InvitationDeclinedAsync(
            It.IsAny<Organization>(),
            It.IsAny<User>(),
            It.IsAny<User>()));

        Setup(x => x.InvitationAcceptedAsync(
            It.IsAny<Organization>(),
            It.IsAny<User>(),
            It.IsAny<User>()));
    }

    public void VerifyInvitationDeclined(
        Func<Times> times)
    {
        Verify(
            x => x.InvitationDeclinedAsync(
                It.IsAny<Organization>(),
                It.IsAny<User>(),
                It.IsAny<User>()),
            times);
    }

    public void VerifyInvitationAccepted(
        Func<Times> times)
    {
        Verify(
            x => x.InvitationAcceptedAsync(
                It.IsAny<Organization>(),
                It.IsAny<User>(),
                It.IsAny<User>()),
            times);
    }

    public void VerifyInvitationSent(
        Func<Times> times)
    {
        Verify(
            x => x.InvitationAsync(
                It.IsAny<Organization>(),
                It.IsAny<User>(),
                It.IsAny<User>()),
            times);
    }
}