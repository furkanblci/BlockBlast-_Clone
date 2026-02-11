using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles validation of block placement on the grid.
/// Checks if a block can be placed at a specific position.
/// </summary>
public class PlacementValidator : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    /// <summary>
    /// Checks if a block can be placed at the specified grid position.
    /// </summary>
    /// <param name="blockData">The block to place</param>
    /// <param name="gridX">Grid X coordinate (anchor position)</param>
    /// <param name="gridY">Grid Y coordinate (anchor position)</param>
    /// <returns>True if placement is valid, false otherwise</returns>
    public bool CanPlaceBlock(BlockData blockData, int gridX, int gridY)
    {
        if (blockData == null || gridManager == null)
            return false;

        // Check each cell of the block
        foreach (Vector2Int cellOffset in blockData.Shape)
        {
            int targetX = gridX + cellOffset.x;
            int targetY = gridY + cellOffset.y;

            // Check if cell is within grid bounds
            if (!gridManager.IsInsideGrid(targetX, targetY))
                return false;

            // Check if cell is empty
            if (!gridManager.IsCellEmpty(targetX, targetY))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the grid positions that would be occupied by placing this block.
    /// </summary>
    public List<Vector2Int> GetOccupiedPositions(BlockData blockData, int gridX, int gridY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        if (blockData == null)
            return positions;

        foreach (Vector2Int cellOffset in blockData.Shape)
        {
            positions.Add(new Vector2Int(gridX + cellOffset.x, gridY + cellOffset.y));
        }

        return positions;
    }

    /// <summary>
    /// Converts world position to grid coordinates and checks if placement is valid.
    /// </summary>
    public bool CanPlaceBlockAtWorldPosition(BlockData blockData, Vector3 worldPosition, out int gridX, out int gridY)
    {
        if (gridManager.GetCellFromWorldPosition(worldPosition, out gridX, out gridY))
        {
            return CanPlaceBlock(blockData, gridX, gridY);
        }

        gridX = -1;
        gridY = -1;
        return false;
    }
}
