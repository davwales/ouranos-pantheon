using NpgsqlTypes;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres.Querying;

public sealed class RawSqlCommandTests
{
    private sealed class TestEntity;

    [Fact]
    public void FromSql_ShouldSetSqlProperty()
    {
        // Act
        var command = RawSqlCommand.FromSql("SELECT 1");

        // Assert
        command.Sql.ShouldBe("SELECT 1");
    }

    [Fact]
    public void GetParameters_WhenNoParametersAdded_ShouldReturnEmptyArray()
    {
        // Arrange
        var command = RawSqlCommand.FromSql("SELECT 1");

        // Act
        var parameters = command.GetParameters();

        // Assert
        parameters.ShouldBeEmpty();
    }

    [Fact]
    public void WithId_ShouldAddUuidParameter()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new Id<TestEntity>(guid.ToString());

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithId("@id", id);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = parameters[0];
        param.ShouldNotBeNull();
        var npgsqlParam = (Npgsql.NpgsqlParameter)param;
        npgsqlParam.NpgsqlDbType.ShouldBe(NpgsqlDbType.Uuid);
        npgsqlParam.Value.ShouldBe(guid);
    }

    [Fact]
    public void WithId_ShouldConvertStringGuidToUuidValue()
    {
        // Arrange
        var guid = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var id = new Id<TestEntity>("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithId("@id", id);

        // Assert
        var param = (Npgsql.NpgsqlParameter)command.GetParameters()[0];
        param.Value.ShouldBe(guid);
    }

    [Fact]
    public void WithDateTimeOffset_WhenNotNullable_ShouldAddTimestampTzParameter()
    {
        // Arrange
        var value = DateTimeOffset.UtcNow;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithDateTimeOffset("@ts", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.TimestampTz);
        param.Value.ShouldBe(value);
    }

    [Fact]
    public void WithDateTimeOffset_WhenNullableWithValue_ShouldAddParameterWithValue()
    {
        // Arrange
        DateTimeOffset? value = DateTimeOffset.UtcNow;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithDateTimeOffset("@ts", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.Value.ShouldBe(value);
    }

    [Fact]
    public void WithDateTimeOffset_WhenNullableWithNull_ShouldAddParameterWithNullValue()
    {
        // Arrange
        DateTimeOffset? value = null;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithDateTimeOffset("@ts", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.Value.ShouldBeNull();
    }

    [Fact]
    public void WithString_ShouldAddTextParameter()
    {
        // Arrange
        const string value = "test";

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithString("@name", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.Text);
        param.Value.ShouldBe(value);
    }

    [Fact]
    public void WithDecimal_ShouldAddNumericParameter()
    {
        // Arrange
        const decimal value = 123.45m;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithDecimal("@price", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.Numeric);
        param.Value.ShouldBe(value);
    }

    [Fact]
    public void WithInt_ShouldAddIntegerParameter()
    {
        // Arrange
        const int value = 42;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithInt("@count", value);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.Integer);
        param.Value.ShouldBe(value);
    }

    [Fact]
    public void WithIdList_ShouldAddUuidArrayParameter()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var ids = new List<Id<TestEntity>> { new(guid1.ToString()), new(guid2.ToString()), };

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithIds("@ids", ids);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.Array | NpgsqlDbType.Uuid);
        var values = (Guid[])param.Value!;
        values.Length.ShouldBe(2);
        values.ShouldContain(guid1);
        values.ShouldContain(guid2);
    }

    [Fact]
    public void WithIdList_WhenEmpty_ShouldAddEmptyArrayParameter()
    {
        // Arrange & Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithIds("@ids", new List<Id<TestEntity>>());

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        var param = (Npgsql.NpgsqlParameter)parameters[0];
        param.NpgsqlDbType.ShouldBe(NpgsqlDbType.Array | NpgsqlDbType.Uuid);
        var values = (Guid[])param.Value!;
        values.ShouldBeEmpty();
    }

    [Fact]
    public void WithIdList_ShouldConvertStringGuidsToUuidValues()
    {
        // Arrange
        var guid1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var guid2 = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var ids = new List<Id<TestEntity>>
        {
            new("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
        };

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithIds("@ids", ids);

        // Assert
        var param = (Npgsql.NpgsqlParameter)command.GetParameters()[0];
        var values = (Guid[])param.Value!;
        values.ShouldContain(guid1);
        values.ShouldContain(guid2);
    }

    [Fact]
    public void WithParameter_ShouldAddProvidedNpgsqlParameter()
    {
        // Arrange
        var parameter = new Npgsql.NpgsqlParameter("@custom", NpgsqlDbType.Text) { Value = "hello" };

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithParameter(parameter);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ShouldBe(parameter);
    }

    [Fact]
    public void FluentApi_ShouldChainMultipleParameters()
    {
        // Arrange
        var id = new Id<TestEntity>(Guid.NewGuid().ToString());
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithId("@symbolId", id)
            .WithDateTimeOffset("@since", timestamp)
            .WithDecimal("@price", 99.9m)
            .WithInt("@count", 5);

        // Assert
        var parameters = command.GetParameters();
        parameters.Length.ShouldBe(4);
    }

    [Fact]
    public void GetParameters_ShouldPreserveInsertionOrder()
    {
        // Arrange
        var id = new Id<TestEntity>(Guid.NewGuid().ToString());

        // Act
        var command = RawSqlCommand.FromSql("SELECT 1")
            .WithId("@id", id)
            .WithString("@name", "test")
            .WithInt("@count", 10);

        // Assert
        var parameters = command.GetParameters();
        var first = (Npgsql.NpgsqlParameter)parameters[0];
        var second = (Npgsql.NpgsqlParameter)parameters[1];
        var third = (Npgsql.NpgsqlParameter)parameters[2];
        first.ParameterName.ShouldBe("@id");
        first.NpgsqlDbType.ShouldBe(NpgsqlDbType.Uuid);
        second.ParameterName.ShouldBe("@name");
        second.NpgsqlDbType.ShouldBe(NpgsqlDbType.Text);
        third.ParameterName.ShouldBe("@count");
        third.NpgsqlDbType.ShouldBe(NpgsqlDbType.Integer);
    }
}