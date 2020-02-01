using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectorBehavior : MonoBehaviour
{
    public Canvas portraitCanvas;
    public Canvas landscapeCanvas;

    // Items pertaining to the Sky Series levels
    private LevelData levelData;
    public Sprite skySeriesUnlockedLevelSprite;
    public Sprite skySeriesLockedLevelSprite;

    // Landscape canvas items
    public Button landscapeReturnToMenuButton;
    public GameObject landscapeHowtoPopup;
    public Button landscapeHowtoNextButton1;
    public Button landscapeHowtoNextButton2;
    public Button landscapeHowtoNextButton3;
    public Button landscapeHowtoDismissButton;
    // Requirements for hatching the egg for Sky Series
    public Button ssLandscapeLevel1Button;
    public Button ssLandscapeLevel2Button;
    public Button ssLandscapeLevel3Button;
    public Button ssLandscapeLevel4Button;
    public Button ssLandscapeLevel5Button;
    public Text ssLandscapeLevel1Text;
    public Text ssLandscapeLevel2Text;
    public Text ssLandscapeLevel3Text;
    public Text ssLandscapeLevel4Text;
    public Text ssLandscapeLevel5Text;

    // Portrait canvas items
    public Button portraitReturnToMenuButton;
    public GameObject portraitHowtoPopup;
    public Button portraitHowtoNextButton1;
    public Button portraitHowtoNextButton2;
    public Button portraitHowtoNextButton3;
    public Button portraitHowtoDismissButton;
    // Requirements for hatching the egg for Sky Series
    public Button ssPortraitLevel1Button;
    public Button ssPortraitLevel2Button;
    public Button ssPortraitLevel3Button;
    public Button ssPortraitLevel4Button;
    public Button ssPortraitLevel5Button;
    public Text ssPortraitLevel1Text;
    public Text ssPortraitLevel2Text;
    public Text ssPortraitLevel3Text;
    public Text ssPortraitLevel4Text;
    public Text ssPortraitLevel5Text;

    public GameObject levelChanger;
    private LevelChanger levelChangerScript;


    void Awake()
    {
        levelChangerScript = levelChanger.GetComponent<LevelChanger>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Sky series initialization code
        levelData = LevelData.Instance();

        if (ScreenCommon.GetOptimalDeviceOrientation() == ScreenCommon.PORTRAIT_DEVICE)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            landscapeCanvas.gameObject.SetActive(false);

            portraitReturnToMenuButton.onClick.AddListener(ReturnToMenuButtonClicked);

            // Determine whether or not to show the "how to play" popup
            if (PlayerPrefsCommon.GetHowtoPopup() == 0)
            {
                portraitHowtoPopup.SetActive(true);
                portraitHowtoNextButton1.onClick.AddListener(delegate { NextButtonClicked(portraitHowtoNextButton1); });
                portraitHowtoNextButton2.onClick.AddListener(delegate { NextButtonClicked(portraitHowtoNextButton2); });
                portraitHowtoNextButton3.onClick.AddListener(delegate { NextButtonClicked(portraitHowtoNextButton3); });
                portraitHowtoDismissButton.onClick.AddListener(delegate { DismissPopupButtonClicked(portraitHowtoDismissButton); });
            }
            else
            {
                portraitHowtoPopup.SetActive(false);
            }

            ssPortraitLevel1Button.onClick.AddListener(PlayLevel1);
            if (levelData.levelData.level1Complete)
            {
                ssPortraitLevel2Button.onClick.AddListener(PlayLevel2);
                ssPortraitLevel2Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssPortraitLevel2Text.text = "2";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete)
            {
                ssPortraitLevel3Button.onClick.AddListener(PlayLevel3);
                ssPortraitLevel3Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssPortraitLevel3Text.text = "3";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete)
            {
                ssPortraitLevel4Button.onClick.AddListener(PlayLevel4);
                ssPortraitLevel4Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssPortraitLevel4Text.text = "4";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete && levelData.levelData.level4Complete)
            {
                ssPortraitLevel5Button.onClick.AddListener(PlayLevel5);
                ssPortraitLevel5Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssPortraitLevel5Text.text = "5";
            }
        }
        else
        {
            // Landscape setup
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            portraitCanvas.gameObject.SetActive(false);

            landscapeReturnToMenuButton.onClick.AddListener(ReturnToMenuButtonClicked);

            // Determine whether or not to show the "how to play" popup
            if (PlayerPrefsCommon.GetHowtoPopup() == 0)
            {
                landscapeHowtoPopup.SetActive(true);
                landscapeHowtoNextButton1.onClick.AddListener(delegate { NextButtonClicked(landscapeHowtoNextButton1); });
                landscapeHowtoNextButton2.onClick.AddListener(delegate { NextButtonClicked(landscapeHowtoNextButton2); });
                landscapeHowtoNextButton3.onClick.AddListener(delegate { NextButtonClicked(landscapeHowtoNextButton3); });
                landscapeHowtoDismissButton.onClick.AddListener(delegate { DismissPopupButtonClicked(landscapeHowtoDismissButton); });
            }
            else
            {
                landscapeHowtoPopup.SetActive(false);
            }

            ssLandscapeLevel1Button.onClick.AddListener(PlayLevel1);
            if (levelData.levelData.level1Complete)
            {
                ssLandscapeLevel2Button.onClick.AddListener(PlayLevel2);
                ssLandscapeLevel2Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssLandscapeLevel2Text.text = "2";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete)
            {
                ssLandscapeLevel3Button.onClick.AddListener(PlayLevel3);
                ssLandscapeLevel3Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssLandscapeLevel3Text.text = "3";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete)
            {
                ssLandscapeLevel4Button.onClick.AddListener(PlayLevel4);
                ssLandscapeLevel4Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssLandscapeLevel4Text.text = "4";
            }
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete && levelData.levelData.level4Complete)
            {
                ssLandscapeLevel5Button.onClick.AddListener(PlayLevel5);
                ssLandscapeLevel5Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssLandscapeLevel5Text.text = "5";
            }
        }

    }

    private void ReturnToMenuButtonClicked()
    {
        levelChangerScript.FadeToPreviousScene();
    }

    private void DismissPopupButtonClicked(Button button)
    {
        PlayerPrefsCommon.SetHowtoPopup(1);
        Transform parent = button.transform.parent;
        parent.gameObject.SetActive(false);
    }

    private void NextButtonClicked(Button button)
    {
        Transform parent = button.transform.parent;
        parent.gameObject.SetActive(false);
    }

    public void PlayLevel1()
    {
        LevelManager.SetLevelNumber(1);
        levelChangerScript.FadeToScene("Level1Scene");
    }

    public void PlayLevel2()
    {
        LevelManager.SetLevelNumber(2);
        levelChangerScript.FadeToScene("Level1Scene");
    }

    public void PlayLevel3()
    {
        LevelManager.SetLevelNumber(3);
        levelChangerScript.FadeToScene("Level1Scene");
    }

    public void PlayLevel4()
    {
        LevelManager.SetLevelNumber(4);
        levelChangerScript.FadeToScene("Level1Scene");
    }

    public void PlayLevel5()
    {
        LevelManager.SetLevelNumber(5);
        levelChangerScript.FadeToScene("Level1Scene");
    }
}
