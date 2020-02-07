using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleMobileAds.Api;

public class CommonBehavior : MonoBehaviour
{
    public enum GameState
    {
        Loss = -1,
        Wait = 0,
        Active = 1,
        Win = 2,
        Done = 10
    };

    // Public level variables
    public bool cleanup;

    public GameState state = GameState.Wait;

    public bool userWasRescued;

    /*
     * Menu game objects
     */
    public GameObject reviewCanvas;
    private ReviewCanvasBehavior reviewScript;

    public GameObject footerMenuPrefab;
    private GameObject footerMenuCanvas;
    private FooterButtonsBehavior footerButtonsScript;

    public GameObject startMenuPrefab;
    private GameObject startMenuCanvas;
    private StartMenuBehavior startMenuScript;

    public GameObject playCanvasPrefab;
    private GameObject playCanvas;
    private PlayCanvasBehavior playCanvasScript;

    public GameObject pauseMenuPrefab;
    private GameObject pauseMenu;
    private PauseMenuBehavior pauseMenuScript;

    public GameObject itemsCanvasPrefab;
    private GameObject itemsCanvas;
    private ItemsCanvasBehavior itemsCanvasScript;

    public GameObject inventoryCanvasPrefab;
    private GameObject inventoryCanvas;
    private InventoryCanvasBehavior inventoryCanvasScript;

    public GameObject rescueCanvasPrefab;
    private GameObject rescueCanvas;
    private RescueCanvasBehavior rescueCanvasScript;

    public GameObject gameOverCanvasPrefab;
    private GameObject gameOverCanvas;
    private GameOverCanvasBehavior gameOverCanvasScript;

    public GameObject gameOverNewHighScoreCanvasPrefab;
    private GameObject gameOverNewHighScoreCanvas;
    private NewHighScoreCanvasBehavior gameOverNewHighScoreScript;

    public GameObject gameOverNextCanvasPrefab;
    private GameObject gameOverNextCanvas;
    private GameOverNextCanvasBehavior gameOverNextCanvasScript;

    public GameObject gameOverNewHighScoreNextCanvasPrefab;
    private GameObject gameOverNewHighScoreNextCanvas;
    private NewHighScoreCanvasNextBehavior gameOverNewHighScoreNextScript;

    public GameObject messageCanvas;
    private MessageCanvasBehavior messageCanvasScript;

    public GameObject hintMessageCanvas;
    private HintMessageCanvasBehavior hintMessageCanvasScript;

    // Rename to gameBird when it changes to a bird
    // We get this from InventoryData now
    public GameObject kokoPrefab;
    public GameObject samPrefab;

    private GameObject birdObject;
    private BirdBehavior birdScript;

    private int score;
    private int highScore;
    private static int totalAttempts;

    // Needs to be accessible by the parent Level object
    public bool disableColliders;

    private CameraBehavior cameraScript;
    private Camera mainCamera;

    public Queue<GameObject> itemsCollected;

    public GameObject levelChanger;
    private LevelChanger levelChangerScript;

    private uint activeBirdId;

    private bool newHighScore;


    #region GoogleAds
    /*
     * Google Ad stuff
     */
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardAd;
    private bool rewardUser;
    private readonly int interstitialThreshold = 3;

    // This needs to be set to false to enable real ads
    private readonly bool TESTING = true;

    private NotificationManager notificationMgr;

    private int currentLevelNumber;

    private void InitializeAdTypes()
    {
#if UNITY_ANDROID || UNITY_IOS
        AdManager.Initialize(TESTING);
#endif

        // Load the banner ad
        bannerView = AdManager.instance.GetBannerView();
        bannerView.Hide();

        // Load the interstitial ad
        InitializeInterstitialAd();

        // Load the reward ad
        InitializeRewardAd();
    }


    /*
     * Interstitial ad functions
     */
    private void InitializeInterstitialAd()
    {
        // This will always need to be reinitialized
        interstitialAd = AdManager.instance.GetInterstitialAd();

        // Called when user closes an interstitial ad
        interstitialAd.OnAdClosed += HandleInterstitialAdClosed;

    }

    public void HandleInterstitialAdClosed(object sender, EventArgs args)
    {
        // Interstitial ad ended; reload the scene now
        levelChangerScript.FadeToSameScene();
    }

