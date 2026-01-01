# UI Patterns in This Codebase

## Current Menu Structure

### WardrobeMenu Layout
```
Width: 1000 + borderWidth * 2
Height: 600 + borderWidth * 2

+--------------------------------------------------+
| Tab Bar (via menuManager.includeTopTabButtons)   |
+--------------------------------------------------+
|                                                  |
| Portrait Box     Selector Buttons                |
| (256x384)        Hat:    [<] label [>]           |
| at Y+88          Shirt:  [<] label [>]           |
|                  Pants:  [<] label [>]           |
| Equipment        Boots:  [<] label [>]           |
| Icons below      (Rings if enabled)              |
|                                                  |
| [<] Portrait [>] Category Buttons                |
|                  [Spring][Summer][Fall][Winter]  |
|                                                  |
|                  [Save Favorite]                 |
+--------------------------------------------------+
```

### Key Positioning Patterns

**Portrait Box:**
```csharp
_portraitBox = new Rectangle(
    xPositionOnScreen + borderWidth + spaceToClearSideBorder,
    yPositionOnScreen + 88,  // Offset for outfit name label
    256,   // 4x scale farmer
    384    // 4x scale farmer
);
```

**Equipment Icons (Dynamic Grid):**
```csharp
bool includeRings = Config.IncludeRingsInOutfits;
int numCols = includeRings ? 3 : 2;
int gridWidth = numCols * 64;
int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - gridWidth / 2;  // Centered under portrait
int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
```

**Selector Buttons (Right Side):**
```csharp
int selectorBtnsX = xPositionOnScreen + width/2 + borderWidth + spaceToClearSideBorder - 120;
int selectorBtnsY = _portraitBox.Y - 32;
int yBtnSpacing = 96;  // Vertical space between each button set
```

### FavoritesMenu Layout

**Outfit Grid:**
```csharp
int gridStartX = xPositionOnScreen + borderWidth + spaceToClearSideBorder + 16;
int gridStartY = yPositionOnScreen + borderWidth + spaceToClearSideBorder + 64;
int cardWidth = 108;
int cardHeight = 148;
int cardsPerRow = 4;
int cardSpacing = 16;
```

**Scrolling:**
```csharp
int visibleRows = 3;
int totalCards = outfits.Count;
int totalRows = (int)Math.Ceiling(totalCards / (float)cardsPerRow);
```

## Common Spacing Values

| Constant | Value | Usage |
|----------|-------|-------|
| `borderWidth` | 16 | Menu frame border |
| `spaceToClearSideBorder` | 16 | Content inset from frame |
| Button size | 64 | Standard clickable buttons |
| Arrow buttons | 48-60 | Direction selectors |
| Card spacing | 16 | Gap between outfit cards |
| Section gap | 32 | Between major UI sections |
| Label spacing | 4 | Between text label pairs |

## Snap Region IDs

```csharp
internal const int LABELS = 10000;
internal const int PORTRAIT = 20000;
internal const int CATEGORIES = 30000;
```

Use these bases to create unique myIDs for gamepad navigation.

## Window Resize Pattern

```csharp
public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
{
    base.gameWindowSizeChanged(oldBounds, newBounds);

    // Recenter
    Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
    xPositionOnScreen = (int)topLeft.X;
    yPositionOnScreen = (int)topLeft.Y;

    // Recalculate dependent positions
    _portraitBox.X = xPositionOnScreen + borderWidth + spaceToClearSideBorder;
    _portraitBox.Y = yPositionOnScreen + 88;
    // ... continue for all elements

    // CRITICAL: Propagate to child menus (naming dialogs, etc.)
    if (GetChildMenu() != null)
    {
        GetChildMenu().gameWindowSizeChanged(oldBounds, newBounds);
    }
}
```

## Text Drawing Helpers

```csharp
// Standard text
b.DrawString(Game1.smallFont, text, position, Color.Black);

// Styled text (larger, with shadow)
SpriteText.drawString(b, text, x, y);

// Hover text (call in draw, after other elements)
if (!string.IsNullOrEmpty(hoverText))
    drawHoverText(b, hoverText, Game1.smallFont);

// Measure text for centering
Vector2 textSize = Game1.smallFont.MeasureString(text);
int centeredX = bounds.X + bounds.Width / 2 - (int)(textSize.X / 2);
```
