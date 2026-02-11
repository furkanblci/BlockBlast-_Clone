using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the spawning of blocks in the game.
/// Spawns 3 random blocks at designated spawn positions.
/// Handles block regeneration when all blocks are placed.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Block Configuration")]
    [SerializeField] private List<BlockData> availableBlocks = new List<BlockData>();
    [SerializeField] private GameObject blockPrefab; // Prefab with BlockInstance component
    [SerializeField] private GameObject cellPrefab;  // Cell prefab for BlockInstance

    [Header("Spawn Settings")]
    [SerializeField] private int numberOfSpawnSlots = 3;
    [SerializeField] private Vector3 spawnAreaCenter = new Vector3(0f, -4f, 0f);
    [SerializeField] private float spacingBetweenBlocks = 3f;

    [Header("Visual Settings")]
    [SerializeField] private float blockCellSize = 1f;

    // Currently spawned blocks
    private List<BlockInstance> spawnedBlocks = new List<BlockInstance>();
    private Vector3[] spawnPositions;

    public List<BlockInstance> SpawnedBlocks => spawnedBlocks;

    private void Awake()
    {
        CalculateSpawnPositions();
    }

    private void Start()
    {
        // Only spawn automatically if GameManager is not present (to avoid double spawning)
        if (GameManager.Instance == null)
        {
            SpawnInitialBlocks();
        }
    }

    /// <summary>
    /// Calculates the spawn positions for the block slots.
    /// </summary>
    private void CalculateSpawnPositions()
    {
        spawnPositions = new Vector3[numberOfSpawnSlots];

        // Calculate positions evenly spaced around the spawn area center
        float totalWidth = (numberOfSpawnSlots - 1) * spacingBetweenBlocks;
        float startX = spawnAreaCenter.x - totalWidth / 2f;

        for (int i = 0; i < numberOfSpawnSlots; i++)
        {
            spawnPositions[i] = new Vector3(
                startX + i * spacingBetweenBlocks,
                spawnAreaCenter.y,
                0f
            );
        }
    }

    /// <summary>
    /// Spawns the initial set of blocks.
    /// </summary>
    private void SpawnInitialBlocks()
    {
        for (int i = 0; i < numberOfSpawnSlots; i++)
        {
            SpawnBlockAtSlot(i);
        }

        Debug.Log($"Spawned {spawnedBlocks.Count} blocks");

        // Check for Game Over condition with new blocks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckGameStatus();
        }
    }

    /// <summary>
    /// Spawns a random block at the specified slot index.
    /// </summary>
    private void SpawnBlockAtSlot(int slotIndex)
    {
        if (availableBlocks.Count == 0)
        {
            Debug.LogWarning("No available blocks to spawn!");
            return;
        }

        if (slotIndex < 0 || slotIndex >= numberOfSpawnSlots)
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return;
        }

        // Get random block data
        BlockData randomBlockData = GetRandomBlockData();

        // Create block instance
        BlockInstance block = CreateBlockInstance(randomBlockData, spawnPositions[slotIndex]);
        
        if (block != null)
        {
            spawnedBlocks.Add(block);
        }
    }

    /// <summary>
    /// Creates a BlockInstance at the specified position.
    /// </summary>
    private BlockInstance CreateBlockInstance(BlockData blockData, Vector3 position)
    {
        GameObject blockObj;
        BlockInstance blockInstance;

        // Check if we should use a prefab or create from scratch
        if (blockPrefab != null)
        {
            blockObj = Instantiate(blockPrefab, position, Quaternion.identity, transform);
            blockInstance = blockObj.GetComponent<BlockInstance>();
            
            if (blockInstance == null)
            {
                blockInstance = blockObj.AddComponent<BlockInstance>();
            }
        }
        else
        {
            // Create new GameObject with BlockInstance
            blockObj = new GameObject($"Block_{blockData.BlockName}");
            blockObj.transform.position = position;
            blockObj.transform.parent = transform;
            blockInstance = blockObj.AddComponent<BlockInstance>();
        }

        // Configure the block instance
        blockInstance.SetBlockData(blockData);
        blockInstance.SetSpawnPosition(position);

        // Note: BlockInstance will generate visuals in Start() if cellPrefab is assigned
        // For runtime creation, we need to set it manually
        if (cellPrefab != null)
        {
            // Use reflection or make cellPrefab/cellSize public in BlockInstance
            // For now, BlockInstance needs to have these set in prefab or assigned manually
        }

        return blockInstance;
    }

    /// <summary>
    /// Gets a random BlockData from the available blocks list.
    /// </summary>
    private BlockData GetRandomBlockData()
    {
        int randomIndex = Random.Range(0, availableBlocks.Count);
        return availableBlocks[randomIndex];
    }

    /// <summary>
    /// Removes a block from the spawned blocks list (called when block is placed on grid).
    /// </summary>
    public void RemoveBlock(BlockInstance block)
    {
        if (spawnedBlocks.Contains(block))
        {
            spawnedBlocks.Remove(block);
            
            // Check if all blocks are used up
            if (spawnedBlocks.Count == 0)
            {
                OnAllBlocksUsed();
            }
        }
    }

    /// <summary>
    /// Called when all spawned blocks have been placed.
    /// Spawns a new set of blocks.
    /// </summary>
    private void OnAllBlocksUsed()
    {
        Debug.Log("All blocks used! Spawning new set...");
        SpawnInitialBlocks();
    }

    /// <summary>
    /// Checks if any of the spawned blocks can be placed on the grid.
    /// This will be used for Game Over detection.
    /// </summary>
    public bool CanAnyBlockBePlaced(GridManager gridManager)
    {
        // This will be implemented later when we have placement validation
        // For now, return true
        return true;
    }

    /// <summary>
    /// Forces a refresh of all blocks (useful for testing).
    /// </summary>
    public void RefreshAllBlocks()
    {
        ClearAllBlocks();
        SpawnInitialBlocks();
    }

    /// <summary>
    /// Clears all currently spawned blocks.
    /// </summary>
    private void ClearAllBlocks()
    {
        foreach (BlockInstance block in spawnedBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        spawnedBlocks.Clear();
    }

    private void OnDestroy()
    {
        ClearAllBlocks();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Visualize spawn positions in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            // Calculate temporary positions for visualization
            Vector3[] tempPositions = new Vector3[numberOfSpawnSlots];
            float totalWidth = (numberOfSpawnSlots - 1) * spacingBetweenBlocks;
            float startX = spawnAreaCenter.x - totalWidth / 2f;

            for (int i = 0; i < numberOfSpawnSlots; i++)
            {
                tempPositions[i] = new Vector3(
                    startX + i * spacingBetweenBlocks,
                    spawnAreaCenter.y,
                    0f
                );
            }
            spawnPositions = tempPositions;
        }

        // Draw spawn positions
        Gizmos.color = Color.yellow;
        foreach (Vector3 pos in spawnPositions)
        {
            Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
        }

        // Draw spawn area center
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnAreaCenter, 0.2f);
    }
#endif
}
