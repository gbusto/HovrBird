using UnityEngine;
using UnityEngine.UI;

public class RescueCanvasBehavior : MonoBehaviour
{
    public Image rescueBackgroundImage;
    public Text messageText;
    public Button watchAdButton;
    public Button dismissAdButton;

    public Image rescueWarningImage;

    public void SetMessageText(string text)
    {
        messageText.text = text;
    }
}
