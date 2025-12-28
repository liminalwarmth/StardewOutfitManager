# Enhancement: Custom Gamepad Keyboard for Outfit Naming

## Overview

When using a gamepad to rename an outfit, the default `TextEntryMenu` (on-screen keyboard) provides no context about what's being named. This enhancement would create a custom `OutfitTextEntryMenu` that:

1. Displays "Name This Outfit" title above the keyboard
2. Shows the outfit model farmer preview alongside the keyboard
3. Replaces the standard `TextEntryMenu` only for outfit naming

## Current Behavior

When renaming an outfit on gamepad:
1. User clicks rename button (or gamepad equivalent)
2. `NamingMenu` opens with title banner "Name This Outfit" and textbox
3. TextBox's `Selected` property triggers `_showKeyboard = true`
4. In `TextBox.Update()`, if gamepad controls are active, calls `Game1.showTextEntry(this)`
5. `TextEntryMenu` overlay appears with on-screen keyboard - **NO title or outfit preview**

## Proposed Implementation

### Approach: Custom TextBox Subclass + Custom TextEntryMenu

**Step 1: Create `OutfitTextBox` class**

Subclass `TextBox` and override `Update()` to call our custom menu instead of `Game1.showTextEntry()`:

```csharp
public class OutfitTextBox : TextBox
{
    private FavoriteOutfit _outfit;
    private Farmer _modelFarmer;

    public OutfitTextBox(FavoriteOutfit outfit, Farmer modelFarmer, ...)
    {
        _outfit = outfit;
        _modelFarmer = modelFarmer;
    }

    public override void Update()
    {
        // Handle selection changes
        Selected = new Rectangle(X, Y, Width, Height)
            .Contains(new Point(Game1.getMouseX(), Game1.getMouseY()));

        // Intercept gamepad keyboard trigger
        if (_showKeyboard)
        {
            if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
            {
                // Use our custom menu instead of Game1.showTextEntry(this)
                Game1.activeClickableMenu = new OutfitTextEntryMenu(this, _outfit, _modelFarmer);
            }
            _showKeyboard = false;
        }
    }
}
```

**Note**: `_showKeyboard` is private in `TextBox`, so we'd need to either:
- Use reflection to access it
- Duplicate the selection logic entirely
- Use Harmony to patch the base class

**Step 2: Create `OutfitTextEntryMenu` class**

Modified copy of `TextEntryMenu` that adds:
- Title banner at top ("Name This Outfit")
- Outfit preview on left side (model farmer in outfit)
- Keyboard grid on right side

```csharp
public class OutfitTextEntryMenu : IClickableMenu
{
    private TextBox _target;
    private FavoriteOutfit _outfit;
    private Farmer _modelFarmer;

    // Keyboard grid (copied from TextEntryMenu)
    private List<ClickableTextureComponent> keys;
    private int currentKeyboardMode; // 0=letters, 1=uppercase, 2=symbols

    public OutfitTextEntryMenu(TextBox target, FavoriteOutfit outfit, Farmer farmer)
    {
        _target = target;
        _outfit = outfit;
        _modelFarmer = farmer;

        // Initialize keyboard grid (672x352 centered, offset for preview)
        // Add preview area on left (256 wide)
        // Position keyboard on right
    }

    public override void draw(SpriteBatch b)
    {
        // Dark overlay
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds,
               Color.Black * 0.8f);

        // Draw title banner
        SpriteText.drawStringWithScrollCenteredAt(b, "Name This Outfit",
            Game1.uiViewport.Width / 2, yPositionOnScreen - 80);

        // Draw outfit preview (left side)
        DrawOutfitPreview(b);

        // Draw textbox (above keyboard)
        DrawTextBox(b);

        // Draw keyboard grid (right side)
        DrawKeyboard(b);
    }

    private void DrawOutfitPreview(SpriteBatch b)
    {
        // Background box for preview
        // Draw model farmer in outfit
        // Similar to FavoritesMenu's portrait rendering
    }
}
```

### Key Technical Details

**TextEntryMenu Dependencies** (from decompiled source):
- `Game1.mouseCursors2` - Button graphics for keys
- `Game1.playSound()` - Sound effects ("bigSelect", "button1", "bigDeSelect")
- `Game1.closeTextEntry()` - Called when menu closes
- `Game1.options.SnappyMenus` - Menu navigation preference
- `Game1.dialogueFont` - Font for character labels
- `Game1.fadeToBlackRect` - Background texture
- `Game1.DrawBox()` - Utility for frame rendering
- `Game1.input.GetGamePadState()` - Input state

**TextEntryMenu Layout**:
- Size: 672x352 pixels, centered on screen
- Keys: 11 per row, 4 rows, 64x64 pixels each (4x scaled from 16x16)
- 3 keyboard modes: lowercase, uppercase, symbols
- Control buttons: Backspace, Space, OK, Uppercase toggle, Symbols toggle

