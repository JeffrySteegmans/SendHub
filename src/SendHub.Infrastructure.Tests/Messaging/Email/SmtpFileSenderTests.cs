using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    [Theory]
    [InlineData(null, "from@example.com", "to@example.com")]
    [InlineData("smtp.example.com", null, "to@example.com")]
    [InlineData("smtp.example.com", "from@example.com", null)]
    public void GivenMissingRequiredField_WhenValidatingSettings_ThenShouldFailValidation(
        string? host, string? from, string? to)
    {
        var settings = new SmtpSettings
        {
            Host = host!,
            Port = 587,
            From = from!,
            To = to!
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            settings, new ValidationContext(settings), results, validateAllProperties: true);

        Assert.False(isValid);
    }

    [Fact]
    public void GivenValidSettings_WhenValidating_ThenShouldPassValidation()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            From = "from@example.com",
            To = "to@example.com"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            settings, new ValidationContext(settings), results, validateAllProperties: true);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void GivenPortOutOfRange_WhenValidatingSettings_ThenShouldFailValidation(int port)
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = port,
            From = "from@example.com",
            To = "to@example.com"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            settings, new ValidationContext(settings), results, validateAllProperties: true);

        Assert.False(isValid);
    }

    private static SmtpFileSender CreateSender() =>
        new(
            Options.Create(new SmtpSettings
            {
                Host = "smtp.example.com",
                Port = 587,
                From = "from@example.com",
                To = "to@example.com"
            }),
            new Mock<ILogger<SmtpFileSender>>().Object);
}
