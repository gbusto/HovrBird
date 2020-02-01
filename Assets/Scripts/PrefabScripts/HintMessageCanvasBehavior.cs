using UnityEngine;
using UnityEngine.UI;

public class HintMessageCanvasBehavior : MonoBehaviour
{
    public GameObject hintCanvas;
    public Text hintMessageText;
    public Button dismissHintButton;
    public Text dismissHintButtonText;

    public void Start()
    {
        dismissHintButton.onClick.AddListener(DismissHintButtonClicked);
    }

    public void ShowMessage(string message)
    {
        hintMessageText.text = message;
        hintCanvas.gameObject.SetActive(true);
    }

    public void DismissHintButtonClicked()
    {
        hintCanvas.gameObject.SetActive(false);
    }
}
