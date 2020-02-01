using UnityEngine;
using UnityEngine.UI;

public class GameOverRescueMenuBehavior : MonoBehaviour
{
    public Button gameOverRescueContinueButton;
    public Button gameOverRescueNoThanksButton;

    public GameObject commonObject;
    private CommonBehavior commonScript;
        
    // Start is called before the first frame update
    void Awake()
    {
        gameOverRescueContinueButton.onClick.AddListener(GameOverRescueContinueButtonClicked);
        gameOverRescueNoThanksButton.onClick.AddListener(GameOverRescueNoThanksButtonClicked);

        commonScript = commonObject.GetComponent<CommonBehavior>();
    }

    void GameOverRescueContinueButtonClicked()
    {
        commonScript.ShowRewardAd();
    }

    void GameOverRescueNoThanksButtonClicked()
    {
        commonScript.RescueDontContinueGame();
    }
}
