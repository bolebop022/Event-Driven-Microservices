using Microsoft.AspNetCore.Mvc;
using Moq;
using Play.Common;
using Play.Inventory.Serivce.Controllers;
using Play.Inventory.Serivce.Entities;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Play.Inventory.Serivce.Dtos;
using MongoDB.Driver.Linq;

namespace Play.Inventory.UnitTests;

public class ItemsControllerTest
{
    private readonly Mock<IRepository<InventoryItem>> mockInventoryItemsRepository;
    private readonly Mock<IRepository<CatalogItem>> mockCatalgItemsRepository;
    private readonly ItemsController controller;

    public ItemsControllerTest()
    {
        mockInventoryItemsRepository = new Mock<IRepository<InventoryItem>>();
        mockCatalgItemsRepository = new Mock<IRepository<CatalogItem>>();
        controller = new ItemsController(mockInventoryItemsRepository.Object, mockCatalgItemsRepository.Object);
    }

    [Fact]
    public async Task GetAsync_WithInvalidUserId_ReturnsBadRequestTest()
    {
        // Given
        var invalidUserId = Guid.Empty;

        var result = await controller.GetAsync(invalidUserId);
        
        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public async Task GetAsync_WithValidUserId_ReturnsInventoryItemTest()
    {
        var userId = Guid.NewGuid();

        var catalogItemId = Guid.NewGuid();

        var inventoryItems = new List<InventoryItem>
        {
            new InventoryItem{ CatalogItemId = catalogItemId, UserId = userId, Quantity = 5}
        };

        var catalogItems = new List<CatalogItem>
        {
            new CatalogItem {Id = catalogItemId, Name = "Potion", Description = "Restore HP"}
        };

        mockInventoryItemsRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>()))
            .ReturnsAsync(inventoryItems);
        mockCatalgItemsRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<CatalogItem, bool>>>()))
            .ReturnsAsync(catalogItems);
        
        var result = await controller.GetAsync(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<InventoryItemDto>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal(inventoryItems[0].CatalogItemId, returnValue.First().CatalogItemId);
    }

    [Fact]
    public async Task PostAsync_WithNewInventoryItem_CreatesInventoryItem()
    {
        var grantItemsDto = new GrantItemsDto
        (
            Guid.NewGuid(),
            Guid.NewGuid(),
            10
        );

        mockInventoryItemsRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>()))
            .ReturnsAsync((InventoryItem)null);
        
        var result = await controller.PostAsync(grantItemsDto);

        Assert.IsType<OkResult>(result);
        mockInventoryItemsRepository.Verify(repo => repo.CreateAsync(It.IsAny<InventoryItem>()));
    }

    [Fact]
    public async Task PostAsync_UpdatesInventoryItemTest()
    {
        var catalogItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingInventoryItem = new InventoryItem
        {
            CatalogItemId = catalogItemId,
            UserId = userId,
            Quantity = 5,
        };

        var grantItemsDto = new GrantItemsDto(userId, catalogItemId, 10);

        mockInventoryItemsRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>()))
            .ReturnsAsync(existingInventoryItem);

        var result = await controller.PostAsync(grantItemsDto);

        Assert.IsType<OkResult>(result);
        Assert.Equal(15, existingInventoryItem.Quantity);
        mockInventoryItemsRepository.Verify(repo => repo.UpdateAsync(existingInventoryItem));
    }
}
