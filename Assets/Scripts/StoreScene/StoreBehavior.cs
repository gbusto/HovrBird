using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class StoreBehavior : MonoBehaviour
{
    public IAPManager purchaseManager;

    public GameObject adObject;
    private AdObjectBehavior adObjectScript;

    public Canvas portraitCanvas;
    public Canvas landscapeCanvas;

    public GameObject inventoryCanvas;
    public Button inventoryDismissButton;

    public GameObject levelChanger;
    private LevelChanger levelChangerScript;

    public Button landscapeBackButton;
    public Button landscapeInventoryButton;
    public Button landscapeCoinIAP1Button;
    public Text landscapeCoinIAP1PriceText;
    public Button landscapeCoinIAP2Button;
    public Text landscapeCoinIAP2PriceText;
    public Button landscapeCoinIAP3Button;
    public Text landscapeCoinIAP3PriceText;
    public Button landscapeLivesEarnedButton;
    public Text landscapeLivesEarnedTimerText;
    public Button landscapeLivesIAP1Button;
    public Button landscapeLivesIAP2Button;
    public Button landscapeBirdIAP1Button;
    public Text landscapeBirdIAP1PriceText;
    public Button landscapeBirdIAP2Button;
    public Text landscapeBirdIAP2PriceText;
    public Button landscapeBirdIAP3Button;
    public Text landscapeBirdIAP3PriceText;
    public Button landscapeBirdIAP4Button;
    public Text landscapeBirdIAP4PriceText;

    public Button portraitBackButton;
    public Button portraitInventoryButton;
    public Button portraitCoinIAP1Button;
    public Text portraitCoinIAP1PriceText;
    public Button portraitCoinIAP2Button;
    public Text portraitCoinIAP2PriceText;
    public Button portraitCoinIAP3Button;
    public Text portraitCoinIAP3PriceText;
    public Button portraitLivesEarnedButton;
    public Text portraitLivesEarnedTimerText;
    public Button portraitLivesIAP1Button;
    public Button portraitLivesIAP2Button;
    public Button portraitBirdIAP1Button;
    public Text portraitBirdIAP1PriceText;
    public Button portraitBirdIAP2Button;
    public Text portraitBirdIAP2PriceText;
    public Button portraitBirdIAP3Button;
    public Text portraitBirdIAP3PriceText;
    public Button portraitBirdIAP4Button;
    public Text portraitBirdIAP4PriceText;

    private enum IAP_IDS
    {
        coinIAP1 = 1,
        coinIAP2,
        coinIAP3,
        birdIAP1 = 100,
        birdIAP2,
        birdIAP3,
        birdIAP4,
    };

    private InventoryData inventoryData;

    private static int LIVES_PURCHASED_WITH_COINS = 20;
    private static int PURCHASE_LIVES_AMOUNT = 1000;

    private Text timerText;
    private TimeSpan diffTime;

    // XXX MUST be set to false before release
    private bool TESTING = true;

    /*
     * These functions will be called from AdObjectScript.
     *
     * DO NOT DELETE THEM!
     */
    private void RewardAdFailed(AdErrorEventArgs args)
    {
        print("Failed to load reward ad... " + args.ToString());
    }

    private void RewardAdClosed(RewardStruct reward)
    {
        if (reward.rewardUser)
        {
            print("Rewarding user with " + reward.rewardAmount + " lives!");
            inventoryData.AddLives(reward.rewardAmount);
        }
    }

    private void Awake()
    {
        purchaseManager = new IAPManager();

        inventoryData = InventoryData.Instance();

        levelChangerScript = levelChanger.GetComponent<LevelChanger>();

        AdManager.Initialize(TESTING);

        adObjectScript = adObject.GetComponent<AdObjectBehavior>();
        adObjectScript.InitAdObject(this.gameObject);

        // Need to define functions RewardAdFailed(AdErrorEventArgs args) and
        // RewardAdClosed(RewardStruct reward).
    }

    // Start is called before the first frame update
    void Start()
    {
        inventoryDismissButton.onClick.AddListener(InventoryDismissButtonClicked);

        DateTime dateToClaimLives = inventoryData.GetDateToClaimLives();
        diffTime = dateToClaimLives.Subtract(DateTime.Now);

        if (ScreenCommon.GetOptimalDeviceOrientation() == ScreenCommon.PORTRAIT_DEVICE)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            landscapeCanvas.gameObject.SetActive(false);

            portraitBackButton.onClick.AddListener(ReturnToMenuButtonClicked);
            portraitInventoryButton.onClick.AddListener(InventoryButtonClicked);

            portraitCoinIAP1PriceText.text = GetPriceForItem(IAP_IDS.coinIAP1);
            portraitCoinIAP2PriceText.text = GetPriceForItem(IAP_IDS.coinIAP2);
            portraitCoinIAP3PriceText.text = GetPriceForItem(IAP_IDS.coinIAP3);
            portraitBirdIAP1PriceText.text = GetPriceForItem(IAP_IDS.birdIAP1);
            portraitBirdIAP2PriceText.text = GetPriceForItem(IAP_IDS.birdIAP2);
            portraitBirdIAP3PriceText.text = GetPriceForItem(IAP_IDS.birdIAP3);
            portraitBirdIAP4PriceText.text = GetPriceForItem(IAP_IDS.birdIAP4);

            portraitCoinIAP1Button.onClick.AddListener(CoinIAP1ButtonClicked);
            portraitCoinIAP2Button.onClick.AddListener(CoinIAP2ButtonClicked);
            portraitCoinIAP3Button.onClick.AddListener(CoinIAP3ButtonClicked);
            portraitBirdIAP1Button.onClick.AddListener(BirdIAP1ButtonClicked);
            portraitBirdIAP2Button.onClick.AddListener(BirdIAP2ButtonClicked);
            portraitBirdIAP3Button.onClick.AddListener(BirdIAP3ButtonClicked);
            portraitBirdIAP4Button.onClick.AddListener(BirdIAP4ButtonClicked);

            portraitLivesEarnedButton.onClick.AddListener(EarnedLivesButtonClicked);
            portraitLivesIAP1Button.onClick.AddListener(LivesIAP1ButtonClicked);
            portraitLivesIAP2Button.onClick.AddListener(LivesIAP2ButtonClicked);

            timerText = portraitLivesEarnedTimerText;
        }
        else
        {
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            portraitCanvas.gameObject.SetActive(false);

            landscapeBackButton.onClick.AddListener(ReturnToMenuButtonClicked);
            landscapeInventoryButton.onClick.AddListener(InventoryButtonClicked);

            landscapeCoinIAP1PriceText.text = GetPriceForItem(IAP_IDS.coinIAP1);
            landscapeCoinIAP2PriceText.text = GetPriceForItem(IAP_IDS.coinIAP2);
            landscapeCoinIAP3PriceText.text = GetPriceForItem(IAP_IDS.coinIAP3);
            landscapeBirdIAP1PriceText.text = GetPriceForItem(IAP_IDS.birdIAP1);
            landscapeBirdIAP2PriceText.text = GetPriceForItem(IAP_IDS.birdIAP2);
            landscapeBirdIAP3PriceText.text = GetPriceForItem(IAP_IDS.birdIAP3);
            landscapeBirdIAP4PriceText.text = GetPriceForItem(IAP_IDS.birdIAP4);

            landscapeCoinIAP1Button.onClick.AddListener(CoinIAP1ButtonClicked);
            landscapeCoinIAP2Button.onClick.AddListener(CoinIAP2ButtonClicked);
            landscapeCoinIAP3Button.onClick.AddListener(CoinIAP3ButtonClicked);
            landscapeBirdIAP1Button.onClick.AddListener(BirdIAP1ButtonClicked);
            landscapeBirdIAP2Button.onClick.AddListener(BirdIAP2ButtonClicked);
            landscapeBirdIAP3Button.onClick.AddListener(BirdIAP3ButtonClicked);
            landscapeBirdIAP4Button.onClick.AddListener(BirdIAP4ButtonClicked);

            landscapeLivesEarnedButton.onClick.AddListener(EarnedLivesButtonClicked);
            landscapeLivesIAP1Button.onClick.AddListener(LivesIAP1ButtonClicked);
            landscapeLivesIAP2Button.onClick.AddListener(LivesIAP2ButtonClicked);

            timerText = landscapeLivesEarnedTimerText;
        }
    }

    private void Update()
    {
        DateTime dateToClaimLives = inventoryData.GetDateToClaimLives();
        diffTime = dateToClaimLives.Subtract(DateTime.Now);

        if (diffTime.Ticks < 0)
        {
            timerText.text = "FREE";
        }
        else
        {
            string[] substrings = diffTime.ToString().Split('.');
            timerText.text = substrings[0];
        }
    }

    private void ReturnToMenuButtonClicked()
    {
        levelChangerScript.FadeToPreviousScene();
    }

    private void InventoryButtonClicked()
    {
        if (false == inventoryCanvas.gameObject.activeInHierarchy)
        {
            inventoryCanvas.gameObject.SetActive(true);
        }
    }

    private void InventoryDismissButtonClicked()
    {
        if (inventoryCanvas.gameObject.activeInHierarchy)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
    }

    private string GetPriceForItem(IAP_IDS id)
    {
        switch (id)
        {
            case IAP_IDS.coinIAP1:
                return "$0.99";

            case IAP_IDS.coinIAP2:
                return "$2.99";

            case IAP_IDS.coinIAP3:
                return "$4.99";

            case IAP_IDS.birdIAP1:
                return "$0.99";

            case IAP_IDS.birdIAP2:
                return "$0.99";

            case IAP_IDS.birdIAP3:
                return "$0.99";

            case IAP_IDS.birdIAP4:
                return "$0.99";
        }

        return "$ERROR";
    }

    public void CoinIAP1ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.coinIAP1) + " for coins!");
        purchaseManager.MakePurchase(IAPManager.COIN_PACK1_ID);
    }

    public void CoinIAP2ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.coinIAP2) + " for coins!");
        purchaseManager.MakePurchase(IAPManager.COIN_PACK2_ID);
    }

    public void CoinIAP3ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.coinIAP3) + " for coins!");
        purchaseManager.MakePurchase(IAPManager.COIN_PACK3_ID);
    }

    public void BirdIAP1ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.birdIAP1) + " for bird 1!");
        purchaseManager.MakePurchase(IAPManager.KOKO2_PRODUCT_ID);
    }

    public void BirdIAP2ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.birdIAP2) + " for bird 2!");
        purchaseManager.MakePurchase(IAPManager.SAM2_PRODUCT_ID);
    }

    public void BirdIAP3ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.birdIAP3) + " for bird 3!");
        purchaseManager.MakePurchase(IAPManager.NIGEL2_PRODUCT_ID);
    }

    public void BirdIAP4ButtonClicked()
    {
        print("Spending " + GetPriceForItem(IAP_IDS.birdIAP4) + " for bird 4!");
        purchaseManager.MakePurchase(IAPManager.STEVEN2_PRODUCT_ID);
    }

    public void EarnedLivesButtonClicked()
    {
        // Check if the timer has reached zero or if enough time has elapsed and
        // the user can claim their 3 lives for this 24 hours period
        print("Check to see if user has earned their 3 lives...");
        inventoryData.ClaimLives();
    }

    public void LivesIAP1ButtonClicked()
    {
        // Display an ad in exchange for 3 lives
        print("User is watching ad for 3 lives!");
        adObjectScript.ShowRewardLivesAd();
    }

    public void LivesIAP2ButtonClicked()
    {
        if (inventoryData.GetItemCount(InventoryData.coinKey) >= PURCHASE_LIVES_AMOUNT)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>
            {
                [InventoryData.coinKey] = PURCHASE_LIVES_AMOUNT,
            };

            inventoryData.SpendCurrency(dict, FirebaseManager.CURRENCY_SPEND_PURCHASE_LIVES);
            inventoryData.AddLives(LIVES_PURCHASED_WITH_COINS);
        }
    }
}
