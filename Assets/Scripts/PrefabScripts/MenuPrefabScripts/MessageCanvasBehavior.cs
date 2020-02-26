using UnityEngine;
using UnityEngine.UI;

public class MessageCanvasBehavior : MonoBehaviour
{
    public Text messageText;
    public Image messageBackground;

    private const int MAX_MESSAGE_LENGTH = 45;

    private float timeCounter;
    private bool fading;
    private FadeState state;
    private enum FadeState
    {
        fadeIn = 1,
        stay,
        fadeOut
    };

    private const float MIN_ALPHA = 0.0f;
    private const float MAX_ALPHA = 1.0f;
    public const float DISPLAY_TIME_SHORT = 1.0f;
    public const float DISPLAY_TIME_MED = 2.0f;
    public const float DISPLAY_TIME_LONG = 4.0f;

    private float displayTime;

    void Update()
    {
        if (fading)
        {
            switch (state)
            {
                case FadeState.fadeIn:
                    if (messageBackground.color.a >= MAX_ALPHA)
                    {
                        state = FadeState.stay;
                    }
                    break;

                case FadeState.stay:
                    timeCounter += Time.deltaTime;
                    if (timeCounter > displayTime)
                    {
                        state = FadeState.fadeOut;
                        timeCounter = 0;
                    }
                    break;

                case FadeState.fadeOut:
                    FadeOut();
                    fading = false;
                    gameObject.SetActive(false);
                    break;
            }
        }
    }

    public bool ShowMessage(string message, float time = DISPLAY_TIME_MED)
    {
        messageText.text = message;
        fading = true;

        state = FadeState.fadeIn;
        FadeIn();

        // Set this variable here as it is user controlled
        displayTime = time;

        gameObject.SetActive(true);

        return message.Length <= MAX_MESSAGE_LENGTH;
    }

    private void FadeIn()
    {
        messageBackground.CrossFadeAlpha(MAX_ALPHA, DISPLAY_TIME_SHORT, false);
        messageText.CrossFadeAlpha(MAX_ALPHA, DISPLAY_TIME_SHORT, false);
    }

    private void FadeOut()
    {
        messageBackground.CrossFadeAlpha(MIN_ALPHA, DISPLAY_TIME_SHORT, false);
        messageText.CrossFadeAlpha(MIN_ALPHA, DISPLAY_TIME_SHORT, false);
    }
}
