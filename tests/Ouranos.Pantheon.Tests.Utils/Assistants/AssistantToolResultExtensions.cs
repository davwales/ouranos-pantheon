using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;
using Shouldly;

namespace Ouranos.Pantheon.Tests.Utils.Assistants;

public static class AssistantToolResultExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new IdJsonConverterFactory(), new JsonStringEnumConverter() },
    };

    public static TData ShouldContainData<TData>(this AssistantToolResult result)
        where TData : class
    {
        using var document = JsonDocument.Parse(result.Output);
        document.RootElement.TryGetProperty("note", out _).ShouldBeFalse(result.Output);

        var data = JsonSerializer.Deserialize<TData>(result.Output, Options);
        return data.ShouldNotBeNull();
    }

    public static void ShouldContainData<TData>(this AssistantToolResult result, TData expected)
        where TData : class
    {
        var actual = result.ShouldContainData<TData>();

        JsonSerializer
            .Serialize(actual, Options)
            .ShouldBe(JsonSerializer.Serialize(expected, Options));
    }

    public static string ShouldContainNote(this AssistantToolResult result)
    {
        using var document = JsonDocument.Parse(result.Output);
        var note = document.RootElement.GetProperty("note").GetString();
        return note.ShouldNotBeNull();
    }
}
