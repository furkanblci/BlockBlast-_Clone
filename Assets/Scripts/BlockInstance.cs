using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a block instance in the game world.
/// Generates visual representation based on a BlockData ScriptableObject.
/// </summary>
public class BlockInstance : MonoBehaviour
{
    [Header("Block Configuration")]
    [SerializeField] private BlockData blockData;
    [SerializeField] private GameObject cellPrefab;

    [Header("Visual Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private bool useBlockDataColor = true;
    [SerializeField] private Color overrideColor = Color.white;

    [Header("Drag & Drop Settings")]
    [SerializeField] private float dragAlpha = 0.7f;
    [SerializeField] private float cursorOffset = 0.5f; // Offset above cursor
    
    [Header("Animation Settings")]
    [SerializeField] private float returnDuration = 0.3f;  // Duration of return animation
    [SerializeField] private AnimationCurve returnEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlacementValidator placementValidator;
    [SerializeField] private GhostPreview ghostPreview;
    [SerializeField] private SpawnManager spawnManager;

    // Store references to instantiated cell visuals
    private List<GameObject> cellObjects = new List<GameObject>();
    
    // Remember spawn position for returning when placement fails
    private Vector3 spawnPosition;

    // Drag & drop state
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 initialPosition;
    private Color originalColor;
    private Coroutine returnCoroutine;
    private int originalSortingOrder;

    public BlockData BlockData => blockData;
    public Vector3 SpawnPosition => spawnPosition;
    public bool IsDragging => isDragging;

    private void Start()
    {
        // Find references if not assigned
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        if (placementValidator == null)
            placementValidator = FindObjectOfType<PlacementValidator>();
        if (ghostPreview == null)
            ghostPreview = FindObjectOfType<GhostPreview>();
        if (spawnManager == null)
            spawnManager = FindObjectOfType<SpawnManager>();

        if (blockData != null)
        {
            GenerateVisuals();
        }
        else
        {
            Debug.LogWarning($"BlockInstance '{gameObject.name}' has no BlockData assigned!");
        }
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Manages input detection using Raycast to select specific cells.
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to check if we clicked on THIS block's cells
            Vector3 mousePos = GetMouseWorldPosition();
            // Using RaycastAll to ensure we hit the correct collider if overlapped
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                StartDrag();
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    /// <summary>
    /// Sets the block data and regenerates visuals.
    /// </summary>
    public void SetBlockData(BlockData data)
    {
        blockData = data;
        GenerateVisuals();
    }

    /// <summary>
    /// Generates the visual representation of the block.
    /// Creates child GameObjects for each cell in the block shape.
    /// </summary>
    public void GenerateVisuals()
    {
        // Clear existing visuals
        ClearVisuals();

        if (blockData == null || blockData.Shape == null || blockData.Shape.Count == 0)
        {
            Debug.LogWarning("Cannot generate visuals: BlockData or shape is empty.");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogWarning("Cannot generate visuals: Cell prefab is not assigned.");
            return;
        }

        // Determine color to use
        Color cellColor = useBlockDataColor ? blockData.BlockColor : overrideColor;

        // Instantiate a cell for each position in the shape
        foreach (Vector2Int cellPos in blockData.Shape)
        {
            // Calculate local position based on grid coordinates
            Vector3 localPosition = new Vector3(
                cellPos.x * cellSize,
                cellPos.y * cellSize,
                0f
            );

            // Instantiate the cell
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.transform.localPosition = localPosition;
            cell.transform.localScale = Vector3.one * cellSize;
            cell.name = $"Cell ({cellPos.x}, {cellPos.y})";

            // Set color if the cell has a SpriteRenderer
            SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = cellColor;
            }

            // Add collider to each cell for accurate raycast detection
            BoxCollider2D collider = cell.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * cellSize;

            // Store reference
            cellObjects.Add(cell);
        }

        Debug.Log($"Generated {cellObjects.Count} cells for block '{blockData.BlockName}'");
    }

    /// <summary>
    /// Sets the color of all cells.
    /// </summary>
    public void SetColor(Color color)
    {
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Sets the transparency of all cells.
    /// </summary>
    public void SetAlpha(float alpha)
    {
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Gets all cell positions in world space.
    /// </summary>
    public List<Vector3> GetWorldCellPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                positions.Add(cell.transform.position);
            }
        }
        
        return positions;
    }

    /// <summary>
    /// Gets all cell positions in local space relative to this block instance.
    /// </summary>
    public List<Vector2Int> GetLocalCellPositions()
    {
        return blockData != null ? new List<Vector2Int>(blockData.Shape) : new List<Vector2Int>();
    }

    /// <summary>
    /// Sets the spawn position of this block (where it should return if placement fails).
    /// </summary>
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
        transform.position = position;
    }

