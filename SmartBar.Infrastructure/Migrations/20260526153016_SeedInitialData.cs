using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cocktails",
                columns: new[] { "CocktailId", "Description", "Name", "RecipeSteps" },
                values: new object[] { new Guid("a4444444-4444-4444-4444-444444444444"), "Classic cocktail made with rye or bourbon, sweet vermouth, and bitters.", "Manhattan", "" });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "IngredientId", "Category", "Name" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Whiskey", "Bourbon" },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), "Bitters", "Angostura Bitters" },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), "Vermouth", "Sweet Vermouth" }
                });

            migrationBuilder.InsertData(
                table: "CocktailIngredients",
                columns: new[] { "CocktailId", "IngredientId", "Amount", "Unit" },
                values: new object[,]
                {
                    { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("b1111111-1111-1111-1111-111111111111"), 60m, "ml" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("b3333333-3333-3333-3333-333333333333"), 2m, "dashes" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("c2222222-2222-2222-2222-222222222222"), 30m, "ml" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CocktailIngredients",
                keyColumns: new[] { "CocktailId", "IngredientId" },
                keyValues: new object[] { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("b1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "CocktailIngredients",
                keyColumns: new[] { "CocktailId", "IngredientId" },
                keyValues: new object[] { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("b3333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "CocktailIngredients",
                keyColumns: new[] { "CocktailId", "IngredientId" },
                keyValues: new object[] { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("c2222222-2222-2222-2222-222222222222") });

            migrationBuilder.DeleteData(
                table: "Cocktails",
                keyColumn: "CocktailId",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "IngredientId",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "IngredientId",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "IngredientId",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));
        }
    }
}
