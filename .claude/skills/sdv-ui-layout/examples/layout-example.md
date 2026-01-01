# UI Layout Example: Adding a New Button

This example shows the workflow for adding a button to an existing menu.

## Task: Add a "Randomize Outfit" button to WardrobeMenu

### Step 1: Design in Text

```
Current Layout:
+------------------+
| Portrait         |
+------------------+
| [<] Direction [>]|
+------------------+
| Equipment Icons  |
+------------------+

Proposed Layout:
+------------------+
| Portrait         |
+------------------+
| [<] Direction [>]|
+------------------+
| Equipment Icons  |
+------------------+
|   [Randomize]    |  <- New button, centered under equipment
+------------------+
```

### Step 2: Define Position Semantically

```csharp
// Position relative to equipment grid, not absolute
int randomizeButtonX = eqIconXOffset + gridWidth / 2 - Layout.ButtonSize / 2;
int randomizeButtonY = eqIconYOffset + 128 + Layout.ElementGap;  // 128 = 2 rows * 64

randomizeButton = new ClickableTextureComponent(
    "Randomize",
    new Rectangle(randomizeButtonX, randomizeButtonY, Layout.ButtonSize, Layout.ButtonSize),
    null,
    "Randomize outfit",
    Game1.mouseCursors,
    new Rectangle(381, 361, 10, 10),  // Dice icon
    4f  // Scale
);
```

### Step 3: Add Gamepad Navigation

```csharp
// Connect to existing navigation
randomizeButton.myID = PORTRAIT + 100;

// Find bottom equipment icon and link
var bottomEquipment = equipmentIcons.Last();
bottomEquipment.downNeighborID = randomizeButton.myID;
randomizeButton.upNeighborID = bottomEquipment.myID;

// Link to category buttons to the right
randomizeButton.rightNeighborID = categoryButtons.First().myID;
categoryButtons.First().leftNeighborID = randomizeButton.myID;
```

### Step 4: Update gameWindowSizeChanged

```csharp
public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
{
    base.gameWindowSizeChanged(oldBounds, newBounds);

    // ... existing recalculations ...

    // Recalculate randomize button
    int randomizeButtonX = eqIconXOffset + gridWidth / 2 - Layout.ButtonSize / 2;
    int randomizeButtonY = eqIconYOffset + 128 + Layout.ElementGap;
    randomizeButton.bounds = new Rectangle(
        randomizeButtonX, randomizeButtonY,
        Layout.ButtonSize, Layout.ButtonSize
    );
}
```

### Step 5: Static Validation Checklist

Before testing:
- [x] Button has explicit width/height (64x64)
- [x] Position uses semantic reference (eqIconXOffset, gridWidth)
- [x] myID assigned (PORTRAIT + 100)
- [x] All neighborIDs connected (up, right)
- [x] gameWindowSizeChanged updates bounds
- [x] Button fits within menu bounds

### Step 6: Test

Only now request manual testing. Common issues to verify:
- Button visible and clickable
- Hover text displays correctly
- Gamepad can navigate to button
- Window resize maintains position
