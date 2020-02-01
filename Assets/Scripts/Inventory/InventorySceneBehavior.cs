using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventorySceneBehavior : MonoBehaviour
{
    public Canvas portraitCanvas;
    public Canvas landscapeCanvas;
    public GameObject levelChanger;
    private LevelChanger levelChangerScript;

    // Landscape canvas items
    public Button landscapeReturnToMenuButton;
    public Button landscapeItemsTabButton;
    public Button landscapeBirdsTabButton;

    public ScrollRect landscapeItemsScrollView;
    public ScrollRect landscapeBirdScrollView;

    public Text landscapeCoinCountText;
    public Text landscapeBananaCountText;
    public Text landscapeBlueberriesCountText;
    public Text landscapeStrawberryCountText;

    public Button landscapeBird1Button; // Koko the bird button
    public Button landscapeBird2Button; // Sam the toucan button

    public Image landscapeBird1Panel;
    public Image landscapeBird2Panel;

    public Text landscapeBird2NameText;
    public Text landscapeBird2DescriptionText;

    public Canvas landscapeBirdPopupCanvas;
    public Image landscapeRequirement1Image;
    public Image landscapeRequirement2Image;
    public Image landscapeRequirement3Image;
    public Image landscapeRequirement4Image;
    public Text landscapeRequirement1Text;
    public Text landscapeRequirement2Text;
    public Text landscapeRequirement3Text;
    public Text landscapeRequirement4Text;
    public Button landscapeHatchButton;
    public Button landscapeDismissButton;
    public Image landscapeCheckmark1;
    public Image landscapeCheckmark2;
    public Image landscapeCheckmark3;
    public Image landscapeCheckmark4;




    // Portrait canvas items
    public Button portraitReturnToMenuButton;
    public Button portraitItemsTabButton;
    public Button portraitBirdsTabButton;

    public ScrollRect portraitItemsScrollView;
    public ScrollRect portraitBirdScrollView;

    public Text portraitCoinCountText;
    public Text portraitBananaCountText;
    public Text portraitBlueberriesCountText;
    public Text portraitStrawberryCountText;

    public Button portraitBird1Button; // Koko the bird button
    public Button portraitBird2Button; // Sam the toucan button

    public Image portraitBird1Panel;
    public Image portraitBird2Panel;

    public Text portraitBird2NameText;
    public Text portraitBird2DescriptionText;

    public Canvas portraitBirdPopupCanvas;
    public Image portraitRequirement1Image;
    public Image portraitRequirement2Image;
    public Image portraitRequirement3Image;
    public Image portraitRequirement4Image;
    public Text portraitRequirement1Text;
    public Text portraitRequirement2Text;
    public Text portraitRequirement3Text;
    public Text portraitRequirement4Text;
    public Button portraitHatchButton;
    public Button portraitDismissButton;
    public Image portraitCheckmark1;
    public Image portraitCheckmark2;
    public Image portraitCheckmark3;
    public Image portraitCheckmark4;

    public Sprite checkmarkSprite;
    public Sprite kokoSprite;
    public Sprite samSprite;
    public Sprite lockedEggSprite;
    public Sprite unlockedEggSprite;
    public Sprite coinSprite;
    public Sprite bananaSprite;
    public Sprite blueberrySprite;
    public Sprite strawberrySprite;

    private Canvas birdPopupCanvas;
    private Image requirement1Image;
    private Image requirement2Image;
    private Image requirement3Image;
    private Image requirement4Image;
    private Text requirement1Text;
    private Text requirement2Text;
    private Text requirement3Text;
    private Text requirement4Text;
    private Image checkmark1Image;
    private Image checkmark2Image;
    private Image checkmark3Image;
    private Image checkmark4Image;
    private Button dismissButton;
    private Button hatchButton;

    private Text coinCountText;
    private Text bananaCountText;
    private Text blueberriesCountText;
    private Text strawberryCountText;

    public struct BirdRow
    {
        public uint birdId;
        public Button birdButton;
        public Text birdNameText;
        public Text birdDescriptionText;
        public string birdName;
        public string birdDescription;
        public Image birdPanel;
    }

    private BirdRow activeBirdRow;

    private InventoryData inventoryData;

    private static Dictionary<string, int> samRequirements = new Dictionary<string, int>
    {
        [InventoryData.coinKey] = 150,
        [InventoryData.bananaKey] = 50,
        [InventoryData.blueberryKey] = 20,
        [InventoryData.strawberryKey] = 1
    };
    private const string samName = "Sam";
    private const string samDescripion = "This toucan loves his fruit!";


    void Awake()
    {
        levelChangerScript = levelChanger.GetComponent<LevelChanger>();
    }

    void Start()
    {
        inventoryData = InventoryData.Instance();
        Dictionary<uint, bool> birdDict = inventoryData.birdDict;

        if (ScreenCommon.GetOptimalDeviceOrientation() == ScreenCommon.PORTRAIT_DEVICE)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            landscapeCanvas.gameObject.SetActive(false);

            portraitReturnToMenuButton.onClick.AddListener(ReturnToMenuButtonClicked);

            coinCountText = portraitCoinCountText;
            bananaCountText = portraitBananaCountText;
            blueberriesCountText = portraitBlueberriesCountText;
            strawberryCountText = portraitStrawberryCountText;

            portraitBirdsTabButton.onClick.AddListener(delegate { BirdsTabButtonClicked(portraitItemsScrollView, portraitBirdScrollView); });
            portraitItemsTabButton.onClick.AddListener(delegate { ItemsTabButtonClicked(portraitItemsScrollView, portraitBirdScrollView); });
            portraitItemsTabButton.Select();
            ItemsTabButtonClicked(portraitItemsScrollView, portraitBirdScrollView);

            portraitBird1Panel.gameObject.SetActive(false);
            portraitBird2Panel.gameObject.SetActive(false);

            BirdRow[] birdRows =
            {
                new BirdRow
                {
                    birdId = InventoryData.KOKO_ID,
                    birdButton = portraitBird1Button,
                    birdPanel = portraitBird1Panel
                },
                new BirdRow
                {
                    birdId = InventoryData.SAM_ID,
                    birdButton = portraitBird2Button,
                    birdNameText = portraitBird2NameText,
                    birdDescriptionText = portraitBird2DescriptionText,
                    birdName = samName,
                    birdDescription = samDescripion,
                    birdPanel = portraitBird2Panel
                }
            };

            uint activeBirdId = PlayerPrefsCommon.GetActiveBirdId();

            int counter = 0;
            foreach (KeyValuePair<uint, bool> bird in birdDict)
            {
                BirdRow row = birdRows[counter];

                if (row.birdId == activeBirdId)
                {
                    SwitchActiveBird(row);
                }

                if (bird.Key == InventoryData.KOKO_ID)
                {
                    row.birdButton.onClick.AddListener(delegate { BirdButtonCliked(row); });
                    counter++;
                    continue;
                }

                Sprite sprite = GetSpriteForId(bird.Key, bird.Value);
                if (null == sprite)
                {
                    counter++;
                    continue;
                }

                if (sprite == lockedEggSprite)
                {
                    row.birdButton.onClick.AddListener(delegate { LockedEggButtonClicked(row); });
                }
                else if (sprite == unlockedEggSprite)
                {
                    row.birdButton.onClick.AddListener(delegate { UnlockedEggButtonClicked(row); });
                }
                else if (sprite != null)
                {
                    row.birdNameText.text = row.birdName;
                    row.birdDescriptionText.text = row.birdDescription;
                    row.birdButton.onClick.AddListener(delegate { BirdButtonCliked(row); });
                }
                row.birdButton.GetComponent<Image>().sprite = sprite;
                counter++;
            }

            birdPopupCanvas = portraitBirdPopupCanvas;
            requirement1Image = portraitRequirement1Image;
            requirement2Image = portraitRequirement2Image;
            requirement3Image = portraitRequirement3Image;
            requirement4Image = portraitRequirement4Image;
            requirement1Text = portraitRequirement1Text;
            requirement2Text = portraitRequirement2Text;
            requirement3Text = portraitRequirement3Text;
            requirement4Text = portraitRequirement4Text;
            checkmark1Image = portraitCheckmark1;
            checkmark2Image = portraitCheckmark2;
            checkmark3Image = portraitCheckmark3;
            checkmark4Image = portraitCheckmark4;
            hatchButton = portraitHatchButton;
            dismissButton = portraitDismissButton;
        }
        else
        {
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            portraitCanvas.gameObject.SetActive(false);

            landscapeReturnToMenuButton.onClick.AddListener(ReturnToMenuButtonClicked);

            coinCountText = landscapeCoinCountText;
            bananaCountText = landscapeBananaCountText;
            blueberriesCountText = landscapeBlueberriesCountText;
            strawberryCountText = landscapeStrawberryCountText;

            landscapeBirdsTabButton.onClick.AddListener(delegate { BirdsTabButtonClicked(landscapeItemsScrollView, landscapeBirdScrollView); });
            landscapeItemsTabButton.onClick.AddListener(delegate { ItemsTabButtonClicked(landscapeItemsScrollView, landscapeBirdScrollView); });
            landscapeItemsTabButton.Select();
            ItemsTabButtonClicked(landscapeItemsScrollView, landscapeBirdScrollView);

            landscapeBird1Panel.gameObject.SetActive(false);
            landscapeBird2Panel.gameObject.SetActive(false);

            BirdRow[] birdRows =
            {
                new BirdRow
                {
                    birdId = InventoryData.KOKO_ID,
                    birdButton = landscapeBird1Button,
                    birdPanel = landscapeBird1Panel
                },
                new BirdRow
                {
                    birdId = InventoryData.SAM_ID,
                    birdButton = landscapeBird2Button,
                    birdNameText = landscapeBird2NameText,
                    birdDescriptionText = landscapeBird2DescriptionText,
                    birdName = samName,
                    birdDescription = samDescripion,
                    birdPanel = landscapeBird2Panel
                }
            };

            uint activeBirdId = PlayerPrefsCommon.GetActiveBirdId();

            int counter = 0;
            foreach (KeyValuePair<uint, bool> bird in birdDict)
            {
                BirdRow row = birdRows[counter];

                if (row.birdId == activeBirdId)
                {
                    SwitchActiveBird(row);
                }

                if (bird.Key == InventoryData.KOKO_ID)
                {
                    row.birdButton.onClick.AddListener(delegate { BirdButtonCliked(row); });
                    counter++;
                    continue;
                }

                Sprite sprite = GetSpriteForId(bird.Key, bird.Value);
                if (null == sprite)
                {
                    counter++;
                    continue;
                }

                if (sprite == lockedEggSprite)
                {
                    row.birdButton.onClick.AddListener(delegate { LockedEggButtonClicked(row); });
                }
                else if (sprite == unlockedEggSprite)
                {
                    row.birdButton.onClick.AddListener(delegate { UnlockedEggButtonClicked(row); });
                }
                else if (sprite != null)
                {
                    row.birdNameText.text = row.birdName;
                    row.birdDescriptionText.text = row.birdDescription;
                    row.birdButton.onClick.AddListener(delegate { BirdButtonCliked(row); });
                }
                row.birdButton.GetComponent<Image>().sprite = sprite;
                counter++;
            }

            birdPopupCanvas = landscapeBirdPopupCanvas;
            requirement1Image = landscapeRequirement1Image;
            requirement2Image = landscapeRequirement2Image;
            requirement3Image = landscapeRequirement3Image;
            requirement4Image = landscapeRequirement4Image;
            requirement1Text = landscapeRequirement1Text;
            requirement2Text = landscapeRequirement2Text;
            requirement3Text = landscapeRequirement3Text;
            requirement4Text = landscapeRequirement4Text;
            checkmark1Image = landscapeCheckmark1;
            checkmark2Image = landscapeCheckmark2;
            checkmark3Image = landscapeCheckmark3;
            checkmark4Image = landscapeCheckmark4;
            hatchButton = landscapeHatchButton;
            dismissButton = landscapeDismissButton;
        }

        birdPopupCanvas.gameObject.SetActive(false);
        dismissButton.onClick.AddListener(PopupDismissButtoncClicked);
    }

    public void ReturnToMenuButtonClicked()
    {
        levelChangerScript.FadeToPreviousScene();
    }

    public void ItemsTabButtonClicked(ScrollRect itemsScroll, ScrollRect birdScroll)
    {
        UpdateItemCounts();

        itemsScroll.gameObject.SetActive(true);
        birdScroll.gameObject.SetActive(false);
    }

    public void BirdsTabButtonClicked(ScrollRect itemsScroll, ScrollRect birdScroll)
    {
        birdScroll.gameObject.SetActive(true);
        itemsScroll.gameObject.SetActive(false);
    }

    public void LockedEggButtonClicked(BirdRow bird)
    {
        Dictionary<string, int> reqs = GetRequirementForId(bird.birdId);
        if (null == reqs)
        {
            return;
        }

        string coin = InventoryData.coinKey;
        string banana = InventoryData.bananaKey;
        string blueberry = InventoryData.blueberryKey;
        string strawberry = InventoryData.strawberryKey;
        int currentCoinCount = inventoryData.GetItemCount(coin);
        int currentBananaCount = inventoryData.GetItemCount(banana);
        int currentBlueberryCount = inventoryData.GetItemCount(blueberry);
        int currentStrawberryCount = inventoryData.GetItemCount(strawberry);
        int requiredCoinCount = reqs[coin];
        int requiredBananaCount = reqs[banana];
        int requiredBlueberryCount = reqs[blueberry];
        int requiredStrawberryCount = reqs[strawberry];

        requirement1Image.sprite = coinSprite;
        requirement2Image.sprite = bananaSprite;
        requirement3Image.sprite = blueberrySprite;
        requirement4Image.sprite = strawberrySprite;
        requirement1Text.text = currentCoinCount + "/" + requiredCoinCount;
        requirement2Text.text = currentBananaCount + "/" + requiredBananaCount;
        requirement3Text.text = currentBlueberryCount + "/" + requiredBlueberryCount;
        requirement4Text.text = currentStrawberryCount + "/" + requiredStrawberryCount;

        if (currentCoinCount >= requiredCoinCount)
        {
            checkmark1Image.sprite = checkmarkSprite;
        }
        else
        {
            checkmark1Image.gameObject.SetActive(false);
        }

        if (currentBananaCount >= requiredBananaCount)
        {
            checkmark2Image.sprite = checkmarkSprite;
        }
        else
        {
            checkmark2Image.gameObject.SetActive(false);
        }

        if (currentBlueberryCount >= requiredBlueberryCount)
        {
            checkmark3Image.sprite = checkmarkSprite;
        }
        else
        {
            checkmark3Image.gameObject.SetActive(false);
        }

        if (currentStrawberryCount >= requiredStrawberryCount)
        {
            checkmark4Image.sprite = checkmarkSprite;
        }
        else
        {
            checkmark4Image.gameObject.SetActive(false);
        }

        hatchButton.enabled = false;
        Color c = hatchButton.image.color;
        c.a = 0.1f;
        hatchButton.image.color = c;

        birdPopupCanvas.gameObject.SetActive(true);
    }

    public void UnlockedEggButtonClicked(BirdRow bird)
    {
        Color c = hatchButton.image.color;
        LockedEggButtonClicked(bird);
        hatchButton.image.color = c;
        hatchButton.enabled = true;

        hatchButton.onClick.AddListener(delegate { PopupHatchButtonClicked(bird); });
    }

    public void PopupDismissButtoncClicked()
    {
        birdPopupCanvas.gameObject.SetActive(false);
    }

    public void PopupHatchButtonClicked(BirdRow bird)
    {
        // Subtract the cost of each item from the user's inventory
        Dictionary<string, int> requirements = GetRequirementForId(bird.birdId);
        foreach (KeyValuePair<string, int> req in requirements)
        {
            inventoryData.collectibleDict[req.Key] -= req.Value;
        }

        // Cool animation stuff should happen here

        inventoryData.HatchEggWithId(bird.birdId);
        Sprite sprite = GetSpriteForId(bird.birdId, true);
        bird.birdButton.image.sprite = sprite;

        inventoryData.SaveUserData();

        bird.birdButton.onClick.RemoveAllListeners();
        bird.birdButton.onClick.AddListener(delegate { BirdButtonCliked(bird); });

        bird.birdNameText.text = bird.birdName;
        bird.birdDescriptionText.text = bird.birdDescription;

        PopupDismissButtoncClicked();
    }

    public void BirdButtonCliked(BirdRow bird)
    {
        SwitchActiveBird(bird);
    }

    private Sprite GetSpriteForId(uint id, bool hatched)
    {
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return kokoSprite;

            case InventoryData.SAM_ID:
                if (hatched)
                {
                    return samSprite;
                }

                if (RequirementsMet(samRequirements))
                {
                    return unlockedEggSprite;
                }

                return lockedEggSprite;
        }

        return null;
    }

    private Dictionary<string, int> GetRequirementForId(uint id)
    {
        switch (id)
        {
            case InventoryData.SAM_ID:
                return samRequirements;
        }

        return null;
    }

    private bool RequirementsMet(Dictionary<string, int> requirementsDict)
    {
        foreach (KeyValuePair<string, int> req in requirementsDict)
        {
            int amount = inventoryData.GetItemCount(req.Key);
            if (amount < req.Value)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateItemCounts()
    {
        coinCountText.text = "x" + inventoryData.GetItemCount(InventoryData.coinKey).ToString();
        bananaCountText.text = "x" + inventoryData.GetItemCount(InventoryData.bananaKey).ToString();
        blueberriesCountText.text = "x" + inventoryData.GetItemCount(InventoryData.blueberryKey).ToString();
        strawberryCountText.text = "x" + inventoryData.GetItemCount(InventoryData.strawberryKey).ToString();
    }

    private void SwitchActiveBird(BirdRow bird)
    {
        if (null == activeBirdRow.birdPanel)
        {
            activeBirdRow = bird;
        }
        else
        {
            activeBirdRow.birdPanel.gameObject.SetActive(false);

            activeBirdRow = bird;
        }

        activeBirdRow.birdPanel.gameObject.SetActive(true);
        PlayerPrefsCommon.SetActiveBirdId(bird.birdId);
    }
}
