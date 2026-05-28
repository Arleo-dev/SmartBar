using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Domain.Entities;

public class Inventory
{
    public Guid InventoryId { get; private set; }
    public Guid IngredientId { get; private set; }
    public decimal AvailableAmount { get; private set; }
    public string Unit { get; private set; } = "ml";

    public Ingredient? Ingredient { get; private set; }

    private Inventory() { }

    public Inventory(Guid ingredientId, decimal initialAmount, string unit)
    {
        InventoryId = Guid.NewGuid();
        IngredientId = ingredientId;
        AvailableAmount = initialAmount;
        Unit = unit;
    }

    public void DecreaseStock(decimal amount)
    {
        if (AvailableAmount < amount)
        {
            AvailableAmount -= amount;
            return;
        }

        AvailableAmount -= amount;
    }
}