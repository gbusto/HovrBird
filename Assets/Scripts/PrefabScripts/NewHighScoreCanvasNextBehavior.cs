using UnityEngine;
using UnityEngine.UI;

public class NewHighScoreCanvasNextBehavior : MonoBehaviour
{
    public Image newHighScoreBackgroundImage;
    public Text highScoreWordsText;
    public Text highScoreNumberText;
    public Button retryButton;
    public Button nextLevelButton;

    // Update is called once per frame
    void Update()
    {
        newHighScoreBackgroundImage.gameObject.transform.Rotate(Vector3.forward * -15 * Time.deltaTime);
    }

    public void SetHighScoreNumber(int highScore)
    {
        highScoreNumberText.text = highScore.ToString();
    }
}
