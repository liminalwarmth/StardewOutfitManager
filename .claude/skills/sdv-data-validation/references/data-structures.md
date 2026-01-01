# Data Structure Reference

## FavoritesData JSON Example

```json
{
  "Favorites": [
    {
      "isFavorite": true,
      "Name": "Spring Farmer",
      "Category": "Spring",
      "LastWorn": 45,
      "Items": {
        "Hat": "Straw Hat_abc123",
        "Shirt": "Basic Pullover_def456",
        "Pants": "Farmer Pants_ghi789"
      },
      "ItemIds": {
        "Hat": "(H)36",
        "Shirt": "(S)1001",
        "Pants": "(P)0"
      },
      "ItemColors": {
        "Shirt": "255,200,100,255",
        "Pants": "50,100,150,255"
      },
      "HairIndex": "",
      "Hair": 4,
      "AccessoryIndex": "",
      "Accessory": 0
    }
  ]
}
```

## Config JSON Example

```json
{
  "StartingDresser": true,
  "RobinSellsDressers": true,
  "TravelingMerchantSellsDressers": true,
  "IncludeRingsInOutfits": true,
  "IncludeFacialHair": false,
  "IncludeModdedHairstyles": true,
  "IncludeModdedAccessories": true,
  "DresserInventorySharing": "Touching"
}
```

## Common Validation Patterns

### Null-Safe Dictionary Access

```csharp
// Pattern 1: TryGetValue
if (outfit.Items?.TryGetValue("Hat", out string hatTag) == true && !string.IsNullOrEmpty(hatTag))
{
    // Use hatTag
}

// Pattern 2: GetValueOrDefault (for optional values)
string shirtColor = outfit.ItemColors?.GetValueOrDefault("Shirt") ?? "";

// Pattern 3: Initialize on access
outfit.Items ??= new Dictionary<string, string>();
```

### Category Validation

```csharp
private static readonly HashSet<string> ValidCategories = new()
{
    "Spring", "Summer", "Fall", "Winter", "Special"
};

public bool IsValidCategory(string category)
{
    return ValidCategories.Contains(category);
}
```

### QualifiedItemId Validation

```csharp
public bool IsValidQualifiedId(string id)
{
    if (string.IsNullOrEmpty(id)) return false;

    // Must start with type prefix in parentheses
    if (!id.StartsWith("(")) return false;

    // Check for known prefixes
    var validPrefixes = new[] { "(O)", "(BC)", "(F)", "(W)", "(B)", "(H)", "(P)", "(S)", "(T)", "(TR)", "(R)" };
    return validPrefixes.Any(p => id.StartsWith(p));
}
```

### Color String Validation

```csharp
public bool IsValidColorString(string colorStr)
{
    if (string.IsNullOrEmpty(colorStr)) return false;

    var parts = colorStr.Split(',');
    if (parts.Length != 4) return false;

    return parts.All(p => byte.TryParse(p.Trim(), out _));
}

public Color ParseColorString(string colorStr)
{
    var parts = colorStr.Split(',');
    return new Color(
        byte.Parse(parts[0]),
        byte.Parse(parts[1]),
        byte.Parse(parts[2]),
        byte.Parse(parts[3])
    );
}
```

## Migration Patterns

### Adding New Optional Field

```csharp
// When adding a new optional field, ensure it's initialized on load
public void OnDataLoaded(FavoritesData data)
{
    foreach (var outfit in data.Favorites)
    {
        // Initialize new field if missing (null from old save)
        outfit.ItemColors ??= new Dictionary<string, string>();
    }
}
```

### Renaming a Field (Breaking Change)

```csharp
// If JSON has old name, SMAPI's JSON serializer won't find it
// Need custom migration in load logic

// Option 1: Use JsonProperty for backwards compatibility
[JsonProperty("OldName")]
public string NewName { get; set; }

// Option 2: Post-load migration (more control)
public void MigrateFromV1(JObject rawData)
{
    var favorites = rawData["Favorites"] as JArray;
    foreach (JObject outfit in favorites)
    {
        if (outfit.ContainsKey("OldName"))
        {
            outfit["NewName"] = outfit["OldName"];
            outfit.Remove("OldName");
        }
    }
}
```

### Changing Field Type

```csharp
// Before: public int SomeValue { get; set; }
// After: public string SomeValue { get; set; }

// Migration:
public void MigrateIntToString(FavoriteOutfit outfit)
{
    // If loaded as int, convert to string
    if (outfit.RawData.TryGetValue("SomeValue", out object val) && val is int intVal)
    {
        outfit.SomeValue = intVal.ToString();
    }
}
```

## Error Handling Patterns

### Graceful Load Failure

```csharp
public FavoritesData LoadFavorites(string path)
{
    try
    {
        var data = Helper.Data.ReadJsonFile<FavoritesData>(path);
        if (data == null)
        {
            Monitor.Log($"No favorites file found at {path}, creating new", LogLevel.Debug);
            return new FavoritesData();
        }

        // Validate and repair
        ValidateAndRepair(data);
        return data;
    }
    catch (Exception ex)
    {
        Monitor.Log($"Error loading favorites: {ex.Message}", LogLevel.Error);
        Monitor.Log("Creating new favorites data to prevent data loss", LogLevel.Warn);
        return new FavoritesData();
    }
}

private void ValidateAndRepair(FavoritesData data)
{
    // Remove invalid outfits instead of crashing
    data.Favorites.RemoveAll(o => o == null);

    foreach (var outfit in data.Favorites)
    {
        // Initialize missing dictionaries
        outfit.Items ??= new Dictionary<string, string>();
        outfit.ItemIds ??= new Dictionary<string, string>();
        outfit.ItemColors ??= new Dictionary<string, string>();

        // Validate category
        if (!ValidCategories.Contains(outfit.Category))
        {
            Monitor.Log($"Invalid category '{outfit.Category}' for outfit '{outfit.Name}', defaulting to 'Special'", LogLevel.Warn);
            outfit.Category = "Special";
        }
    }
}
```
