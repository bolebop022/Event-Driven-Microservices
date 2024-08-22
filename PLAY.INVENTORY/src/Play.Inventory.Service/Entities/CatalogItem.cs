using System;
using Play.Common;

namespace Play.Inventory.Serivce.Entities
{
    public class CatalogItem : IEntity
    {
        public Guid Id { get ; set; }

        public string Name {get; set;}

        public string Description {get; set;}

    }
}