using System.Threading.Tasks;
using MassTransit;
using Moq;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Serivce.Consumers;
using Play.Inventory.Serivce.Entities;

namespace Play.Inventory.UnitTests.ConsumersTests;

public class CatalogItemDeletedConsumerTest
{
    private readonly Mock<IRepository<CatalogItem>> mockRepository;
    private readonly CatalogItemDeletedConsumer consumer;
    private readonly Mock<ConsumeContext<CatalogItemDeleted>> mockConsumeContext;

    public CatalogItemDeletedConsumerTest()
    {
        // Initialize the mock repository
        mockRepository = new Mock<IRepository<CatalogItem>>();
        
        // Create the consumer with the mocked repository
        consumer = new CatalogItemDeletedConsumer(mockRepository.Object);
        
        // Initialize the mock consume context
        mockConsumeContext = new Mock<ConsumeContext<CatalogItemDeleted>>();
    }

    [Fact]
    public async Task Consume_ItemExists_RemovesItem()
    {
        // Arrange
        // Create a sample message for CatalogItemDeleted
        var message = new CatalogItemDeleted
        (
            Guid.NewGuid()  // Generate a new GUID for ItemId
        );

        // Set up an existing CatalogItem to simulate an item present in the repository
        var existingItem = new CatalogItem
        {
            Id = message.ItemId,
            Name = "Potion",
            Description = "Restores HP"
        };

        // Mock the GetAsync method to return the existing item when queried with the given ItemId
        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync(existingItem);
        
        // Mock the ConsumeContext to return the created message
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        // Invoke the Consume method of the consumer with the mock context
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        // Verify that RemoveAsync was called exactly once with the correct item ID
        mockRepository.Verify(repo => repo.RemoveAsync(existingItem.Id), Times.Once);
    }

    [Fact]
    public async Task Consume_ItemDoesNotExist_DoesNotRemoveItem()
    {
        // Arrange
        // Create a sample message for CatalogItemDeleted
        var message = new CatalogItemDeleted
        (
            Guid.NewGuid()  // Generate a new GUID for ItemId
        );

        // Mock the GetAsync method to return null, simulating the item does not exist in the repository
        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync((CatalogItem)null);
        
        // Mock the ConsumeContext to return the created message
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        // Invoke the Consume method of the consumer with the mock context
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        // Verify that RemoveAsync was never called, as the item does not exist
        mockRepository.Verify(repo => repo.RemoveAsync(It.IsAny<Guid>()), Times.Never);
    }
}


