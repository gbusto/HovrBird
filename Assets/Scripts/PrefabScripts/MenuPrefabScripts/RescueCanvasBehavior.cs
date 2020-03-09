using UnityEngine;
using UnityEngine.UI;

public class RescueCanvasBehavior : MonoBehaviour
{
    public Image rescueBackgroundImage;
    public Text messageText;
    public Text rescueHeartText;
    public Button yesButton;
    public Button dismissButton;

    public Image rescueWarningImage;

    public void SetMessageText(string text)
    {
        messageText.text = text;
    }
}
