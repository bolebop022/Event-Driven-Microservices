using System.Threading.Tasks;
using MassTransit;
using Moq;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Serivce.Consumers;
using Play.Inventory.Serivce.Entities;
using Xunit;

namespace Play.Inventory.UnitTests.ConsumersTests;

public class CatalogItemCreatedConsumerTest
{
    private readonly Mock<IRepository<CatalogItem>> mockRepository;
    private readonly CatalogItemCreatedConsumer consumer;
    private readonly Mock<ConsumeContext<CatalogItemCreated>> mockConsumeContext;

    public CatalogItemCreatedConsumerTest()
    {
        mockRepository = new Mock<IRepository<CatalogItem>>();
        consumer = new CatalogItemCreatedConsumer(mockRepository.Object);
        mockConsumeContext = new Mock<ConsumeContext<CatalogItemCreated>>();
    }

    [Fact]
    public async Task Consume_ItemAlreadyExists_DoesNotCreateItem()
    {
        // Arrange
        var message = new CatalogItemCreated
        (
            Guid.NewGuid(),
            "Potion",
            "Restores HP"
        );

        var existingItem = new CatalogItem
        {
            Id = message.ItemId,
            Name = message.Name,
            Description = message.Description
        };

        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync(existingItem);
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<CatalogItem>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ItemDoesNotExist_CreatesNewItem()
    {
        // Arrange
        var message = new CatalogItemCreated
        (
             Guid.NewGuid(),
            "Potion",
            "Restores HP"
        );

        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync((CatalogItem)null);
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        mockRepository.Verify(repo => repo.CreateAsync(It.Is<CatalogItem>(item => 
            item.Id == message.ItemId &&
            item.Name == message.Name &&
            item.Description == message.Description)), Times.Once);
    }
}

