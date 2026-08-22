using Microsoft.Extensions.Logging;
using OpenAI;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClientTests
{
    private readonly ILogger<OuranosMachineLearningClient> _logger = Substitute.For<
        ILogger<OuranosMachineLearningClient>
    >();

    private readonly HttpClient _httpClient = new(Substitute.For<HttpMessageHandler>())
    {
        BaseAddress = new Uri("http://test.com/v1/"),
    };

    private readonly OpenAIClient _openAiClient = new(
        new System.ClientModel.ApiKeyCredential("test"),
        new OpenAIClientOptions { Endpoint = new Uri("http://test.com/v1/") }
    );

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(null!, _httpClient, _openAiClient);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenHttpClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(_logger, null!, _openAiClient);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenOpenAiClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(_logger, _httpClient, null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }
}