    private void UnsubscribeInterstitialAdEvents()
    {
        interstitialAd.OnAdClosed -= HandleInterstitialAdClosed;
    }


    /*
     * Reward ad functions
     */
    private void InitializeRewardAd()
    {
        rewardAd = AdManager.instance.GetRewardAd();

        // Called when the user should be rewarded for interacting with the ad.
        rewardAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardAd.OnAdClosed += HandleRewardAdClosed;

        rewardAd.OnAdFailedToLoad += HandleRewardAdLoadFailed;
    }

    private void UnsubscribeRewardAdEvents()
    {
        rewardAd.OnUserEarnedReward -= HandleUserEarnedReward;
        rewardAd.OnAdClosed -= HandleRewardAdClosed;
        rewardAd.OnAdFailedToLoad -= HandleRewardAdLoadFailed;
    }

    public void HandleRewardAdLoadFailed(object sender, AdErrorEventArgs args)
    {
        /*
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
            Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            // Don't do anything if the network is still accessible
            return;
        }

        // If the network is inaccessible, notify the user
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            playCanvasScript.heartImage.sprite = playCanvasScript.noLifeSprite;
            userWasRescued = true;

            if (false == PlayerPrefsCommon.GetUserIsOffline())
            {
                string message = "It seems you're offline. You won't be able to rescue yourself after hitting an obstacle until you're back online.";
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissUserIsOfflinePopup);
                hintMessageCanvasScript.ShowMessage(message);
            }
        }
        */
    }

    private void DismissUserIsOfflinePopup()
    {
        PlayerPrefsCommon.SetUserIsOffline(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissUserIsOfflinePopup);
    }

    public void HandleRewardAdClosed(object sender, EventArgs args)
    {
        // Don't need to load a new reward ad here, wait until the level restarts
        if (rewardUser)
        {
            // Reset the variable here
            rewardUser = false;
            RescueContinueGame();
        }
        else
        {
            RescueDontContinueGame();
        }
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardUser = true;
    }

    /*
     * End of Google Ad stuff
     */
    #endregion



