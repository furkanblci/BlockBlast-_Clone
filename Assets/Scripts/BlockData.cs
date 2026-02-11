using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines a block shape using local coordinates.
/// Designers can create new blocks by right-clicking in Project window:
/// Create > Block Blast > Block Data
/// </summary>
[CreateAssetMenu(fileName = "New Block", menuName = "Block Blast/Block Data", order = 1)]
public class BlockData : ScriptableObject
{
    [Header("Block Identity")]
    [SerializeField] private string blockName = "New Block";
    [SerializeField] private int blockId;

    [Header("Shape Definition")]
    [Tooltip("List of cell positions that make up this block shape. (0,0) is the pivot/origin.")]
    [SerializeField] private List<Vector2Int> shape = new List<Vector2Int>();

    [Header("Visual Settings (Optional)")]
    [SerializeField] private Color blockColor = Color.white;

    // Cached values for performance
    private int? cachedWidth;
    private int? cachedHeight;
    private Vector2Int? cachedMinBounds;
    private Vector2Int? cachedMaxBounds;

    // Public properties
    public string BlockName => blockName;
    public int BlockId => blockId;
    public IReadOnlyList<Vector2Int> Shape => shape;
    public Color BlockColor => blockColor;

    /// <summary>
    /// Gets the width of the block (in grid cells).
    /// </summary>
    public int Width
    {
        get
        {
            if (!cachedWidth.HasValue)
            {
                CalculateBounds();
            }
            return cachedWidth.Value;
        }
    }

    /// <summary>
    /// Gets the height of the block (in grid cells).
    /// </summary>
    public int Height
    {
        get
        {
            if (!cachedHeight.HasValue)
            {
                CalculateBounds();
            }
            return cachedHeight.Value;
        }
    }

    /// <summary>
    /// Gets the minimum bounds of the block shape.
    /// </summary>
    public Vector2Int MinBounds
    {
        get
        {
            if (!cachedMinBounds.HasValue)
            {
                CalculateBounds();
            }
            return cachedMinBounds.Value;
        }
    }

    /// <summary>
    /// Gets the maximum bounds of the block shape.
    /// </summary>
    public Vector2Int MaxBounds
    {
        get
        {
            if (!cachedMaxBounds.HasValue)
            {
                CalculateBounds();
            }
            return cachedMaxBounds.Value;
        }
    }

    /// <summary>
    /// Calculates and caches the bounds of this block shape.
    /// </summary>
    private void CalculateBounds()
    {
        if (shape == null || shape.Count == 0)
        {
            cachedWidth = 0;
            cachedHeight = 0;
            cachedMinBounds = Vector2Int.zero;
            cachedMaxBounds = Vector2Int.zero;
            return;
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Vector2Int cell in shape)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        cachedMinBounds = new Vector2Int(minX, minY);
        cachedMaxBounds = new Vector2Int(maxX, maxY);
        cachedWidth = maxX - minX + 1;
        cachedHeight = maxY - minY + 1;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only validation that checks for issues without mutating data.
    /// Only logs warnings - never modifies the shape list.
    /// </summary>
    private void OnValidate()
    {
        // Clear cached values when data changes
        cachedWidth = null;
        cachedHeight = null;
        cachedMinBounds = null;
        cachedMaxBounds = null;

        // Validation: Check for duplicate cells (WARNING ONLY - NO MUTATION)
        if (shape != null && shape.Count > 0)
        {
            HashSet<Vector2Int> uniqueCells = new HashSet<Vector2Int>();
            List<Vector2Int> duplicates = new List<Vector2Int>();

            foreach (Vector2Int cell in shape)
            {
                if (!uniqueCells.Add(cell))
                {
                    duplicates.Add(cell);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"[{blockName}] Shape contains duplicate cells: {string.Join(", ", duplicates)}. " +
                    "Consider removing duplicates manually for cleaner data.");
            }
        }

        // Validation: Check for empty shape
        if (shape == null || shape.Count == 0)
        {
            Debug.LogWarning($"[{blockName}] Shape is empty. Block will not be visible.");
        }
    }
#endif

    /// <summary>
    /// Gets a rotated version of this block's shape.
    /// </summary>
    /// <param name="rotations">Number of 90-degree clockwise rotations (0-3)</param>
    /// <returns>List of rotated cell positions</returns>
    public List<Vector2Int> GetRotatedShape(int rotations)
    {
        rotations = rotations % 4;
        if (rotations < 0) rotations += 4;

        if (rotations == 0)
        {
            return new List<Vector2Int>(shape);
        }

        List<Vector2Int> rotatedShape = new List<Vector2Int>();

        foreach (Vector2Int cell in shape)
        {
            Vector2Int rotatedCell = cell;
            
            // Apply rotation rotations times
            for (int i = 0; i < rotations; i++)
            {
                // 90-degree clockwise rotation: (x, y) -> (y, -x)
                rotatedCell = new Vector2Int(rotatedCell.y, -rotatedCell.x);
            }
            
            rotatedShape.Add(rotatedCell);
        }

        return rotatedShape;
    }

    /// <summary>
    /// Checks if this block shape contains a specific cell position.
    /// </summary>
    public bool ContainsCell(Vector2Int cell)
    {
        return shape != null && shape.Contains(cell);
    }

    /// <summary>
    /// Gets the number of cells in this block.
    /// </summary>
    public int CellCount => shape != null ? shape.Count : 0;
}
