using UnityEngine;
using TMPro;

/// <summary>
/// Manages the UI display for the score system.
/// Updates TextMeshPro elements based on ScoreManager events.
/// </summary>
public class ScoreUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI paramScoreText;
    [SerializeField] private TextMeshProUGUI paramHighScoreText;
    [SerializeField] private TextMeshProUGUI paramComboText;

    [Header("Combo Settings")]
    [SerializeField] private string comboPrefix = "COMBO x";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color comboColor = Color.yellow;

    private void Start()
    {
        // Subscribe to events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
            ScoreManager.Instance.OnHighScoreChanged += UpdateHighScoreUI;
            ScoreManager.Instance.OnComboChanged += UpdateComboUI;

            // Initialize UI
            UpdateScoreUI(ScoreManager.Instance.GetScore());
            UpdateHighScoreUI(ScoreManager.Instance.GetHighScore());
            UpdateComboUI(ScoreManager.Instance.GetCombo());
        }
        else
        {
            Debug.LogWarning("ScoreManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
            ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScoreUI;
            ScoreManager.Instance.OnComboChanged -= UpdateComboUI;
        }
    }

    private void UpdateScoreUI(int score)
    {
        if (paramScoreText != null)
        {
            paramScoreText.text = score.ToString();
        }
    }

    private void UpdateHighScoreUI(int highScore)
    {
        if (paramHighScoreText != null)
        {
            paramHighScoreText.text = highScore.ToString();
        }
    }

    private void UpdateComboUI(int combo)
    {
        if (paramComboText != null)
        {
            if (combo > 1)
            {
                paramComboText.text = comboPrefix + combo;
                paramComboText.gameObject.SetActive(true);
                // Simple punch effect could be added here
            }
            else
            {
                paramComboText.gameObject.SetActive(false);
            }
        }
    }
}