    void Awake()
    {
        currentLevelNumber = LevelManager.GetCurrentLevel();

        activeBirdId = PlayerPrefsCommon.GetActiveBirdId();
        GameObject birdPrefab;
        if (false == InventoryData.Instance().ValidateBird(activeBirdId))
        {
            PlayerPrefsCommon.SetActiveBirdId(InventoryData.KOKO_ID);
            activeBirdId = PlayerPrefsCommon.GetActiveBirdId();
        }

        itemsCollected = new Queue<GameObject>();

        // User can't just set the preferences to a bird ID; the bird must be
        // validated by InventoryData
        birdPrefab = GetPrefabForId(activeBirdId);

        birdObject = Instantiate(birdPrefab);
        birdScript = birdObject.GetComponent<BirdBehavior>();

        birdScript.commonObject = this.gameObject;
        birdScript.commonScript = this;

        cameraScript = Camera.main.GetComponent<CameraBehavior>();

        levelChangerScript = levelChanger.GetComponent<LevelChanger>();

        // Menu instantiation
        footerMenuCanvas = Instantiate(footerMenuPrefab);
        footerButtonsScript = footerMenuCanvas.GetComponent<FooterButtonsBehavior>();
        footerButtonsScript.exitButton.onClick.AddListener(FadeToPreviousScene);
        // Inventory footer button click listener is being set in FooterButtonsBehavior
        footerButtonsScript.inventoryButton.onClick.AddListener(InventoryButtonClicked);

        startMenuCanvas = Instantiate(startMenuPrefab);
        startMenuScript = startMenuCanvas.GetComponent<StartMenuBehavior>();
        if (currentLevelNumber > 0)
        {
            startMenuScript.startText.text = "Level " + currentLevelNumber;
        }
        else
        {
            startMenuScript.startText.text = "Classic";
        }
        startMenuScript.playButton.onClick.AddListener(delegate { StartGame(true); });

        playCanvas = Instantiate(playCanvasPrefab);
        playCanvasScript = playCanvas.GetComponent<PlayCanvasBehavior>();
        // In landscape mode, slide the pause button down into the corner
        if (currentLevelNumber > 0)
        {
            Vector3 newPos = footerButtonsScript.inventoryButton.transform.localPosition;
            playCanvasScript.pauseButton.transform.localPosition = newPos;
        }
        // Add code to deactivate pause button when another menu is showing
        playCanvasScript.pauseButton.onClick.AddListener(pauseButtonClicked);
        playCanvasScript.heartImage.sprite = playCanvasScript.fullLifeSprite;
        playCanvas.gameObject.SetActive(false);

        pauseMenu = Instantiate(pauseMenuPrefab);
        pauseMenuScript = pauseMenu.GetComponent<PauseMenuBehavior>();
        pauseMenuScript.resumeButton.onClick.AddListener(delegate { StartGame(false); });
        pauseMenu.gameObject.SetActive(false);

        itemsCanvas = Instantiate(itemsCanvasPrefab);
        itemsCanvasScript = itemsCanvas.GetComponent<ItemsCanvasBehavior>();
        itemsCanvasScript.Initialize();
        itemsCanvas.gameObject.SetActive(false);
        // On start, this canvas will hide all of its elements

        inventoryCanvas = Instantiate(inventoryCanvasPrefab);
        inventoryCanvasScript = inventoryCanvas.GetComponent<InventoryCanvasBehavior>();
        inventoryCanvasScript.inventoryDismissButton.onClick.AddListener(InventoryCanvasDismissed);
        inventoryCanvas.gameObject.SetActive(false);

        rescueCanvas = Instantiate(rescueCanvasPrefab);
        rescueCanvasScript = rescueCanvas.GetComponent<RescueCanvasBehavior>();
        rescueCanvasScript.watchAdButton.onClick.AddListener(ShowRewardAd);
        rescueCanvasScript.dismissAdButton.onClick.AddListener(RescueDontContinueGame);
        rescueCanvas.gameObject.SetActive(false);

        gameOverCanvas = Instantiate(gameOverCanvasPrefab);
        gameOverCanvasScript = gameOverCanvas.GetComponent<GameOverCanvasBehavior>();
        gameOverCanvasScript.retryButton.onClick.AddListener(RestartGame);
        gameOverCanvas.gameObject.SetActive(false);

        gameOverNewHighScoreCanvas = Instantiate(gameOverNewHighScoreCanvasPrefab);
        gameOverNewHighScoreScript = gameOverNewHighScoreCanvas.GetComponent<NewHighScoreCanvasBehavior>();
        gameOverNewHighScoreScript.retryButton.onClick.AddListener(RestartGame);
        gameOverNewHighScoreCanvas.gameObject.SetActive(false);

        gameOverNextCanvas = Instantiate(gameOverNextCanvasPrefab);
        gameOverNextCanvasScript = gameOverNextCanvas.GetComponent<GameOverNextCanvasBehavior>();
        gameOverNextCanvasScript.retryButton.onClick.AddListener(RestartGame);
        gameOverNextCanvasScript.nextLevelButton.onClick.AddListener(NextLevelButtonClicked);
        gameOverNextCanvas.gameObject.SetActive(false);

        gameOverNewHighScoreNextCanvas = Instantiate(gameOverNewHighScoreNextCanvasPrefab);
        gameOverNewHighScoreNextScript = gameOverNewHighScoreNextCanvas.GetComponent<NewHighScoreCanvasNextBehavior>();
        gameOverNewHighScoreNextScript.retryButton.onClick.AddListener(RestartGame);
        gameOverNewHighScoreNextScript.nextLevelButton.onClick.AddListener(NextLevelButtonClicked);
        gameOverNewHighScoreNextCanvas.gameObject.SetActive(false);

        messageCanvasScript = messageCanvas.GetComponent<MessageCanvasBehavior>();
        messageCanvas.gameObject.SetActive(false);

        hintMessageCanvasScript = hintMessageCanvas.GetComponent<HintMessageCanvasBehavior>();
        hintMessageCanvas.gameObject.SetActive(false);

        // Level 0 is classic mode
        if (0 == currentLevelNumber)
        {
            if (false == PlayerPrefsCommon.GetHowtoPlayClassic())
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissHowotoPlayClassicClicked);
                string message = "Welcome to Classic mode!\n\nTap to fly and avoid obstacles for as long as you can. Always strive to beat your high score!";
                hintMessageCanvasScript.ShowMessage(message);
            }
        }

        if (1 == currentLevelNumber)
        {
            if (false == PlayerPrefsCommon.GetHowtoPlayPopup())
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissHowotoPlayPopupClicked);
                string message = "Welcome to Adventure mode!\n\nComplete level 1 by crossing the finish line and avoiding flying obstacles. Collect bananas and coins along the way!";
                hintMessageCanvasScript.ShowMessage(message);
            }
        }

        if (3 == currentLevelNumber)
        {
            if (false == PlayerPrefsCommon.GetUncommonFruitNew())
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissUncommonFruitPopupClicked);
                string message = "Level 3 introduces a new fruit: blueberries.\n\n";
                message += "Continue to catch as many coins, bananas, and blueberries as you can!";
                hintMessageCanvasScript.ShowMessage(message);
            }
        }

        if (5 == currentLevelNumber)
        {
            if (false == PlayerPrefsCommon.GetRareFruitNew())
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissRareFruitPopupClicked);
                string message = "Level 5 contains 2 new items: strawberries and an egg!\n\n";
                message += "Make sure you don't miss the egg!";
                hintMessageCanvasScript.ShowMessage(message);
            }
        }

        InventoryData inventoryData = InventoryData.Instance();
        LevelData levelData = LevelData.Instance();
        reviewCanvas.gameObject.SetActive(false);
        if (false == PlayerPrefsCommon.GetReviewPrompt1())
        {
            if (levelData.levelData.level5Complete && inventoryData.HasEggHatched(InventoryData.SAM_ID))
            {
                reviewScript = reviewCanvas.GetComponent<ReviewCanvasBehavior>();
                reviewScript.dismissButton.onClick.AddListener(DismissReviewCanvas);
                reviewScript.reviewButton.onClick.AddListener(RedirectForReview);
                reviewCanvas.gameObject.SetActive(true);
            }
        }

        notificationMgr = NotificationManager.Instance();
    }

    private void DismissReviewCanvas()
    {
        PlayerPrefsCommon.SetReviewPrompt1(true);
        reviewCanvas.gameObject.SetActive(false);
    }

    private void RedirectForReview()
    {
#if UNITY_IOS
        string url = "https://apps.apple.com/us/app/brick-break/id1491060128";
#elif UNITY_ANDROID
        string url = "https://play.google.com/store/apps/details?id=xyz.ashgames.hovrbird";
#elif UNITY_EDITOR
        string url = "https://apps.apple.com/us/app/brick-break/id1491060128";
#endif

        Application.OpenURL(url);
    }

    private void DismissHowotoPlayClassicClicked()
    {
        PlayerPrefsCommon.SetHowtoPlayClassic(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissHowotoPlayClassicClicked);
    }

    private void DismissHowotoPlayPopupClicked()
    {
        PlayerPrefsCommon.SetHowtoPlayPopup(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissHowotoPlayPopupClicked);
    }

    private void DismissUncommonFruitPopupClicked()
    {
        PlayerPrefsCommon.SetUncommonFruitNew(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissUncommonFruitPopupClicked);
    }

    private void DismissRareFruitPopupClicked()
    {
        PlayerPrefsCommon.SetRareFruitNew(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissRareFruitPopupClicked);
    }

    private void InventoryButtonClicked()
    {
        // Clear the notification for first items collected if it's showing
        if (notificationMgr.notifications[NotificationManager.firstTimeItemsNotificationId])
        {
            notificationMgr.NotificationChange(NotificationManager.firstTimeItemsNotificationId,
                                               NotificationManager.CLEAR_NOTIFICATION);
        }

        inventoryCanvas.gameObject.SetActive(true);
    }

    private void InventoryCanvasDismissed()
    {
        inventoryCanvas.gameObject.SetActive(false);

        if (startMenuCanvas.activeInHierarchy)
        {
            uint newActiveBirdId = PlayerPrefsCommon.GetActiveBirdId();
            if (newActiveBirdId != activeBirdId)
            {
                if (false == InventoryData.Instance().ValidateBird(newActiveBirdId))
                {
                    PlayerPrefsCommon.SetActiveBirdId(InventoryData.KOKO_ID);
                    activeBirdId = PlayerPrefsCommon.GetActiveBirdId();
                }
                else
                {
                    activeBirdId = newActiveBirdId;
                    FadeToSameScene();
                }
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (state == GameState.Active)
        {
            pauseButtonClicked();
        }
    }

    private void OnApplicationQuit()
    {
        notificationMgr.SaveUserData();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeAdTypes();

        if (null != cameraScript)
        {
            if (false == cameraScript.moveWithBird)
            {
                cameraScript.moveWithBird = true;
            }
        }

        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Touch screenTap;
        bool mouseClick = false;
        switch (state)
        {
            case GameState.Active:
                // Get a screen touch or mouse click (mostly for testing on a desktop)
                if (Input.touchCount > 0 || ((mouseClick = Input.GetMouseButtonDown(0)) != false))
                {
                    if (mouseClick)
                    {
                        if (false == EventSystem.current.IsPointerOverGameObject())
                        {
                            birdScript.fly = true;
                        }
                    }
                    else
                    {
                        screenTap = Input.GetTouch(0);

                        if (false == EventSystem.current.IsPointerOverGameObject(screenTap.fingerId))
                        {
                            if (screenTap.phase == TouchPhase.Began)
                            {
                                birdScript.fly = true;
                            }
                        }
                    }
                }

                break;

            case GameState.Done:
                if (null != cameraScript)
                {
                    cameraScript.moveWithBird = false;
                }
                break;
        }
    }


    /*
     * Functions that need to be exposed for the level object and are expected
     * to work a certain way. All other functions and code can be modified if
     * needed.
     */
#region ExposedToLevel

    // Variable: (bool) disableColliders
    // Variable: (Queue) itemsCollected

    public GameObject GetBirdObject()
    {
        return birdObject;
    }

    public GameState GetGameState()
    {
        return state;
    }

    public void AddCollisionTags(List<string> tags)
    {
        birdScript.AddCollisionTags(tags);
    }

    public void AddTriggerCollisionTags(List<string> tags)
    {
        birdScript.AddTriggerCollisionTags(tags);
    }

    /*
     * This function is called from the parent Level object once it's done saving and cleaning up
     * whatever it needs so we can restart the level whenever the user is ready
     */
    // Changes game state to Done
    public void CleanupDone(bool nextLevelUnlocked = false)
    {
        state = GameState.Done;

        playCanvasScript.pauseButton.gameObject.SetActive(false);

        if (nextLevelUnlocked)
        {
            if (newHighScore)
            {
                ShowGameOverNewHighScoreNextCanvas();
            }
            else
            {
                ShowGameOverNextCanvas();
            }
        }
        else
        {
            if (newHighScore)
            {
                ShowGameOverNewHighScoreCanvas();
            }
            else
            {
                ShowGameOverCanvas();
            }
        }

        if (false == PlayerPrefsCommon.GetCollectedFirstEgg() && 5 == currentLevelNumber)
        {
            InventoryData iData = InventoryData.Instance();
            if (iData.birdDict.Count > 1)
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissCollectedFirstEggPopupClicked);
                string message = "You caught your first egg!\n\n";
                message += "View the requirements for hatching the egg in your inventory by clicking the Birds tab.";
                hintMessageCanvasScript.ShowMessage(message);
            }
        }
    }

    private void DismissCollectedFirstEggPopupClicked()
    {
        PlayerPrefsCommon.SetCollectedFirstEgg(true);
        hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissCollectedFirstEggPopupClicked);
    }

    public void UpdateHighScore(int score, bool newHighScore = false)
    {
        highScore = score;

        if (newHighScore)
        {
            this.newHighScore = newHighScore;
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void InitItemCanvasImage(int itemNumber, Sprite sprite)
    {
        itemsCanvasScript.InitItemNumber(itemNumber, sprite);
    }

    public void SetItemCanvasText(int itemNumber, string text)
    {
        itemsCanvasScript.SetItemNumberText(itemNumber, text);
    }


#endregion







    public void DisableCollisions()
    {
        disableColliders = true;
    }

    public void EnableCollisions()
    {
        disableColliders = false;
    }

    public void CollisionOccurred(GameObject obj)
    {
        GameOver();
    }

    public void TriggerCollisionOccurred(GameObject obj)
    {
        if (obj.tag == "Score")
        {
            // Update score here using an event
            IncrementScore();
        }
        else if (obj.tag == "FinishLine")
        {
            GameOver(success: true);
        }
        else if (obj.tag == "Collectible")
        {
            IncrementScore();
            itemsCollected.Enqueue(obj);
        }
    }

    public void pauseButtonClicked()
    {
        StopGame();
        ShowPauseMenu();
    }

    public void NextLevelButtonClicked()
    {
        LevelManager.SetLevelNumber(currentLevelNumber + 1);

        if (interstitialAd.IsLoaded() && (totalAttempts % interstitialThreshold == 0))
        {
            interstitialAd.Show();
        }
        else
        {
            FadeToSameScene();
        }
    }

    // Switches state from Wait to Active
    public void StartGame(bool impulse = false)
    {
        state = GameState.Active;

        if (impulse)
        {
            // Give the bird an impulse
            birdScript.fly = true;
        }

        // Tell the bird to start the game
        birdScript.startGame = true;

        if (false == playCanvas.activeInHierarchy)
        {
            playCanvas.gameObject.SetActive(true);
        }

        if (false == itemsCanvas.activeInHierarchy)
        {
            itemsCanvas.gameObject.SetActive(true);
        }

        // Hide the start menu if they're showing
        DismissStartMenu();
        DismissPauseMenu();

        bannerView.Show();

        FirebaseManager.LogLevelStartEvent(currentLevelNumber);
    }

    // Switches state from Active to Wait
    public void StopGame()
    {
        state = GameState.Wait;
        birdScript.DisableGravity();

        // Tell the bird to stop the game
        birdScript.startGame = false;
    }

    private void OnDestroy()
    {
        UnsubscribeInterstitialAdEvents();
        UnsubscribeRewardAdEvents();

        bannerView.Destroy();
        interstitialAd.Destroy();
    }

    public string GetLastCollisionTag()
    {
        return birdScript.collisionTag;
    }

    public string GetLastTriggerCollisionTag()
    {
        return birdScript.triggerCollisionTag;
    }

    public void ShowRewardAd()
    {
#if UNITY_EDITOR
        rewardUser = false;
        RescueContinueGame();
#else
        if (rewardAd.IsLoaded())
        {
            rewardAd.Show();
        }
        else
        {
            // If the ad isn't loaded, just go straight to the end of the game
            RescueDontContinueGame();
        }
#endif
    }

    public void RescueContinueGame()
    {
        // Continue game after user watches an ad

        // Allow game object to go through game obstacles temporarily and "blink"
        birdScript.Rescued();

        userWasRescued = true;
        StartGame();

        rescueCanvas.gameObject.SetActive(false);
        playCanvasScript.heartImage.sprite = playCanvasScript.noLifeSprite;
        playCanvasScript.pauseButton.gameObject.SetActive(true);
    }

    public void RescueDontContinueGame()
    {
        userWasRescued = true;
        rescueCanvas.gameObject.SetActive(false);
        GameOver();
    }

    // Changes state to either Wait (if user wasn't rewarded), Win (if the user
    // crossed the finish line), or Loss (if user lost)
    public void GameOver(bool success = false)
    {
        // Do anything that's necessary to end the game

        if (success)
        {
            StopGame();

            if (false == PlayerPrefsCommon.GetCompletedLevel1() && 1 == currentLevelNumber)
            {
                hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissLevel1CompletedPopupClicked);
                string message = "Congratulations!\n\nYou unlocked level 2!\n\n";
                message += "You also collected " + score + " items! Check your inventory by tapping the backpack button on the bottom right.";
                hintMessageCanvasScript.ShowMessage(message);
            }

            FirebaseManager.LogLevelEndEvent(currentLevelNumber, 1);

            totalAttempts += 1;

            state = GameState.Win;

            birdScript.RemoveAllCollisionTags();
        }
        else
        {
            if (false == userWasRescued && rewardAd.IsLoaded())
            {
                StopGame();

                // Warn the user here if they have items
                //gameOverRescueText.text = "Items will be lost if you quit!\nContinue?";

                ShowRescueCanvas();
            }
            else
            {
                FirebaseManager.LogLevelEndEvent(currentLevelNumber, 0);

                totalAttempts += 1;

                state = GameState.Loss;

                birdScript.RemoveAllCollisionTags();
            }
        }
    }

    private void DismissLevel1CompletedPopupClicked()
    {
        PlayerPrefsCommon.SetCompletedLevel1(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissLevel1CompletedPopupClicked);
    }

    public void RestartGame()
    {
#if UNITY_EDITOR
        FadeToSameScene();
#else
        // Show interstitial ad here
        if (interstitialAd.IsLoaded() && (totalAttempts % interstitialThreshold == 0))
        {
            interstitialAd.Show();
        }
        else
        {
            FadeToSameScene();
        }
#endif
    }

    public void FadeToSameScene()
    {
        levelChangerScript.FadeToSameScene();
    }

    public void FadeToScene(string sceneName)
    {
        levelChangerScript.FadeToScene(sceneName);
    }

    public void FadeToPreviousScene()
    {
        levelChangerScript.FadeToPreviousScene();
    }

    private void IncrementScore()
    {
        score += 1;
        playCanvasScript.scoreText.text = score.ToString();
    }

    private GameObject GetPrefabForId(uint id)
    {
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return kokoPrefab;

            case InventoryData.SAM_ID:
                // Validate with InventoryData to ensure the user actually has access to this
                return samPrefab;

            default:
                PlayerPrefsCommon.SetActiveBirdId(InventoryData.KOKO_ID);
                return kokoPrefab;
        }
    }

    private void ShowGameOverNewHighScoreCanvas()
    {
        gameOverNewHighScoreScript.SetHighScoreNumber(highScore);

        if (rescueCanvas.activeInHierarchy)
        {
            rescueCanvas.gameObject.SetActive(false);
        }

        gameOverNewHighScoreCanvas.gameObject.SetActive(true);

        ShowFooterButtons();
    }

    private void ShowGameOverCanvas()
    {
        // Set the scores in the game over script to update the labels/texts
        gameOverCanvasScript.SetHighScoreText(highScore);
        gameOverCanvasScript.SetScoreText(score);

        if (rescueCanvas.activeInHierarchy)
        {
            rescueCanvas.gameObject.SetActive(false);
        }

        gameOverCanvas.gameObject.SetActive(true);

        ShowFooterButtons();
    }

    private void ShowGameOverNewHighScoreNextCanvas()
    {
        gameOverNewHighScoreNextScript.SetHighScoreNumber(highScore);

        if (rescueCanvas.activeInHierarchy)
        {
            rescueCanvas.gameObject.SetActive(false);
        }

        gameOverNewHighScoreNextCanvas.gameObject.SetActive(true);

        ShowFooterButtons();
    }

    private void ShowGameOverNextCanvas()
    {
        // Set the scores in the game over script to update the labels/texts
        gameOverNextCanvasScript.SetHighScoreText(highScore);
        gameOverNextCanvasScript.SetScoreText(score);

        if (rescueCanvas.activeInHierarchy)
        {
            rescueCanvas.gameObject.SetActive(false);
        }

        gameOverNextCanvas.gameObject.SetActive(true);

        ShowFooterButtons();
    }

    private void ShowRescueCanvas()
    {
        if (0 == currentLevelNumber)
        {
            rescueCanvas.gameObject.SetActive(true);
            rescueCanvasScript.rescueWarningImage.gameObject.SetActive(false);
            playCanvasScript.pauseButton.gameObject.SetActive(false);
        }
        else
        {
            rescueCanvas.gameObject.SetActive(true);
            playCanvasScript.pauseButton.gameObject.SetActive(false);
        }
    }

    private void ShowPauseMenu()
    {
        playCanvas.SetActive(false);

        pauseMenu.gameObject.SetActive(true);
        ShowFooterButtons();
    }

    private void ShowFooterButtons()
    {
        footerMenuCanvas.gameObject.SetActive(true);

        bannerView.Hide();
    }

    private void DismissStartMenu()
    {
        if (startMenuCanvas.activeInHierarchy)
        {
            startMenuCanvas.gameObject.SetActive(false);
        }

        DismissFooterButtons();
    }

    private void DismissPauseMenu()
    {
        if (pauseMenu.activeInHierarchy)
        {
            pauseMenu.gameObject.SetActive(false);
        }

        DismissFooterButtons();
    }

    private void DismissFooterButtons()
    {
        if (footerMenuCanvas.activeInHierarchy)
        {
            footerMenuCanvas.gameObject.SetActive(false);
        }
    }
}