    /// <summary>
    /// Returns the block to its original spawn position with a smooth animation.
    /// </summary>
    public void ReturnToSpawnPosition()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(AnimateToSpawnPosition());
    }

    /// <summary>
    /// Coroutine that animates the block back to its spawn position.
    /// </summary>
    private System.Collections.IEnumerator AnimateToSpawnPosition()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = spawnPosition;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float curveValue = returnEase.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            yield return null;
        }

        transform.position = targetPos;
        returnCoroutine = null;
    }

    // ========== DRAG & DROP SYSTEM ==========

    /// <summary>
    /// Starts the dragging process.
    /// </summary>
    private void StartDrag()
    {
        if (blockData == null) return;
        
        // Stop return animation if running (allows catching block mid-air)
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // Store initial position
        initialPosition = transform.position;

        // Calculate offset between mouse and block position
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        
        // Add cursor offset to keep block above cursor
        dragOffset.y += cursorOffset;

        // Start dragging
        isDragging = true;

        // Visual feedback: make block semi-transparent
        if (cellObjects.Count > 0)
        {
            SpriteRenderer firstCellRenderer = cellObjects[0].GetComponent<SpriteRenderer>();
            if (firstCellRenderer != null)
            {
                originalColor = firstCellRenderer.color;
                originalSortingOrder = firstCellRenderer.sortingOrder;
            }
        }

        SetAlpha(dragAlpha);
        SetSortingOrder(100); // Bring to front
    }

    /// <summary>
    /// Called every frame while dragging.
    /// </summary>
    private void UpdateDrag()
    {
        // Get mouse position in world space
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // Apply offset for smooth following
        Vector3 targetPosition = mouseWorldPos + dragOffset;
        targetPosition.z = 0f; // Keep Z at 0

        // Move block to mouse position
        transform.position = targetPosition;

        // Update ghost preview
        UpdateGhostPreview(mouseWorldPos);
    }

    /// <summary>
    /// Ends the dragging process.
    /// </summary>
    private void EndDrag()
    {
        isDragging = false;
        
        // Stop return animation if it was running (edge case)
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // Clear ghost preview
        if (ghostPreview != null)
            ghostPreview.ClearPreview();

        // Reset visual state
        SetAlpha(1f);
        SetSortingOrder(originalSortingOrder);

        // CHECK VALIDATION & PLACE
        if (TryPlaceBlock())
        {
            // Placement successful!
            Debug.Log("Block placed successfully!");
        }
        else
        {
            // placement failed, return to spawn
            ReturnToSpawnPosition();
            Debug.Log("Invalid placement, returning to spawn.");
        }
    }

    /// <summary>
    /// Tries to place the block on the grid at the current position.
    /// </summary>
    private bool TryPlaceBlock()
    {
        if (gridManager == null || placementValidator == null)
            return false;

        // Get mouse position
        Vector3 mousePos = GetMouseWorldPosition();
        
        // Convert to grid coordinates
        if (gridManager.GetCellFromWorldPosition(mousePos, out int gridX, out int gridY))
        {
            // Check validation
            if (placementValidator.CanPlaceBlock(blockData, gridX, gridY))
            {
                // Place it (Logic to be implemented in GridManager/Controller)
                // For now, snap to grid position visuals
                PlaceVisualsAt(gridX, gridY);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Helper to snap block visuals to grid and finalize placement.
    /// </summary>
    private void PlaceVisualsAt(int gridX, int gridY)
    {
        // 1. Update Grid Data
        if (gridManager != null)
        {
            gridManager.PlaceBlock(blockData, gridX, gridY);
        }

        // 2. Notify Spawn Manager
        if (spawnManager != null)
        {
            spawnManager.RemoveBlock(this);
        }

        // 3. Destroy this block instance as it's now part of the grid
        Destroy(gameObject);
    }

    /// <summary>
    /// Gets the mouse position in world space.
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    /// <summary>
    /// Sets the sorting order of all cell sprites.
    /// </summary>
    private void SetSortingOrder(int order)
    {
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingOrder = order;
                }
            }
        }
    }
    /// <summary>
    /// Updates the ghost preview based on current mouse position.
    /// </summary>
    private void UpdateGhostPreview(Vector3 mouseWorldPos)
    {
        if (ghostPreview == null || placementValidator == null || gridManager == null)
            return;

        // Convert mouse position to grid coordinates
        Vector2Int gridPos = gridManager.GetCellFromWorldPosition(mouseWorldPos);

        // Check if valid grid position
        if (gridPos.x == -1)
        {
            ghostPreview.ClearPreview();
            return;
        }

        // Check if placement is valid
        bool isValid = placementValidator.CanPlaceBlock(blockData, gridPos.x, gridPos.y);

        // Show ghost preview
        ghostPreview.ShowPreview(blockData, gridPos.x, gridPos.y, isValid);
    }

    private void OnDestroy()
    {
        // No need to manually destroy children, Unity handles it.
        // Clearing list reference is enough.
        cellObjects.Clear();
    }

    /// <summary>
    /// Clears all visual cell objects.
    /// </summary>
    public void ClearVisuals()
    {
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(cell);
                }
                else
                {
                    DestroyImmediate(cell);
                }
            }
        }
        cellObjects.Clear();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor helper: Regenerate visuals when values change in Inspector.
    /// </summary>
    private void OnValidate()
    {
        // Only regenerate if in play mode and we have data
        if (Application.isPlaying && blockData != null && cellPrefab != null)
        {
            GenerateVisuals();
        }
    }
#endif
}
