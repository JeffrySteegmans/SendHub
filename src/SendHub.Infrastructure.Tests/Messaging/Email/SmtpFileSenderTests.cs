using Microsoft.Extensions.Logging;
using Moq;
using SendHub.Infrastructure.Messaging.Email;

namespace SendHub.Infrastructure.Tests.Messaging.Email;

public sealed class SmtpFileSenderTests
{
    [Fact]
    public void GivenValidSettings_WhenAccessingName_ThenShouldReturnEmailSmtp()
    {
        var sender = CreateSender();

        Assert.Equal("Email (SMTP)", sender.Name);
    }

    private static SmtpFileSender CreateSender()
    {
        var settingsMock = new Mock<IApplicationSettings>();
        settingsMock.Setup(x => x.SmtpHost).Returns("smtp.example.com");
        settingsMock.Setup(x => x.SmtpPort).Returns(587);
        settingsMock.Setup(x => x.SmtpFrom).Returns("from@example.com");
        settingsMock.Setup(x => x.SmtpTo).Returns("to@example.com");

        return new SmtpFileSender(
            settingsMock.Object,
            new Mock<ILogger<SmtpFileSender>>().Object);
    }
}
