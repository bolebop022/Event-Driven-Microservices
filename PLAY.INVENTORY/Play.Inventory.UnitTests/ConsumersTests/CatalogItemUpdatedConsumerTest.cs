using System.Threading.Tasks;
using MassTransit;
using Moq;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Serivce.Consumers;
using Play.Inventory.Serivce.Entities;
using Xunit;

namespace Play.Inventory.UnitTests.ConsumersTests;

public class CatalogItemUpdatedConsumerTest
{
    private readonly Mock<IRepository<CatalogItem>> mockRepository;
    private readonly CatalogItemUpdatedConsumer consumer;
    private readonly Mock<ConsumeContext<CatalogItemUpdated>> mockConsumeContext;

    public CatalogItemUpdatedConsumerTest()
    {
        // Initialize the mock repository
        mockRepository = new Mock<IRepository<CatalogItem>>();
        
        // Create the consumer with the mocked repository
        consumer = new CatalogItemUpdatedConsumer(mockRepository.Object);
        
        // Initialize the mock consume context
        mockConsumeContext = new Mock<ConsumeContext<CatalogItemUpdated>>();
    }

    [Fact]
    public async Task Consume_ItemExists_UpdatesItem()
    {
        // Arrange
        // Create a sample message for CatalogItemUpdated
        var message = new CatalogItemUpdated
        (
            Guid.NewGuid(),  // Generate a new GUID for ItemId
            "Updated Potion",
            "Restores HP fully"
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
        // Verify that UpdateAsync was called exactly once with the correct updated item
        mockRepository.Verify(repo => repo.UpdateAsync(It.Is<CatalogItem>(item => 
            item.Id == message.ItemId &&
            item.Name == message.Name &&
            item.Description == message.Description)), Times.Once);
    }

    [Fact]
    public async Task Consume_ItemDoesNotExist_DoesNotUpdateOrCreateItem()
    {
        // Arrange
        // Create a sample message for CatalogItemUpdated
        var message = new CatalogItemUpdated
        (
            Guid.NewGuid(),  // Generate a new GUID for ItemId
            "Updated Potion",
            "Restores HP fully"
        );

        // Mock the GetAsync method to return null, simulating the item does not exist in the repository
        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync((CatalogItem)null);
        
        // Mock the ConsumeContext to return the created message
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        // Invoke the Consume method of the consumer with the mock context
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        // Verify that UpdateAsync was never called since the item did not exist
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CatalogItem>()), Times.Never);
        
        // Ensure CreateAsync was never called as well
        mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<CatalogItem>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ItemDoesNotExist_DoesNotCreateItem()
    {
        // Arrange
        // Create a sample message for CatalogItemUpdated
        var message = new CatalogItemUpdated
        (
            Guid.NewGuid(),  // Generate a new GUID for ItemId
            "New Item",
            "Description for new item"
        );

        // Mock the GetAsync method to return null, simulating that the item does not exist in the repository
        mockRepository.Setup(repo => repo.GetAsync(message.ItemId)).ReturnsAsync((CatalogItem)null);

        // Mock the ConsumeContext to return the created message
        mockConsumeContext.Setup(x => x.Message).Returns(message);

        // Act
        // Invoke the Consume method of the consumer with the mock context
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        // Verify that neither UpdateAsync nor CreateAsync were called, as the method exits early
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CatalogItem>()), Times.Never);
        mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<CatalogItem>()), Times.Never);
    }
}


