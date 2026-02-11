using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the ghost preview that shows where a block will be placed on the grid.
/// </summary>
public class GhostPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridVisualizer gridVisualizer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color validColor = new Color(0.3f, 1f, 0.3f, 0.5f);   // Green transparent
    [SerializeField] private Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.5f); // Red transparent
    [SerializeField] private GameObject ghostCellPrefab;

    private List<GameObject> ghostCells = new List<GameObject>();
    private bool isVisible = false;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        
        if (gridVisualizer == null)
            gridVisualizer = FindObjectOfType<GridVisualizer>();
    }

    /// <summary>
    /// Shows the ghost preview at the specified grid position.
    /// </summary>
    public void ShowPreview(BlockData blockData, int gridX, int gridY, bool isValid)
    {
        if (blockData == null) return;

        // Clear previous preview
        ClearPreview();

        // Create ghost cells
        foreach (Vector2Int cellOffset in blockData.Shape)
        {
            int targetX = gridX + cellOffset.x;
            int targetY = gridY + cellOffset.y;

            // Skip cells outside grid
            if (!gridManager.IsInsideGrid(targetX, targetY))
                continue;

            // Get world position for this grid cell
            Vector3 worldPos = gridVisualizer.GetCellWorldPosition(targetX, targetY);

            // Create ghost cell
            GameObject ghostCell = CreateGhostCell(worldPos, isValid);
            ghostCells.Add(ghostCell);
        }

        isVisible = true;
    }

    /// <summary>
    /// Creates a single ghost cell at the specified position.
    /// </summary>
    private GameObject CreateGhostCell(Vector3 position, bool isValid)
    {
        GameObject cell;

        if (ghostCellPrefab != null)
        {
            cell = Instantiate(ghostCellPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            // Create simple sprite if no prefab assigned
            cell = new GameObject("GhostCell");
            cell.transform.position = position;
            cell.transform.parent = transform;
            
            SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSquareSprite();
            renderer.sortingOrder = 50; // Above grid, below dragged block
        }

        // Set color based on validity
        SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isValid ? validColor : invalidColor;
        }

        // Scale to match grid cell size
        cell.transform.localScale = Vector3.one * gridManager.CellSize;

        return cell;
    }

    /// <summary>
    /// Hides and destroys all ghost cells.
    /// </summary>
    public void ClearPreview()
    {
        foreach (GameObject cell in ghostCells)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }
        ghostCells.Clear();
        isVisible = false;
    }

    /// <summary>
    /// Creates a simple square sprite for ghost cells.
    /// </summary>
    private Sprite CreateSquareSprite()
    {
        // Create a simple white square texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    public bool IsVisible => isVisible;

    private void OnDestroy()
    {
        ClearPreview();
    }
}
