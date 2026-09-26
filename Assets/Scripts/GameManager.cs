using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] GameObject youWinText;
    [SerializeField] GameObject gameOverContainer;
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject restartButton;

    int enemiesLeft = 0;

    const string ENEMIES_LEFT_STRING = "Enemies Left: ";

    public void AdjustEnemiesLeft(int amount)
    {
        enemiesLeft += amount;
        enemiesLeftText.text = ENEMIES_LEFT_STRING + enemiesLeft.ToString();

        if (enemiesLeft <= 0)
        {
            youWinText.SetActive(true);
            ShowQuitButton();
        }
    }

    void ShowQuitButton()
    {
        // Reuse the game over screen, but only keep the Quit button visible.
        if (gameOverContainer == null) return;
        gameOverContainer.SetActive(true);
        if (gameOverText != null) gameOverText.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        StarterAssets.StarterAssetsInputs starterAssetsInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
        if (starterAssetsInputs == null) return;
        starterAssetsInputs.cursorLocked = false;
        starterAssetsInputs.SetCursorState(false);
        starterAssetsInputs.enabled = false;
    }

    public void RestartLevelButton()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    public void QuitButton()
    {
        Debug.LogWarning("Does not work in the Unity Editor!  You silly goose!");
        Application.Quit();
    }
}