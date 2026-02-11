using UnityEngine;

/// <summary>
/// Handles the visual representation of the game grid.
/// Instantiates cell prefabs and maintains their positions.
/// </summary>
public class GridVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject cellPrefab;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellSpacing = 0.1f;

    // Store cell instances and their world positions
    private GridCell[,] cells;
    private Vector3[,] cellWorldPositions;

    private void Start()
    {
        // Get GridManager reference if not assigned
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! Please assign or add GridManager to scene.");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("Cell prefab not assigned!");
            return;
        }

        GenerateGrid();
    }

    /// <summary>
    /// Generates the visual grid by instantiating cell prefabs.
    /// </summary>
    private void GenerateGrid()
    {
        int width = gridManager.GridWidth;
        int height = gridManager.GridHeight;

        cells = new GridCell[width, height];
        cellWorldPositions = new Vector3[width, height];

        // Calculate the total size of the grid
        float totalWidth = width * cellSize + (width - 1) * cellSpacing;
        float totalHeight = height * cellSize + (height - 1) * cellSpacing;

        // Calculate starting position to center the grid
        Vector3 startPosition = new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);

        // Instantiate cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate world position for this cell
                Vector3 cellPosition = startPosition + new Vector3(
                    x * (cellSize + cellSpacing) + cellSize / 2f,
                    y * (cellSize + cellSpacing) + cellSize / 2f,
                    0f
                );

                // Instantiate the cell
                GameObject cellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform);
                cellObject.transform.localScale = Vector3.one * cellSize;

                // Get and initialize the GridCell component
                GridCell cell = cellObject.GetComponent<GridCell>();
                if (cell == null)
                {
                    cell = cellObject.AddComponent<GridCell>();
                }

                cell.Initialize(x, y);

                // Store references
                cells[x, y] = cell;
                cellWorldPositions[x, y] = cellPosition;
            }
        }

        Debug.Log($"Grid generated: {width}x{height} cells centered at origin.");
    }

    /// <summary>
    /// Gets the world position of a cell at the given grid coordinates.
    /// </summary>
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        if (gridManager.IsInsideGrid(x, y))
        {
            return cellWorldPositions[x, y];
        }

        Debug.LogWarning($"Cell ({x}, {y}) is outside grid bounds.");
        return Vector3.zero;
    }

    /// <summary>
    /// Gets the GridCell component at the given grid coordinates.
    /// </summary>
    public GridCell GetCell(int x, int y)
    {
        if (gridManager.IsInsideGrid(x, y))
        {
            return cells[x, y];
        }

        return null;
    }

    /// <summary>
    /// Updates the visual state of a specific cell based on GridManager data.
    /// </summary>
    public void UpdateCellVisual(int x, int y)
    {
        GridCell cell = GetCell(x, y);
        if (cell != null)
        {
            bool isEmpty = gridManager.IsCellEmpty(x, y);
            cell.SetVisualState(!isEmpty);
        }
    }

    /// <summary>
    /// Updates the color of a specific cell.
    /// </summary>
    public void UpdateCellColor(int x, int y, Color color)
    {
        GridCell cell = GetCell(x, y);
        if (cell != null)
        {
            // Assuming GridCell has a way to set color, if not we need to add it or access SpriteRenderer directly
            SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }
    }

    /// <summary>
    /// Updates all cell visuals based on GridManager data.
    /// </summary>
    public void UpdateAllCellVisuals()
    {
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                UpdateCellVisual(x, y);
            }
        }
    }

    /// <summary>
    /// Converts world position to grid coordinates.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        float closestDistance = float.MaxValue;
        Vector2Int closestCell = new Vector2Int(-1, -1);

        // Find the closest cell to the world position
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                float distance = Vector3.Distance(worldPosition, cellWorldPositions[x, y]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCell = new Vector2Int(x, y);
                }
            }
        }

        return closestCell;
    }
}
