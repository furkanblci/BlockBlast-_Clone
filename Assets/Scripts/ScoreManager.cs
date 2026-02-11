using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the scoring system, including combos and high scores.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Scoring Rules")]
    [SerializeField] private int pointsPerBlockCell = 10;
    [SerializeField] private int pointsPerLineClear = 100;
    [SerializeField] private float comboMultiplier = 1.5f; // Multiplier per combo step

    [Header("State")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private int currentCombo = 0;

    // Events for UI updates (could use C# events or UnityEvents)
    public delegate void ScoreChangedHelper(int newScore);
    public event ScoreChangedHelper OnScoreChanged;
    
    public delegate void ComboChangedHelper(int newCombo);
    public event ComboChangedHelper OnComboChanged;

    public delegate void HighScoreChangedHelper(int newHighScore);
    public event HighScoreChangedHelper OnHighScoreChanged;

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

        LoadHighScore();
    }

    /// <summary>
    /// Adds points for placing a block.
    /// </summary>
    public void AddPlacementScore(BlockData block)
    {
        if (block == null) return;

        int points = block.Shape.Count * pointsPerBlockCell;
        AddScore(points);
        Debug.Log($"Placement Score: {points}");
    }

    /// <summary>
    /// Calculates and adds points for clearing lines.
    /// Handles combo logic.
    /// </summary>
    public void ProcessLineClears(int linesCleared)
    {
        if (linesCleared > 0)
        {
            // Increase combo
            currentCombo++;
            
            // Calculate base score for lines (e.g. 100 * lines)
            int baseLineScore = linesCleared * pointsPerLineClear;
            
            // Apply combo multiplier
            // Formula: Base * (1 + (Combo-1) * 0.5) or similar
            // Let's use simpler: Base * Combo
            int totalScore = baseLineScore * currentCombo;
            
            AddScore(totalScore);
            
            Debug.Log($"Line Clear: {linesCleared} lines. Combo: {currentCombo}. Points: {totalScore}");
            
            OnComboChanged?.Invoke(currentCombo);
        }
        else
        {
            // Reset combo if no lines were cleared this turn
            if (currentCombo > 0)
            {
                currentCombo = 0;
                OnComboChanged?.Invoke(currentCombo);
                Debug.Log("Combo Reset!");
            }
        }
    }

    private void AddScore(int amount)
    {
        currentScore += amount;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
        
        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetScore() => currentScore;
    public int GetHighScore() => highScore;
    public int GetCombo() => currentCombo;

    public void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        // Notify listeners of initial high score
        OnHighScoreChanged?.Invoke(highScore);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
}
