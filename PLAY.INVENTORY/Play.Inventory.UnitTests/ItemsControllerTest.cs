using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Play.Common;
using Play.Inventory.Serivce.Controllers;
using Play.Inventory.Serivce.Entities;
using Xunit;

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
}
