using Luminous.Domain.Common;
using NanoidDotNet;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a list (grocery, to-do, custom) for a family.
/// </summary>
public sealed class FamilyList : Entity
{
    /// <summary>
    /// Gets or sets the family ID (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the list type (grocery, todo, custom).
    /// </summary>
    public string ListType { get; set; } = "custom";

    /// <summary>
    /// Gets or sets the items in this list.
    /// </summary>
    public List<ListItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this list is shared with all family members.
    /// </summary>
    public bool IsShared { get; set; } = true;

    /// <summary>
    /// Gets or sets the owner user ID (for private lists).
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets whether this list is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the list color (hex code).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets the count of unchecked items.
    /// </summary>
    public int UncheckedCount => Items.Count(i => !i.IsChecked);

    /// <summary>
    /// Gets the count of checked items.
    /// </summary>
    public int CheckedCount => Items.Count(i => i.IsChecked);

    /// <summary>
    /// Creates a new list.
    /// </summary>
    public static FamilyList Create(
        string familyId,
        string name,
        string createdBy,
        string listType = "custom",
        bool isShared = true)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new FamilyList
        {
            FamilyId = familyId,
            Name = name.Trim(),
            ListType = listType,
            IsShared = isShared,
            OwnerId = isShared ? null : createdBy,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    public ListItem AddItem(string text, string? category = null, decimal? quantity = null, string? unit = null)
    {
        var item = new ListItem
        {
            Id = Nanoid.Generate(),
            Text = text.Trim(),
            Category = category,
            Quantity = quantity,
            Unit = unit,
            Order = Items.Count
        };
        Items.Add(item);
        ModifiedAt = DateTime.UtcNow;
        return item;
    }

    /// <summary>
    /// Removes an item from the list.
    /// </summary>
    public bool RemoveItem(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return false;

        Items.Remove(item);
        ModifiedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Clears all checked items from the list.
    /// </summary>
    public int ClearCheckedItems()
    {
        var count = Items.RemoveAll(i => i.IsChecked);
        if (count > 0) ModifiedAt = DateTime.UtcNow;
        return count;
    }
}

/// <summary>
/// Represents an item within a list.
/// </summary>
public sealed class ListItem
{
    /// <summary>
    /// Gets or sets the item ID.
    /// Uses NanoId for URL-friendly, compact unique identifiers.
    /// </summary>
    public string Id { get; set; } = Nanoid.Generate();

    /// <summary>
    /// Gets or sets the item text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the item is checked.
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Gets or sets the category (for grocery lists).
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets the item notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the assigned user ID.
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets when the item was added.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the item was checked.
    /// </summary>
    public DateTime? CheckedAt { get; set; }

    /// <summary>
    /// Toggles the checked state.
    /// </summary>
    public void Toggle()
    {
        IsChecked = !IsChecked;
        CheckedAt = IsChecked ? DateTime.UtcNow : null;
    }
}
