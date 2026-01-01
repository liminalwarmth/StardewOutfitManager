# Quick Audit Checklist

Use this checklist when auditing code before commits.

## Pre-Commit Multiplayer Checklist

### State Management
- [ ] New static fields wrapped in `PerScreen<T>` if mutable
- [ ] PerScreen fields marked `readonly`
- [ ] No static mutable collections without PerScreen

### Context Checks
- [ ] Event handlers have `Context.IsWorldReady` guard
- [ ] Save operations check `Context.IsMainPlayer`
- [ ] Player interaction checks `Context.IsPlayerFree`

### Mutex (Shared Furniture)
- [ ] Furniture access uses `mutex.RequestLock()`
- [ ] Menu releases mutex in `emergencyShutDown()`
- [ ] Menu releases mutex in `cleanupBeforeExit()`

### Data Storage
- [ ] Player data paths include: Name + SaveFolder + MultiplayerID
- [ ] modData keys prefixed with mod UniqueID
- [ ] No hardcoded save paths that could collide

## Pre-Commit Gamepad Checklist

### Navigation IDs
- [ ] All ClickableComponents have `myID`
- [ ] All ClickableComponents have `leftNeighborID`
- [ ] All ClickableComponents have `rightNeighborID`
- [ ] All ClickableComponents have `upNeighborID`
- [ ] All ClickableComponents have `downNeighborID`
- [ ] IDs are unique (no duplicates)
- [ ] No dead-end navigation paths

### Component Registration
- [ ] `populateClickableComponentList()` includes all components
- [ ] `snapToDefaultClickableComponent()` called after setup
- [ ] Dynamic components re-registered when added

### Focus
- [ ] Menu has sensible default focus
- [ ] Actions return focus to logical element
- [ ] Child menus handle focus transfer

## Files to Always Audit

When modifying these directories, always run the audit:

| Directory | Concerns |
|-----------|----------|
| `Managers/` | PerScreen usage, player data keying |
| `Menus/` | Gamepad navigation, mutex, PerScreen |
| `Models/` | Data structure changes affect save compatibility |
| `Utils/` | Shared utilities might affect multiple players |

## Known Good Patterns in This Codebase

### PlayerManager.cs
```csharp
// Correct PerScreen usage
internal readonly PerScreen<MenuManager> menuManager = new();
internal readonly PerScreen<FavoritesData> favoritesData = new();
```

### WardrobeMenu.cs
```csharp
// Correct navigation ID setup
internal const int LABELS = 10000;
internal const int PORTRAIT = 20000;
internal const int CATEGORIES = 30000;
```

### StardewOutfitManager.cs
```csharp
// Correct mutex pattern
dresser.mutex.RequestLock(delegate {
    Game1.activeClickableMenu = new WardrobeMenu();
});
```

## Common Issues to Watch For

1. **Forgetting PerScreen for "simple" state** - Even an `int currentIndex` needs PerScreen if it varies per player

2. **Mutex release only in one exit path** - Always implement both `emergencyShutDown()` AND `cleanupBeforeExit()`

3. **Hardcoded -1 for no neighbor** - Use -99999, not -1 or 0

4. **populateClickableComponentList not overridden** - If you add custom components, override and add them

5. **Shared collections** - A `List<Item>` that's static needs PerScreen if different players could have different contents
