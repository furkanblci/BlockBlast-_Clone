using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Game Over UI panel.
/// Listens to GameManager events to show the panel when the game ends.
/// </summary>
public class GameOverUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        // Hide panel initially
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Subscribe to events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        // Setup button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }
    }

    private void HandleGameStateChanged(bool isGameOver)
    {
        if (isGameOver)
        {
            ShowGameOver();
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Show final score if available
            if (finalScoreText != null && ScoreManager.Instance != null)
            {
                finalScoreText.text = "Final Score: " + ScoreManager.Instance.GetScore();
            }
        }
    }

    private void OnRestartClicked()
    {
        // Option 1: Reload scene (Simplest for full reset)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Option 2: Use GameManager restart (Better if we want to keep some state)
        // For now, let's use GameManager to keep it consistent
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            
            // Hide panel immediately
            if (gameOverPanel != null) 
                gameOverPanel.SetActive(false);
        }
    }
}
