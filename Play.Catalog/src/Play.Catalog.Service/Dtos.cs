using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service.Dtos
{
    public record ItemDto(Guid Id, string Name, string Description, decimal Price, DateTimeOffset createdDate);

    public record CreatItemDto([Required] string Name, string Description, [Range(0,100)] decimal Price);

    public record UpdateItemDto([Required] string Name, string Description, [Range(0,100)] decimal Price);
}