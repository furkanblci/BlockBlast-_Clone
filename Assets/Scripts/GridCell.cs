using UnityEngine;

/// <summary>
/// Represents a single cell in the grid.
/// Stores its grid coordinates and provides visual feedback.
/// </summary>
public class GridCell : MonoBehaviour
{
    [Header("Cell Properties")]
    [SerializeField] private int gridX;
    [SerializeField] private int gridY;

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Colors for different cell states
    private Color emptyColor = new Color(0.9f, 0.9f, 0.9f, 1f);  // Light gray
    private Color occupiedColor = new Color(0.3f, 0.5f, 0.8f, 1f);  // Blue

    public int GridX => gridX;
    public int GridY => gridY;

    private void Awake()
    {
        // Get SpriteRenderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Initializes the cell with its grid coordinates.
    /// </summary>
    /// <param name="x">Grid X coordinate</param>
    /// <param name="y">Grid Y coordinate</param>
    public void Initialize(int x, int y)
    {
        gridX = x;
        gridY = y;
        gameObject.name = $"Cell ({x}, {y})";
        
        // Set initial visual state
        SetVisualState(false);
    }

    /// <summary>
    /// Sets the visual state of the cell.
    /// </summary>
    /// <param name="occupied">True if cell should appear occupied</param>
    public void SetVisualState(bool occupied)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = occupied ? occupiedColor : emptyColor;
        }
    }

    /// <summary>
    /// Sets custom colors for the cell states.
    /// </summary>
    public void SetColors(Color empty, Color occupied)
    {
        emptyColor = empty;
        occupiedColor = occupied;
    }

    /// <summary>
    /// Highlights the cell (for hover/selection feedback).
    /// </summary>
    public void Highlight(bool highlight)
    {
        if (spriteRenderer != null && highlight)
        {
            spriteRenderer.color = new Color(1f, 1f, 0.7f, 1f);  // Light yellow
        }
    }
}
