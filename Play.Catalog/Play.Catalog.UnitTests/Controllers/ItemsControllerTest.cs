using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Controllers;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using Xunit;

namespace Play.Catalog.UnitTests.Controllers;




public class ItemsControllerTests
{
    private readonly Mock<IRepository<Item>> mockItemsRepository;
    private readonly Mock<IPublishEndpoint> mockPublishEndpoint;
    private readonly ItemsController controller;

    public ItemsControllerTests()
    {
        mockItemsRepository = new Mock<IRepository<Item>>();
        mockPublishEndpoint = new Mock<IPublishEndpoint>();
        controller = new ItemsController(mockItemsRepository.Object, mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllItems()
    {
        // Arrange
        var expectedItems = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Potion", Description = "Restores HP", Price = 9, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Elixir", Description = "Restores MP", Price = 19, CreatedDate = DateTimeOffset.UtcNow }
        };

        mockItemsRepository.Setup(repo => repo.GetAllAsync())
                            .ReturnsAsync(expectedItems);

        // Act
        var result = await controller.GetAsync();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualItems = Assert.IsAssignableFrom<IEnumerable<ItemDto>>(actionResult.Value);
        Assert.Equal(expectedItems.Count, actualItems.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ItemExists_ReturnsItem()
    {
        // Arrange
        var expectedItem = new Item { Id = Guid.NewGuid(), Name = "Potion", Description = "Restores HP", Price = 9, CreatedDate = DateTimeOffset.UtcNow };

        mockItemsRepository.Setup(repo => repo.GetAsync(expectedItem.Id))
                            .ReturnsAsync(expectedItem);

        // Act
        var result = await controller.GetByIdAsync(expectedItem.Id);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualItem = Assert.IsType<ItemDto>(actionResult.Value);
        Assert.Equal(expectedItem.Id, actualItem.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ItemDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        mockItemsRepository.Setup(repo => repo.GetAsync(itemId))
                            .ReturnsAsync((Item)null);

        // Act
        var result = await controller.GetByIdAsync(itemId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostAsync_CreatesNewItem()
    {
        // Arrange
        var createItemDto = new CreatItemDto ("Potion", "Restores HP", 9 );
        var expectedItem = new Item { Id = Guid.NewGuid(), Name = createItemDto.Name, Description = createItemDto.Description, Price = createItemDto.Price, CreatedDate = DateTimeOffset.UtcNow };

        mockItemsRepository.Setup(repo => repo.CreateAsync(It.IsAny<Item>()))
                            .Callback<Item>(item => item.Id = expectedItem.Id)
                            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.PostAsync(createItemDto);

        // Assert
        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var actualItem = Assert.IsType<Item>(actionResult.Value);
        Assert.Equal(expectedItem.Name, actualItem.Name);
        mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<CatalogItemCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task PutAsync_ItemExists_UpdatesItem()
    {
        // Arrange
        var existingItem = new Item { Id = Guid.NewGuid(), Name = "Potion", Description = "Restores HP", Price = 9, CreatedDate = DateTimeOffset.UtcNow };
        var updateItemDto = new UpdateItemDto ("Super Potion", "Restores more HP", 15 );

        mockItemsRepository.Setup(repo => repo.GetAsync(existingItem.Id))
                            .ReturnsAsync(existingItem);

        // Act
        var result = await controller.PutAsync(existingItem.Id, updateItemDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(updateItemDto.Name, existingItem.Name);
        Assert.Equal(updateItemDto.Description, existingItem.Description);
        Assert.Equal(updateItemDto.Price, existingItem.Price);
        mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<CatalogItemUpdated>(), default), Times.Once);
    }

    [Fact]
    public async Task PutAsync_ItemDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateItemDto = new UpdateItemDto( "Super Potion", "Restores HP more", 15 );

        mockItemsRepository.Setup(repo => repo.GetAsync(itemId))
                            .ReturnsAsync((Item)null);

        // Act
        var result = await controller.PutAsync(itemId, updateItemDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ItemExists_DeletesItem()
    {
        // Arrange
        var existingItem = new Item { Id = Guid.NewGuid(), Name = "Potion", Description = "Restores HP", Price = 9, CreatedDate = DateTimeOffset.UtcNow };

        mockItemsRepository.Setup(repo => repo.GetAsync(existingItem.Id))
                            .ReturnsAsync(existingItem);

        // Act
        var result = await controller.DeleteAsync(existingItem.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockItemsRepository.Verify(repo => repo.RemoveAsync(existingItem.Id), Times.Once);
        mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<CatalogItemDeleted>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ItemDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        mockItemsRepository.Setup(repo => repo.GetAsync(itemId))
                            .ReturnsAsync((Item)null);

        // Act
        var result = await controller.DeleteAsync(itemId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

