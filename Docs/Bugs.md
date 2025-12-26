# Stardew Outfit Manager - Known Bugs

## Display Farmer Recolor Bug

**Status:** Open  
**Location:** `Utils/ModTools.cs` - `drawFarmerScaled()` method

**Issue:** Reflection call to `executeRecolorActions` not working correctly on display farmer (fake farmers created via `CreateFakeModelFarmer`).

**Analysis:**
- `executeRecolorActions` is called (lines 28, 167) but texture pixel modifications may not apply
- `baseTexture` may not initialize correctly for fake farmers
- Duplicate call exists (commented line 27, active line 28)

**Possible Fixes to Try:**
1. Check if `baseTexture` is null before drawing and force initialization
2. Call `farmer.FarmerRenderer.draw()` once before custom draw to force baseTexture setup
3. Use a reverse patch pattern like FashionSense does
4. Explicitly set dirty flags after `changeSkinColor`/`changeEyeColor` calls

**Needs:** In-game testing to verify exact symptom (wrong colors? missing body parts? grayscale?)
