namespace SmartBar.Domain.Entities;

public class Inventory
{
    public Guid InventoryId { get; private set; } = Guid.CreateVersion7();
    public Guid IngredientId { get; private set; }
    public decimal AvailableAmount { get; private set; }
    public string Unit { get; private set; } = "ml";

    public Ingredient? Ingredient { get; private set; }

    private Inventory() { }

    public Inventory(Guid ingredientId, decimal initialAmount, string unit)
    {
        IngredientId = ingredientId;
        AvailableAmount = initialAmount;
        Unit = unit;
    }

    public void DecreaseStock(decimal amount)
    {
        if (AvailableAmount < amount)
        {
            throw new InvalidOperationException($"Not enough stock available to decrease by the specified amount. Available: {AvailableAmount}, Requested: {amount}");
        }

        AvailableAmount -= amount;
    }
}