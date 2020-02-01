using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverMenuBehavior : MonoBehaviour
{
    public Button gameOverRetryButton;
    public Button exitButton;

    public int highScore;
    public int score;

    public Text gameOverHighScoreText;
    public Text gameOverScoreText;

    public GameObject commonObject;
    private CommonBehavior commonScript;

    // Start is called before the first frame update
    void Awake()
    {
        gameOverRetryButton.onClick.AddListener(GameOverRetryButtonClicked);
        exitButton.onClick.AddListener(ExitButtonClicked);

        gameOverHighScoreText.text = "High Score: " + highScore.ToString();
        gameOverScoreText.text = "Score: " + score.ToString();

        commonScript = commonObject.GetComponent<CommonBehavior>();
    }

    void GameOverRetryButtonClicked()
    {
        commonScript.RestartGame();
    }

    void ExitButtonClicked()
    {
        commonScript.FadeToPreviousScene();
    }
}
