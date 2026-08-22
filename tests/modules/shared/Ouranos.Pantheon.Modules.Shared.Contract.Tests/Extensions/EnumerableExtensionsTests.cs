using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Extensions;

public sealed class EnumerableExtensionsTests
{
    [Fact]
    public void Batch_ShouldReturnExpectedResults()
    {
        // Arrange
        var fixture = new Fixture();
        const int batchSize = 5;
        const int numBatches = 10;
        var values = fixture.CreateMany<TestEntity>(batchSize * numBatches).ToList();

        // Act
        var actualBatches = values.Batch(batchSize).ToList();

        // Assert
        actualBatches.Count.ShouldBe(numBatches);
        for (var i = 0; i < numBatches; i++)
        {
            var actualBatch = actualBatches[i].ToList();
            actualBatch.ShouldBe(values.Skip(i * batchSize).Take(batchSize));
        }
    }

    [Fact]
    public void Batch_WhenSourceSizeIsNotMultipleOfBatchSize_ShouldReturnRemainderInLastBatch()
    {
        // Arrange
        var values = Enumerable.Range(1, 11).ToList();

        // Act
        var batches = values.Batch(5).ToList();

        // Assert
        batches.Count.ShouldBe(3);
        batches[2].ToList().ShouldBe([11]);
    }
}
