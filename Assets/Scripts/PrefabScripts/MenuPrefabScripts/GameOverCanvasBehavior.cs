using UnityEngine;
using UnityEngine.UI;

public class GameOverCanvasBehavior : MonoBehaviour
{
    public Image gameOverBackgroundImage;
    public Text highScoreText;
    public Text scoreText;
    public Button retryButton;

    public void SetHighScoreText(int highScore)
    {
        highScoreText.text = "High Score: " + highScore.ToString();
    }

    public void SetScoreText(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
