using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectorBehavior : MonoBehaviour
{
    public Canvas portraitCanvas;
    public Canvas landscapeCanvas;

    public GameObject inventoryCanvasPrefab;
    private GameObject inventoryCanvas;
    private InventoryCanvasBehavior inventoryCanvasScript;

    // Items pertaining to the Sky Series levels
    private LevelData levelData;
    public Sprite skySeriesUnlockedLevelSprite;
    public Sprite skySeriesLockedLevelSprite;

    public Sprite waterSeriesUnlockedLevelSprite;
    public Sprite waterSeriesLockedLevelSprite;

    public Sprite beachSeriesUnlockedLevelSprite;
    public Sprite beachSeriesLockedLevelSprite;

    // Landscape canvas items
    public Button landscapeReturnToMenuButton;
    public Button landscapeInventoryButton;
    public Button landscapeStoreButton;
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

    public Button wsLandscapeLevel6Button;
    public Button wsLandscapeLevel7Button;
    public Button wsLandscapeLevel8Button;
    public Button wsLandscapeLevel9Button;
    public Button wsLandscapeLevel10Button;

    public Button bsLandscapeLevel11Button;
    public Button bsLandscapeLevel12Button;
    public Button bsLandscapeLevel13Button;
    public Button bsLandscapeLevel14Button;
    public Button bsLandscapeLevel15Button;

    public Text ssLandscapeLevel1Text;
    public Text ssLandscapeLevel2Text;
    public Text ssLandscapeLevel3Text;
    public Text ssLandscapeLevel4Text;
    public Text ssLandscapeLevel5Text;

    public Text wsLandscapeLevel6Text;
    public Text wsLandscapeLevel7Text;
    public Text wsLandscapeLevel8Text;
    public Text wsLandscapeLevel9Text;
    public Text wsLandscapeLevel10Text;

    public Text bsLandscapeLevel11Text;
    public Text bsLandscapeLevel12Text;
    public Text bsLandscapeLevel13Text;
    public Text bsLandscapeLevel14Text;
    public Text bsLandscapeLevel15Text;


    // Portrait canvas items
    public Button portraitReturnToMenuButton;
    public Button portraitInventoryButton;
    public Button portraitStoreButton;
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

    public Button wsPortraitLevel6Button;
    public Button wsPortraitLevel7Button;
    public Button wsPortraitLevel8Button;
    public Button wsPortraitLevel9Button;
    public Button wsPortraitLevel10Button;

    public Button bsPortraitLevel11Button;
    public Button bsPortraitLevel12Button;
    public Button bsPortraitLevel13Button;
    public Button bsPortraitLevel14Button;
    public Button bsPortraitLevel15Button;

    public Text ssPortraitLevel1Text;
    public Text ssPortraitLevel2Text;
    public Text ssPortraitLevel3Text;
    public Text ssPortraitLevel4Text;
    public Text ssPortraitLevel5Text;

    public Text wsPortraitLevel6Text;
    public Text wsPortraitLevel7Text;
    public Text wsPortraitLevel8Text;
    public Text wsPortraitLevel9Text;
    public Text wsPortraitLevel10Text;

    public Text bsPortraitLevel11Text;
    public Text bsPortraitLevel12Text;
    public Text bsPortraitLevel13Text;
    public Text bsPortraitLevel14Text;
    public Text bsPortraitLevel15Text;


    public GameObject levelChanger;
    private LevelChanger levelChangerScript;


    void Awake()
    {
        levelChangerScript = levelChanger.GetComponent<LevelChanger>();

        inventoryCanvas = Instantiate(inventoryCanvasPrefab);
        inventoryCanvasScript = inventoryCanvas.GetComponent<InventoryCanvasBehavior>();
        inventoryCanvasScript.inventoryDismissButton.onClick.AddListener(InventoryCanvasDismissed);
        inventoryCanvas.gameObject.SetActive(false);
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
            portraitStoreButton.onClick.AddListener(StoreButtonClicked);
            portraitInventoryButton.onClick.AddListener(InventoryButtonClicked);

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

            /*
             * Sky Series Level Start
             */
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

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete)
            {
                ssPortraitLevel5Button.onClick.AddListener(PlayLevel5);
                ssPortraitLevel5Button.GetComponent<Image>().sprite = skySeriesUnlockedLevelSprite;
                ssPortraitLevel5Text.text = "5";
            }

            /*
             * Water Series Level Start
             */
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete)
            {
                wsPortraitLevel6Button.onClick.AddListener(PlayLevel6);
                wsPortraitLevel6Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsPortraitLevel6Text.text = "6";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete)
            {
                wsPortraitLevel7Button.onClick.AddListener(PlayLevel7);
                wsPortraitLevel7Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsPortraitLevel7Text.text = "7";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete)
            {
                wsPortraitLevel8Button.onClick.AddListener(PlayLevel8);
                wsPortraitLevel8Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsPortraitLevel8Text.text = "8";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete)
            {
                wsPortraitLevel9Button.onClick.AddListener(PlayLevel9);
                wsPortraitLevel9Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsPortraitLevel9Text.text = "9";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete)
            {
                wsPortraitLevel10Button.onClick.AddListener(PlayLevel10);
                wsPortraitLevel10Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsPortraitLevel10Text.text = "10";
            }

            /*
             * Beach Series Level Start
             */
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete)
            {
                bsPortraitLevel11Button.onClick.AddListener(PlayLevel11);
                bsPortraitLevel11Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsPortraitLevel11Text.text = "11";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete)
            {
                bsPortraitLevel12Button.onClick.AddListener(PlayLevel12);
                bsPortraitLevel12Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsPortraitLevel12Text.text = "12";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete)
            {
                bsPortraitLevel13Button.onClick.AddListener(PlayLevel13);
                bsPortraitLevel13Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsPortraitLevel13Text.text = "13";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete &&
                levelData.levelData.level13Complete)
            {
                bsPortraitLevel14Button.onClick.AddListener(PlayLevel14);
                bsPortraitLevel14Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsPortraitLevel14Text.text = "14";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete &&
                levelData.levelData.level13Complete && levelData.levelData.level14Complete)
            {
                bsPortraitLevel15Button.onClick.AddListener(PlayLevel15);
                bsPortraitLevel15Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsPortraitLevel15Text.text = "15";
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
            landscapeStoreButton.onClick.AddListener(StoreButtonClicked);
            landscapeInventoryButton.onClick.AddListener(InventoryButtonClicked);

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

            /*
             * Sky Series Level Start
             */
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

            /*
             * Water Series Level Start
             */
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete)
            {
                wsLandscapeLevel6Button.onClick.AddListener(PlayLevel6);
                wsLandscapeLevel6Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsLandscapeLevel6Text.text = "6";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete)
            {
                wsLandscapeLevel7Button.onClick.AddListener(PlayLevel7);
                wsLandscapeLevel7Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsLandscapeLevel7Text.text = "7";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete)
            {
                wsLandscapeLevel8Button.onClick.AddListener(PlayLevel8);
                wsLandscapeLevel8Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsLandscapeLevel8Text.text = "8";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete)
            {
                wsLandscapeLevel9Button.onClick.AddListener(PlayLevel9);
                wsLandscapeLevel9Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsLandscapeLevel9Text.text = "9";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete)
            {
                wsLandscapeLevel10Button.onClick.AddListener(PlayLevel10);
                wsLandscapeLevel10Button.GetComponent<Image>().sprite = waterSeriesUnlockedLevelSprite;
                wsLandscapeLevel10Text.text = "10";
            }

            /*
             * Beach Series Level Start
             */
            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete)
            {
                bsLandscapeLevel11Button.onClick.AddListener(PlayLevel11);
                bsLandscapeLevel11Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsLandscapeLevel11Text.text = "11";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete)
            {
                bsLandscapeLevel12Button.onClick.AddListener(PlayLevel12);
                bsLandscapeLevel12Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsLandscapeLevel12Text.text = "12";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete)
            {
                bsLandscapeLevel13Button.onClick.AddListener(PlayLevel13);
                bsLandscapeLevel13Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsLandscapeLevel13Text.text = "13";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete &&
                levelData.levelData.level13Complete)
            {
                bsLandscapeLevel14Button.onClick.AddListener(PlayLevel14);
                bsLandscapeLevel14Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsLandscapeLevel14Text.text = "14";
            }

            if (levelData.levelData.level1Complete && levelData.levelData.level2Complete && levelData.levelData.level3Complete &&
                levelData.levelData.level4Complete && levelData.levelData.level5Complete && levelData.levelData.level6Complete &&
                levelData.levelData.level7Complete && levelData.levelData.level8Complete && levelData.levelData.level9Complete &&
                levelData.levelData.level10Complete && levelData.levelData.level11Complete && levelData.levelData.level12Complete &&
                levelData.levelData.level13Complete && levelData.levelData.level14Complete)
            {
                bsLandscapeLevel15Button.onClick.AddListener(PlayLevel15);
                bsLandscapeLevel15Button.GetComponent<Image>().sprite = beachSeriesUnlockedLevelSprite;
                bsLandscapeLevel15Text.text = "15";
            }
        }
    }

    private void InventoryCanvasDismissed()
    {
        inventoryCanvas.gameObject.SetActive(false);
    }

    private void ReturnToMenuButtonClicked()
    {
        levelChangerScript.FadeToPreviousScene();
    }

    private void StoreButtonClicked()
    {
        levelChangerScript.FadeToScene("StoreScene");
    }

    private void InventoryButtonClicked()
    {
        inventoryCanvas.gameObject.SetActive(true);
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

    public void PlayLevel6()
    {
        LevelManager.SetLevelNumber(6);
        levelChangerScript.FadeToScene("WaterSeriesScene");
    }

    public void PlayLevel7()
    {
        LevelManager.SetLevelNumber(7);
        levelChangerScript.FadeToScene("WaterSeriesScene");
    }

    public void PlayLevel8()
    {
        LevelManager.SetLevelNumber(8);
        levelChangerScript.FadeToScene("WaterSeriesScene");
    }

    public void PlayLevel9()
    {
        LevelManager.SetLevelNumber(9);
        levelChangerScript.FadeToScene("WaterSeriesScene");
    }

    public void PlayLevel10()
    {
        LevelManager.SetLevelNumber(10);
        levelChangerScript.FadeToScene("WaterSeriesScene");
    }

    public void PlayLevel11()
    {
        LevelManager.SetLevelNumber(11);
        levelChangerScript.FadeToScene("BeachSeriesScene");
    }

    public void PlayLevel12()
    {
        LevelManager.SetLevelNumber(12);
        levelChangerScript.FadeToScene("BeachSeriesScene");
    }

    public void PlayLevel13()
    {
        LevelManager.SetLevelNumber(13);
        levelChangerScript.FadeToScene("BeachSeriesScene");
    }

    public void PlayLevel14()
    {
        LevelManager.SetLevelNumber(14);
        levelChangerScript.FadeToScene("BeachSeriesScene");
    }

    public void PlayLevel15()
    {
        LevelManager.SetLevelNumber(15);
        levelChangerScript.FadeToScene("BeachSeriesScene");
    }
}
