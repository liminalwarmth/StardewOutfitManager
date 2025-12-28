using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewOutfitManager.Utils
{
    /// <summary>
    /// Utility methods for handling linked dresser inventories based on config settings.
    /// Supports individual dressers, touching (8-way adjacent) clusters, and same-building sharing.
    /// </summary>
    public static class DresserLinkingMethods
    {
        /// <summary>
        /// Get all dressers linked to the starting dresser based on current config mode.
        /// </summary>
        /// <param name="startingDresser">The dresser the player clicked on.</param>
        /// <returns>List of all linked dressers (always includes the starting dresser).</returns>
        public static List<StorageFurniture> GetLinkedDressers(StorageFurniture startingDresser)
        {
            var mode = StardewOutfitManager.Config.DresserInventorySharing;

            return mode switch
            {
                DresserSharingMode.Individual => new List<StorageFurniture> { startingDresser },
                DresserSharingMode.Touching => GetTouchingCluster(startingDresser),
                DresserSharingMode.SameBuilding => GetSameBuildingDressers(startingDresser),
                _ => new List<StorageFurniture> { startingDresser }
            };
        }

        /// <summary>
        /// Find all dressers in a touching cluster using 8-way adjacency (includes diagonals).
        /// Uses flood-fill (BFS) to find the entire connected cluster.
        /// </summary>
        private static List<StorageFurniture> GetTouchingCluster(StorageFurniture start)
        {
            var location = start.Location;
            if (location == null)
                return new List<StorageFurniture> { start };

            var result = new List<StorageFurniture>();
            var visited = new HashSet<StorageFurniture>();
            var queue = new Queue<StorageFurniture>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                // Check all 8 adjacent tiles for other dressers
                foreach (var neighbor in GetAdjacentDressers(current, location))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get all dressers adjacent to a given dresser (8-way including diagonals).
        /// Handles multi-tile furniture by checking all tiles the dresser occupies.
        /// </summary>
        private static IEnumerable<StorageFurniture> GetAdjacentDressers(StorageFurniture dresser, GameLocation location)
        {
            // Get all tiles this furniture occupies
            var dresserBounds = dresser.GetBoundingBox();
            var dresserTiles = new HashSet<Vector2>();
            for (int x = dresserBounds.Left / 64; x < dresserBounds.Right / 64; x++)
            {
                for (int y = dresserBounds.Top / 64; y < dresserBounds.Bottom / 64; y++)
                {
                    dresserTiles.Add(new Vector2(x, y));
                }
            }

            // Build set of 8-way adjacent tiles around the dresser's bounding box
            var adjacentTiles = new HashSet<Vector2>();
            foreach (var tile in dresserTiles)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        adjacentTiles.Add(new Vector2(tile.X + dx, tile.Y + dy));
                    }
                }
            }
            // Remove tiles that are part of the dresser itself
            adjacentTiles.ExceptWith(dresserTiles);

            // Track which dressers we've already yielded to avoid duplicates
            var foundDressers = new HashSet<StorageFurniture>();

            // Find dressers at adjacent tiles
            foreach (var furniture in location.furniture)
            {
                if (furniture is StorageFurniture sf &&
                    sf != dresser &&
                    IsDresser(sf) &&
                    !foundDressers.Contains(sf))
                {
                    var sfBounds = sf.GetBoundingBox();
                    bool isAdjacent = false;

                    // Check if any tile of this furniture is in our adjacent tiles set
                    for (int x = sfBounds.Left / 64; x < sfBounds.Right / 64 && !isAdjacent; x++)
                    {
                        for (int y = sfBounds.Top / 64; y < sfBounds.Bottom / 64 && !isAdjacent; y++)
                        {
                            if (adjacentTiles.Contains(new Vector2(x, y)))
                            {
                                isAdjacent = true;
                            }
                        }
                    }

                    if (isAdjacent)
                    {
                        foundDressers.Add(sf);
                        yield return sf;
                    }
                }
            }
        }

        /// <summary>
        /// Get all dressers in the same building (FarmHouse or Cabin).
        /// Falls back to Individual mode for non-building locations.
        /// </summary>
        private static List<StorageFurniture> GetSameBuildingDressers(StorageFurniture start)
        {
            var location = start.Location;
            if (location == null)
                return new List<StorageFurniture> { start };

            // Only apply building sharing to FarmHouse/Cabin
            if (location is not FarmHouse)
            {
                return new List<StorageFurniture> { start };
            }

            var result = new List<StorageFurniture>();

            foreach (var furniture in location.furniture)
            {
                if (furniture is StorageFurniture sf && IsDresser(sf))
                {
                    result.Add(sf);
                }
            }

            // Ensure we always have at least the starting dresser
            if (!result.Contains(start))
            {
                result.Add(start);
            }

            return result;
        }

        /// <summary>
        /// Check if a StorageFurniture is a dresser.
        /// Dressers in SDV are StorageFurniture with furniture_type "dresser".
        /// We check by looking at the furniture data's type field.
        /// </summary>
        private static bool IsDresser(StorageFurniture furniture)
        {
            // Check furniture type from the data - dressers have Type == "dresser"
            // In SDV 1.6, Furniture.furniture_type is the parsed type from Data/Furniture
            return furniture.furniture_type.Value == Furniture.dresser;
        }

        /// <summary>
        /// Aggregate items from multiple dressers into a single list.
        /// </summary>
        public static List<Item> GetCombinedInventory(List<StorageFurniture> dressers)
        {
            var combined = new List<Item>();
            foreach (var dresser in dressers)
            {
                combined.AddRange(dresser.heldItems);
            }
            return combined;
        }

        /// <summary>
        /// Check if any dresser in the list is currently locked by another player.
        /// </summary>
        public static bool AnyDresserLocked(List<StorageFurniture> dressers)
        {
            return dressers.Any(d => d.mutex.IsLocked());
        }

        /// <summary>
        /// Attempt to lock all dressers in the list synchronously.
        /// Uses a sequential locking strategy with rollback on failure.
        /// </summary>
        /// <param name="dressers">List of dressers to lock.</param>
        /// <returns>True if all dressers were successfully locked, false otherwise.</returns>
        public static bool TryLockAllDressers(List<StorageFurniture> dressers)
        {
            if (dressers == null || dressers.Count == 0)
                return false;

            var lockedDressers = new List<StorageFurniture>();

            foreach (var dresser in dressers)
            {
                // Check if already locked by someone else
                if (dresser.mutex.IsLocked())
                {
                    // Rollback: release any locks we acquired
                    foreach (var locked in lockedDressers)
                    {
                        try { locked.mutex.ReleaseLock(); }
                        catch { /* ignore release errors during rollback */ }
                    }
                    return false;
                }

                // Request the lock - this is synchronous in local context
                // The lock is acquired immediately for the local player
                dresser.mutex.RequestLock(delegate { });

                // In multiplayer, RequestLock may not immediately succeed
                // Check if we actually hold the lock now
                if (dresser.mutex.IsLockHeld())
                {
                    lockedDressers.Add(dresser);
                }
                else
                {
                    // Lock wasn't acquired (likely held by remote player)
                    // Rollback all previously acquired locks
                    foreach (var locked in lockedDressers)
                    {
                        try { locked.mutex.ReleaseLock(); }
                        catch { /* ignore release errors during rollback */ }
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Release all locks held on the given dressers.
        /// </summary>
        public static void ReleaseAllDressers(List<StorageFurniture> dressers)
        {
            if (dressers == null) return;

            foreach (var dresser in dressers)
            {
                try
                {
                    if (dresser.mutex.IsLockHeld())
                    {
                        dresser.mutex.ReleaseLock();
                    }
                }
                catch (Exception ex)
                {
                    StardewOutfitManager.ModMonitor.Log($"Error releasing dresser lock: {ex.Message}", LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// Find which dresser in the linked list contains a specific item and remove it.
        /// </summary>
        /// <param name="dressers">List of linked dressers to search.</param>
        /// <param name="item">The item to find and remove.</param>
        /// <returns>True if item was found and removed, false otherwise.</returns>
        public static bool RemoveItemFromLinkedDressers(List<StorageFurniture> dressers, Item item)
        {
            if (dressers == null || item == null) return false;

            foreach (var dresser in dressers)
            {
                if (dresser.heldItems.Contains(item))
                {
                    dresser.heldItems.Remove(item);
                    return true;
                }
            }
            return false;
        }
    }
}
