using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the game grid state and provides methods for grid operations.
/// The grid uses a 2D array where 0 = empty and 1 = occupied.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    
    [Header("Visual Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellSpacing = 0.1f;

    [Header("References")]
    [SerializeField] private GridVisualizer gridVisualizer;

    // Grid data: 0 = empty, 1 = occupied
    private int[,] grid;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;
    public float CellSpacing => cellSpacing;

    private void Awake()
    {
        InitializeGrid();
    }

    private void Start()
    {
        if (gridVisualizer == null)
        {
            gridVisualizer = FindObjectOfType<GridVisualizer>();
        }
    }

    /// <summary>
    /// Initializes the grid with all cells set to empty (0).
    /// </summary>
    private void InitializeGrid()
    {
        grid = new int[gridWidth, gridHeight];
        
        // Initialize all cells to empty (0)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = 0;
            }
        }
    }

    /// <summary>
    /// Checks if a cell at the given coordinates is empty.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if the cell is empty, false otherwise</returns>
    public bool IsCellEmpty(int x, int y)
    {
        if (!IsInsideGrid(x, y))
        {
            Debug.LogWarning($"Cell ({x}, {y}) is outside grid bounds.");
            return false;
        }

        return grid[x, y] == 0;
    }

    /// <summary>
    /// Sets the state of a cell at the given coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="occupied">True to set as occupied (1), false to set as empty (0)</param>
    public void SetCell(int x, int y, bool occupied)
    {
        if (!IsInsideGrid(x, y))
        {
            Debug.LogWarning($"Cannot set cell ({x}, {y}) - outside grid bounds.");
            return;
        }

        grid[x, y] = occupied ? 1 : 0;
    }

    /// <summary>
    /// Checks if the given coordinates are inside the grid bounds.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if inside grid, false otherwise</returns>
    public bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    /// <summary>
    /// Gets the state of a cell at the given coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>1 if occupied, 0 if empty, -1 if out of bounds</returns>
    public int GetCell(int x, int y)
    {
        if (!IsInsideGrid(x, y))
        {
            return -1;
        }

        return grid[x, y];
    }

    /// <summary>
    /// Clears the entire grid, setting all cells to empty.
    /// </summary>
    public void ClearGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = 0;
                if (gridVisualizer != null)
                {
                    gridVisualizer.UpdateCellVisual(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Places a block on the grid, marking cells as occupied.
    /// </summary>
    public void PlaceBlock(BlockData block, int gridX, int gridY)
    {
        if (block == null) return;

        foreach (Vector2Int cellOffset in block.Shape)
        {
            int targetX = gridX + cellOffset.x;
            int targetY = gridY + cellOffset.y;

            if (IsInsideGrid(targetX, targetY))
            {
                grid[targetX, targetY] = 1; // Mark as occupied
                
                // Update visual color of the grid cell
                if (gridVisualizer != null)
                {
                    gridVisualizer.UpdateCellColor(targetX, targetY, block.BlockColor);
                }
            }
        }

        // 1. Add Placement Score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPlacementScore(block);
        }

        // 2. Check for completed lines
        int linesCleared = CheckForLines();

        // 3. Process Line Clear Score (and handle combo reset if 0)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ProcessLineClears(linesCleared);
        }

        // 4. Check for Game Over condition
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckGameStatus();
        }
    }

    /// <summary>
    /// Checks for full rows and columns and clears them.
    /// Returns the total number of lines cleared.
    /// </summary>
    public int CheckForLines()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        // Check rows
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsRowFull(y))
            {
                rowsToClear.Add(y);
            }
        }

        // Check columns
        for (int x = 0; x < gridWidth; x++)
        {
            if (IsColumnFull(x))
            {
                colsToClear.Add(x);
            }
        }

        int totalLines = rowsToClear.Count + colsToClear.Count;

        if (totalLines > 0)
        {
            // Clear collected rows
            foreach (int y in rowsToClear)
            {
                ClearRow(y);
            }

            // Clear collected columns
            foreach (int x in colsToClear)
            {
                ClearColumn(x);
            }
            
            Debug.Log($"Cleared {totalLines} lines ({rowsToClear.Count} rows, {colsToClear.Count} cols)");
        }

        return totalLines;
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == 0) return false;
        }
        return true;
    }

    private bool IsColumnFull(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (grid[x, y] == 0) return false;
        }
        return true;
    }

    private void ClearRow(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            grid[x, y] = 0;
            if (gridVisualizer != null)
            {
                gridVisualizer.UpdateCellVisual(x, y); // Reset to empty visual
            }
        }
    }

    private void ClearColumn(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            grid[x, y] = 0;
            if (gridVisualizer != null)
            {
                gridVisualizer.UpdateCellVisual(x, y); // Reset to empty visual
            }
        }
    }

    /// <summary>
    /// Debug method to print the grid state to console.
    /// </summary>
    public void PrintGrid()
    {
        string gridString = "Grid State:\n";
        
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                gridString += grid[x, y] + " ";
            }
            gridString += "\n";
        }
        
        Debug.Log(gridString);
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    /// <param name="worldPos">World position to convert</param>
    /// <param name="gridX">Output: Grid X coordinate</param>
    /// <param name="gridY">Output: Grid Y coordinate</param>
    /// <returns>True if the position is within grid bounds, false otherwise</returns>
    public bool GetCellFromWorldPosition(Vector3 worldPos, out int gridX, out int gridY)
    {
        // Calculate the total size of the grid
        float totalWidth = gridWidth * cellSize + (gridWidth - 1) * cellSpacing;
        float totalHeight = gridHeight * cellSize + (gridHeight - 1) * cellSpacing;

        // Calculate the bottom-left corner of the grid (grid is centered at origin)
        Vector3 gridOrigin = new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);

        // Calculate position relative to grid origin
        Vector3 localPos = worldPos - gridOrigin;

        // Calculate grid coordinates based on cell size and spacing
        float cellWithSpacing = cellSize + cellSpacing;
        
        // Convert to grid coordinates
        gridX = Mathf.FloorToInt(localPos.x / cellWithSpacing);
        gridY = Mathf.FloorToInt(localPos.y / cellWithSpacing);

        // Check if within bounds
        return IsInsideGrid(gridX, gridY);
    }

    /// <summary>
    /// Converts a world position to grid coordinates (alternative signature).
    /// Returns Vector2Int with (-1, -1) if outside grid bounds.
    /// </summary>
    /// <param name="worldPos">World position to convert</param>
    /// <returns>Grid coordinates as Vector2Int, or (-1, -1) if outside grid</returns>
    public Vector2Int GetCellFromWorldPosition(Vector3 worldPos)
    {
        if (GetCellFromWorldPosition(worldPos, out int x, out int y))
        {
            return new Vector2Int(x, y);
        }
        
        return new Vector2Int(-1, -1);
    }
}
