using Luminous.Domain.Common;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a meal plan entry for a specific day and slot.
/// </summary>
public sealed class Meal : Entity
{
    /// <summary>
    /// Gets or sets the family ID (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date for this meal.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the meal slot.
    /// </summary>
    public MealSlot Slot { get; set; }

    /// <summary>
    /// Gets or sets the meal name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the meal description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the recipe ID (if linked to a recipe).
    /// </summary>
    public string? RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the number of servings.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// Gets or sets notes for this meal.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the assigned family member IDs.
    /// </summary>
    public List<string> Assignees { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the ingredients are on the grocery list.
    /// </summary>
    public bool IngredientsOnList { get; set; }

    /// <summary>
    /// Creates a new meal entry.
    /// </summary>
    public static Meal Create(
        string familyId,
        DateOnly date,
        MealSlot slot,
        string name,
        string createdBy,
        string? recipeId = null)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Meal
        {
            FamilyId = familyId,
            Date = date,
            Slot = slot,
            Name = name.Trim(),
            RecipeId = recipeId,
            CreatedBy = createdBy
        };
    }
}

/// <summary>
/// Represents a recipe stored by a family.
/// </summary>
public sealed class Recipe : Entity
{
    /// <summary>
    /// Gets or sets the family ID (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipe name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipe description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the ingredients.
    /// </summary>
    public List<Ingredient> Ingredients { get; set; } = [];

    /// <summary>
    /// Gets or sets the instructions.
    /// </summary>
    public List<string> Instructions { get; set; } = [];

    /// <summary>
    /// Gets or sets the prep time in minutes.
    /// </summary>
    public int? PrepTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the cook time in minutes.
    /// </summary>
    public int? CookTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of servings.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// Gets or sets the recipe categories/tags.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the source URL (if imported).
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets the recipe image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets dietary notes.
    /// </summary>
    public List<string> DietaryNotes { get; set; } = [];

    /// <summary>
    /// Gets the total time in minutes.
    /// </summary>
    public int? TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
}

/// <summary>
/// Represents an ingredient in a recipe.
/// </summary>
public sealed class Ingredient
{
    /// <summary>
    /// Gets or sets the ingredient name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets preparation notes (e.g., "diced", "melted").
    /// </summary>
    public string? Preparation { get; set; }

    /// <summary>
    /// Gets or sets whether this ingredient is optional.
    /// </summary>
    public bool IsOptional { get; set; }
}
