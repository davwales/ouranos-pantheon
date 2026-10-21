using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningModuleTests
{
    [Fact]
    public void AddCoreOuranosMachineLearningModule_ShouldAddExpectedServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        services.AddCoreOuranosMachineLearningModule(configuration);

        // Assert
        services.ShouldContainService<IOuranosMachineLearningClient>(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCoreOuranosMachineLearningModule_WhenConnectionStringProvided_ShouldBuildClient()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{OuranosMachineLearningOptions.SectionName}:ConnectionString"] =
                        "http://localhost:5000/",
                    [$"{OuranosMachineLearningOptions.SectionName}:ApiKey"] = "test-api-key",
                }
            )
            .Build();

        services.AddCoreOuranosMachineLearningModule(configuration);

        var sp = services.BuildServiceProvider();

        // Act
        var client = sp.GetRequiredService<IOuranosMachineLearningClient>();

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreOuranosMachineLearningModule_WhenConnectionStringEmpty_ShouldThrowOnResolve()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{OuranosMachineLearningOptions.SectionName}:ConnectionString"] = "",
                }
            )
            .Build();

        services.AddCoreOuranosMachineLearningModule(configuration);

        var sp = services.BuildServiceProvider();

        // Act
        var act = () => sp.GetRequiredService<IOuranosMachineLearningClient>();

        // Assert
        act.ShouldThrow<Exception>();
    }

    [Fact]
    public async Task CreateClientOptions_WhenRequestFails_ShouldNotResendIt()
    {
        // Arrange
        var handler = new CountingHandler(HttpStatusCode.ServiceUnavailable);
        var options = OuranosMachineLearningModule.CreateClientOptions(
            new OuranosMachineLearningOptions { ConnectionString = "http://localhost:5000/" }
        );
        options.Transport = new HttpClientPipelineTransport(new HttpClient(handler));
        var client = new OpenAIClient(new ApiKeyCredential("test-api-key"), options);

        // Act
        var act = () =>
            client.GetChatClient("model").CompleteChatAsync([new UserChatMessage("Hi")]);

        // Assert
        await act.ShouldThrowAsync<ClientResultException>();
        handler.Requests.ShouldBe(1);
    }

    private sealed class CountingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