**Gamepad Button Mapping**:
- Y button: Insert space
- X button: Backspace
- B button: Close menu (cancel)
- Start button: Submit
- LeftStick click: Toggle uppercase/lowercase
- RightStick click: Toggle symbols

### Integration with NamingMenu

Option A: Replace TextBox in NamingMenu with OutfitTextBox
- Requires modifying how we create the NamingMenu
- May need to create our own NamingMenu subclass

Option B: Use Harmony to patch Game1.showTextEntry()
- Check if the TextBox belongs to our outfit naming context
- If so, show our custom menu instead
- Requires a way to identify our TextBox (could use `TextBox.TitleText` property)

### Recommended Approach

1. Start with **Option B (Harmony patch)** as it's less invasive:
   - Patch `Game1.showTextEntry()` prefix
   - Check if `textBox.TitleText` contains "Outfit" or similar marker
   - If match, create `OutfitTextEntryMenu` and set as active menu
   - Return false to skip original method

2. Set `namingMenu.textBox.TitleText = "Outfit"` when creating the NamingMenu to mark it

### Scope and Complexity

**Effort**: Medium-High
- Copy and modify ~300 lines of TextEntryMenu code
- Add outfit preview rendering (can reuse FavoritesMenu code)
- Create Harmony patch for integration
- Test all keyboard modes and gamepad inputs

**Files to Create/Modify**:
- `Menus/OutfitTextEntryMenu.cs` - New file (~400 lines)
- `Patches/TextEntryPatch.cs` - New Harmony patch (~30 lines)
- `Menus/FavoritesMenu.cs` - Set TitleText marker on textbox

### Alternative: Simpler Approach

If the full custom keyboard is too complex, a simpler alternative:
- Just add a Harmony postfix patch to `TextEntryMenu.draw()`
- Draw the title banner and outfit preview as overlays
- No need to copy/modify the keyboard code

This would be faster to implement but less clean architecturally.

## Layout Requirements (Match Keyboard Mode)

When implementing the gamepad keyboard, the layout must match the keyboard-mode `OutfitNamingMenu` for visual consistency:

### Outfit Preview Positioning
- **Size**: 128x192 pixels (same as outfit slot previews in FavoritesMenu)
- **Horizontal**: Left of the textbox, with 26px gap between preview right edge and textbox left border
- **Vertical**: Top of preview aligned flush with textbox visual top (border draws 16px above textBox.Y)
- **Content**: Farmer preview with seasonal background sprite based on outfit category

### Gap and Alignment Notes
- The 26px horizontal gap provides visual balance with the textbox border thickness
- The textbox draws its border above its Y position, so subtract 16px to align with visual top

### Reference Implementation
See `OutfitNamingMenu.UpdatePreviewPosition()` in `Menus/FavoritesMenu.cs` for the exact positioning logic:
```csharp
int previewWidth = 128;
int previewHeight = 192;
int gap = 26;  // Horizontal gap for visual balance
int textBoxBorderOffset = 16;  // TextBox border draws above its Y position

_previewBox = new Rectangle(
    textBox.X - gap - previewWidth,
    textBox.Y - textBoxBorderOffset,  // Align with textbox visual top
    previewWidth,
    previewHeight
);
```

### Farmer Rendering
Use `FarmerRenderer.draw()` with these parameters:
- Position: `new Vector2(_previewBox.Center.X - 32, _previewBox.Bottom - 160)`
- Scale: `0.8f`
- Always face forward: `farmer.faceDirection(2)`

### Window Resize Handling
The custom gamepad keyboard must handle window resize events to maintain proper positioning:
- Override `gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)`
- **IMPORTANT**: Call `base.gameWindowSizeChanged()` FIRST - this updates parent class position values like `yPositionOnScreen` which are used to draw elements like the title banner
- THEN recalculate and reapply custom positioning for elements you want to override
- See `OutfitNamingMenu.gameWindowSizeChanged()` and `ApplyCustomLayout()` for the keyboard-mode implementation

**Key insight from keyboard-mode implementation:**
When subclassing SDV menus, the parent class draws some elements using stored position values (e.g., NamingMenu draws its title banner at `yPositionOnScreen`). If you skip `base.gameWindowSizeChanged()`, those values never update and the elements render in the wrong position. The pattern is: let base update its positions, then override just the specific elements you want to customize.

**CRITICAL: Child menu resize propagation:**
The SDV framework does NOT automatically propagate `gameWindowSizeChanged()` to child menus set via `SetChildMenu()`. The parent menu must explicitly call:
```csharp
GetChildMenu()?.gameWindowSizeChanged(oldBounds, newBounds);
```
Without this, the child menu's resize handler never runs and elements stay at old positions.

## Status

**Priority**: Low (gamepad users can still rename outfits, just without visual context)

**Decision**: Defer until after playtesting with controller to assess actual need.
