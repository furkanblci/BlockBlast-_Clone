using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the overall game state (Playing, GameOver, etc.).
/// Checks for game over conditions after each move.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private PlacementValidator placementValidator;
    [SerializeField] private ScoreManager scoreManager;

    // Game state
    public bool IsGameOver { get; private set; }

    // Events
    public delegate void GameStateChangedHelper(bool isGameOver);
    public event GameStateChangedHelper OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Find references if not assigned
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (spawnManager == null) spawnManager = FindObjectOfType<SpawnManager>();
        if (placementValidator == null) placementValidator = FindObjectOfType<PlacementValidator>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();

        StartGame();
    }

    public void StartGame()
    {
        IsGameOver = false;
        
        // Reset systems
        if (gridManager != null) gridManager.ClearGrid();
        if (scoreManager != null) scoreManager.ResetScore();
        if (spawnManager != null) spawnManager.RefreshAllBlocks();
        
        OnGameStateChanged?.Invoke(false);
        Debug.Log("Game Started!");
    }

    /// <summary>
    /// Checks if any moves are possible with the currently spawned blocks.
    /// If no moves are possible, triggers Game Over.
    /// </summary>
    public void CheckGameStatus()
    {
        if (IsGameOver) return;

        // Wait a frame before checking to let physics/logic settle? No, Logic is immediate.
        // However, we should check AFTER lines are cleared.
        // This method should be called by GridManager/SpawnManager after updates.
        
        StartCoroutine(CheckGameOverRoutine());
    }

    private IEnumerator CheckGameOverRoutine()
    {
        // Wait for end of frame to ensure all updates (line clear, spawn) are processed
        yield return new WaitForEndOfFrame();

        if (spawnManager == null || gridManager == null || placementValidator == null)
        {
            Debug.LogError("Missing references in GameManager!");
            yield break;
        }

        // Check if any block can be placed
        bool movePossible = false;
        List<BlockInstance> blocks = spawnManager.SpawnedBlocks;

        if (blocks.Count == 0)
        {
            // If no blocks, it means they are being respawned. 
            // SpawnManager handles respawn immediately after last block is used.
            // So if count is 0 here, it's weird, but technically not game over.
            // Wait, SpawnManager logic might respawn in same frame.
            movePossible = true; 
        }
        else
        {
            foreach (BlockInstance block in blocks)
            {
                if (block == null) continue;

                // Check every possible position on the grid
                if (CanBlockFitAnywhere(block.BlockData))
                {
                    movePossible = true;
                    break;
                }
            }
        }

        if (!movePossible)
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Checks if a specific block can fit anywhere on the current grid.
    /// Optimization: Returns true immediately on first valid position.
    /// </summary>
    private bool CanBlockFitAnywhere(BlockData blockData)
    {
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                if (placementValidator.CanPlaceBlock(blockData, x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void TriggerGameOver()
    {
        IsGameOver = true;
        OnGameStateChanged?.Invoke(true);
        Debug.Log("GAME OVER! No more moves possible.");
        // TODO: Show UI
    }

    public void RestartGame()
    {
        StartGame();
    }
}
