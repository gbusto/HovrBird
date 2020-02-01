using UnityEngine;
using UnityEngine.UI;

public class StartMenuBehavior_old : MonoBehaviour
{
    public Button playButton;

    public GameObject commonObject;
    private CommonBehavior commonScript;

    // Start is called before the first frame update
    void Start()
    {
        playButton.onClick.AddListener(PlayButtonClicked);
        commonScript = commonObject.GetComponent<CommonBehavior>();
    }

    void PlayButtonClicked()
    {
        gameObject.SetActive(false);
        commonScript.StartGame(impulse:true);
    }
}
