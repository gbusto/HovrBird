using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachSeriesLevelBehavior : MonoBehaviour
{
    // NOTE: Common level attributes
    public GameObject commonPrefab;
    private GameObject commonObject;
    private CommonBehavior commonScript;

    // NOTE: Common level attributes
    public GameObject progressLinePrefab;
    private const float progress25 = 0.25f;
    private bool showedProgress25;
    private const float progress50 = 0.50f;
    private bool showedProgress50;
    private const float progress75 = 0.75f;
    private bool showedProgress75;

    public GameObject gameItemObject;
    private GameItemsBehavior gameItemObjectScript;

    // NOTE: Common level attributes
    public GameObject gameCamera;
    public GameObject background;

    public Sprite daySky;
    public Sprite eveningSky;
    public Sprite nightSky;
    public Sprite duskSky;
    public Sprite stormSky;
    public Sprite nightSand;
    public Sprite nightWater;

    private int collectibleGenerationCounter = collectibleGenerationTurns;
    private const int collectibleGenerationTurns = 37;

    private int obstacleGenerationCounter = obstacleGenerationTurns;
    private const int obstacleGenerationTurns = 14;

    // NOTE: Common level attributes
    public GameObject groundObject1;
    private Renderer ground1Renderer;
    public GameObject groundObject2;
    private Renderer ground2Renderer;
    private float groundObjectsDistance;

    public GameObject waterObject1;
    private Renderer waterObject1Renderer;
    public GameObject waterObject2;
    private Renderer waterObject2Renderer;
    private float waterObjectsDistance;

    public GameObject stormCloudCluster1;
    private Renderer cloudCluster1Renderer;
    public GameObject stormCloudCluster2;
    private Renderer cloudCluster2Renderer;
    private float cloudClusterObjectsDistance;

    // Sprites for collectibles to show in the collectibles menu at the top
    // of the screen
    private int item1Count;
    private int item2Count;
    private int item3Count;
    private int item4Count;

    // The bird object
    private GameObject bird;
    // A script that tells the camera to follow the bird as it moves through the level
    private CameraBehavior cameraScript;

    private bool userWon;

    // Collectible and game obstacle arrays for generating them
    // XXX Add these attributes here

    // Base speed at which all objects should move
    private static readonly float speed = 0.04f;

    // NOTE: Common level attributes
    private float cameraMaxX;
    private float backgroundMinY;
    private float backgroundMaxY;
    private float groundMaxY;

    // NOTE: Common level attributes
    private bool collidersWereDisabled;

    // NOTE: Common level attributes
    private bool generateItems = true;
    private float levelTimer;
    private static readonly float levelTimeLength = 60.0f;

    // We generate only one egg in the last level of a series;
    // generate it once in the level until the user is able to collec it
    private bool generateEggItem;
    // The randomized time at which the egg should be generated
    private float timeForEggGeneration;
    private bool eggHasBeenGenerated;
    private bool timeToGenerateEgg;
    private bool caughtEgg;

    private float minCollectibleHeight;
    private float maxCollectibleHeight;

    private const int NUM_OBSTACLES = 15;
    private const int NUM_COLLECTIBLES = 7;

    // NOTE: Common level attributes
    private LevelData levelData;
    private InventoryData inventoryData;

    private void Awake()
    {
        // NOTE: Common level initialization
        Screen.orientation = ScreenOrientation.Landscape;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // NOTE: Common level initialization
        levelData = LevelData.Instance();
        inventoryData = InventoryData.Instance();
    }

    // Start is called before the first frame update
    void Start()
    {
        // NOTE: Common level initialization
        Application.targetFrameRate = 60;

        // Set far right bounds for where objects should be generated
        if (Screen.width > Screen.height)
        {
            // Calculate the offscreen startpoint when the screen is wider than it is tall
            cameraMaxX = (Camera.main.orthographicSize * 2.0f) * (Screen.width / Screen.height);
        }
        else if (Screen.height > Screen.width)
        {
            // Calculate the offscreen startpoint when the screen is taller than it is wide (phones or portrait iPad)
            cameraMaxX = Camera.main.orthographicSize;
        }

        commonObject = Instantiate(commonPrefab);
        commonScript = commonObject.GetComponent<CommonBehavior>();

        gameItemObjectScript = gameItemObject.GetComponent<GameItemsBehavior>();

        bird = commonScript.GetBirdObject();

        cameraScript = gameCamera.GetComponent<CameraBehavior>();
        cameraScript.bird = bird;

        List<string> collisionTags = new List<string>()
        {
            "Obstacle",
            "Ground"
        };
        commonScript.AddCollisionTags(collisionTags);

        List<string> triggerTags = new List<string>()
        {
            "Collectible",
            "FinishLine"
        };
        commonScript.AddTriggerCollisionTags(triggerTags);

        // Camera orientation and off-screen item start calculation is now done
        // after the user presses the Start button

        ground1Renderer = groundObject1.GetComponent<Renderer>();
        ground2Renderer = groundObject2.GetComponent<Renderer>();
        groundObjectsDistance = groundObject2.transform.localPosition.x - groundObject1.transform.localPosition.x;

        waterObject1Renderer = waterObject1.GetComponent<Renderer>();
        waterObject2Renderer = groundObject2.GetComponent<Renderer>();
        waterObjectsDistance = waterObject2.transform.localPosition.x - waterObject1.transform.localPosition.x;

        cloudCluster1Renderer = stormCloudCluster1.GetComponent<Renderer>();
        cloudCluster2Renderer = stormCloudCluster2.GetComponent<Renderer>();
        cloudClusterObjectsDistance = stormCloudCluster2.transform.localPosition.x - stormCloudCluster1.transform.localPosition.x;

        Renderer backgroundRenderer = background.GetComponent<Renderer>();
        // This minimum value is the minimum Y value at which objects should be generated
        backgroundMinY = ground1Renderer.bounds.max.y + 2;
        backgroundMaxY = backgroundRenderer.bounds.max.y - 2;
        groundMaxY = groundObject1.transform.position.y + ground1Renderer.bounds.size.y;

        minCollectibleHeight = backgroundMinY;
        maxCollectibleHeight = backgroundMaxY;

        gameItemObjectScript.InitGameObjectManager(cameraMaxX, backgroundMinY, backgroundMaxY, NUM_OBSTACLES, NUM_COLLECTIBLES);

        // Switch level prep based on level number
        int levelNumber = LevelManager.GetCurrentLevel();
        switch (levelNumber)
        {
            case 11:
                Level11Prep();
                break;

            case 12:
                Level12Prep();
                break;

            case 13:
                Level13Prep();
                break;

            case 14:
                Level14Prep();
                break;

            case 15:
                Level15Prep();
                break;
        }

        // XXX Seed this with the level number!!!
        // Seed the random number generator here first
        Random.InitState(levelNumber);

        // Start coroutines for checking:
        // 1. Collisions
        // 2. Items collected
        // 3. Items that need to be removed
        StartCoroutine(CheckCollisions());
        StartCoroutine(CheckItemsCollected());
    }



    #region EnumeratorFunctions
    IEnumerator CheckCollisions()
    {
        for (; ; )
        {
            if (commonScript.GetGameState() == CommonBehavior.GameState.Active)
            {
                // Check if we need to disable colliders after a collision
                if (commonScript.disableColliders)
                {
                    // Disable collisions; they will be update when object positions are updated
                    collidersWereDisabled = true;
                    gameItemObjectScript.DisableColliders();
                }
                else if (false == commonScript.disableColliders && collidersWereDisabled)
                {
                    // Enable collisions; they will be updated when object positions are updated
                    collidersWereDisabled = false;
                    gameItemObjectScript.EnableColliders();
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CheckItemsCollected()
    {
        for (; ; )
        {
            while (commonScript.itemsCollected.Count > 0)
            {
                GameObject obj = commonScript.itemsCollected.Dequeue();
                string s = obj.name;
                if (s.Contains("Coin Bird"))
                {
                    item1Count++;
                    commonScript.SetItemCanvasText(0, "+" + item1Count.ToString());
                }
                else if (s.Contains("Banana"))
                {
                    item2Count++;
                    commonScript.SetItemCanvasText(1, "+" + item2Count.ToString());
                }
                else if (s.Contains("Blueberries"))
                {
                    item3Count++;
                    commonScript.SetItemCanvasText(2, "+" + item3Count.ToString());
                }
                else if (s.Contains("Strawberry"))
                {
                    item4Count++;
                    commonScript.SetItemCanvasText(3, "+" + item4Count.ToString());
                }
                else if (s.Contains("Egg"))
                {
                    commonScript.SetItemCanvasText(7, "+1");

                    // The user caught an egg!!!

                    // Save the data saying the user caught the egg!
                    caughtEgg = true;
                }

                gameItemObjectScript.RemoveCollectibleFromGame(obj);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion



    // Update is called once per frame
    void Update()
    {
        switch (commonScript.GetGameState())
        {
            case CommonBehavior.GameState.Active:
                if (false == gameItemObjectScript.active)
                {
                    gameItemObjectScript.StartGame();
                }

                // Update object positions in the game
                // XXX UpdateCloudObjects();
                UpdateGroundObjects();
                UpdateWaterObjects();
                UpdateStormCloudObjects();

                // Update the level timer to know when to end the level
                levelTimer += Time.deltaTime;
                if (levelTimer >= levelTimeLength && generateItems)
                {
                    generateItems = false;

                    // Generate a finish line
                    gameItemObjectScript.GenerateFinishLine();
                }
                else if (levelTimer >= timeForEggGeneration && generateEggItem && false == eggHasBeenGenerated)
                {
                    timeToGenerateEgg = true;
                }

                if ((levelTimer / levelTimeLength >= progress25) && false == showedProgress25)
                {
                    GameObject progressLine = Instantiate(progressLinePrefab);
                    Vector3 pos = progressLine.transform.localPosition;
                    pos.x = cameraMaxX;
                    ProgressLineBehavior plScript = progressLine.GetComponent<ProgressLineBehavior>();
                    plScript.InitProgressLine(commonScript, ProgressLineBehavior.LevelProgress.twentyFive, pos, speed);
                    showedProgress25 = true;
                }
                else if ((levelTimer / levelTimeLength >= progress50) && false == showedProgress50)
                {
                    GameObject progressLine = Instantiate(progressLinePrefab);
                    Vector3 pos = progressLine.transform.localPosition;
                    pos.x = cameraMaxX;
                    ProgressLineBehavior plScript = progressLine.GetComponent<ProgressLineBehavior>();
                    plScript.InitProgressLine(commonScript, ProgressLineBehavior.LevelProgress.fifty, pos, speed);
                    showedProgress50 = true;
                }
                else if ((levelTimer / levelTimeLength >= progress75) && false == showedProgress75)
                {
                    GameObject progressLine = Instantiate(progressLinePrefab);
                    Vector3 pos = progressLine.transform.localPosition;
                    pos.x = cameraMaxX;
                    ProgressLineBehavior plScript = progressLine.GetComponent<ProgressLineBehavior>();
                    plScript.InitProgressLine(commonScript, ProgressLineBehavior.LevelProgress.seventyFive, pos, speed);
                    showedProgress75 = true;
                }

                break;

            case CommonBehavior.GameState.Loss:
                if (gameItemObjectScript.active)
                {
                    gameItemObjectScript.StopGame();
                }
                GameOver();
                break;

            case CommonBehavior.GameState.Win:
                if (gameItemObjectScript.active)
                {
                    gameItemObjectScript.StopGame();
                }
                GameOver(success: true);
                break;

            case CommonBehavior.GameState.Wait:
                if (gameItemObjectScript.active)
                {
                    gameItemObjectScript.StopGame();
                }
                // Don't do anything
                break;

            case CommonBehavior.GameState.Done:
                // Keep objects moving after user wins
                if (userWon)
                {
                    if (false == gameItemObjectScript.active)
                    {
                        gameItemObjectScript.StartGame();
                    }

                    // Ensure all items continue moving after game is over
                    // It looks better than everything just stopping
                    // XXX UpdateCloudObjects();
                    UpdateGroundObjects();
                    UpdateWaterObjects();
                    UpdateStormCloudObjects();
                }
                break;

            default:
                print("Unknown game state!");
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (commonScript.GetGameState())
        {
            case CommonBehavior.GameState.Active:
                if (generateItems)
                {
                    if (obstacleGenerationCounter == obstacleGenerationTurns)
                    {
                        obstacleGenerationCounter = 0;
                        gameItemObjectScript.GenerateNewObstacle(collidersWereDisabled);
                    }

                    if (collectibleGenerationCounter == collectibleGenerationTurns)
                    {
                        collectibleGenerationCounter = 0;
                        gameItemObjectScript.GenerateNewCollectible(timeToGenerateEgg);
                        if (timeToGenerateEgg)
                        {
                            timeToGenerateEgg = false;
                            eggHasBeenGenerated = true;
                        }
                    }
                }

                obstacleGenerationCounter += 1;
                collectibleGenerationCounter += 1;

                break;
        }
    }

    private void GameOver(bool success = false)
    {
        collidersWereDisabled = true;
        gameItemObjectScript.DisableColliders();

        userWon = success;

        bool nextLevelUnlocked = false;

        if (success)
        {
            // Save the collectibles data for the user
            if (caughtEgg)
            {
                inventoryData.AddEggToInventory(InventoryData.STEVEN_ID);
            }

            Dictionary<string, int> currencyDict = new Dictionary<string, int>
            {
                [InventoryData.coinKey] = item1Count,
                [InventoryData.bananaKey] = item2Count,
                [InventoryData.blueberryKey] = item3Count,
                [InventoryData.strawberryKey] = item4Count
            };

            // Add currency and let the inventory save the update dicts
            inventoryData.AddCurrency(currencyDict);

            int score = commonScript.GetScore();
            int levelNumber = LevelManager.GetCurrentLevel();
            uint activeBirdId = PlayerPrefsCommon.GetActiveBirdId();

            FirebaseManager.LogPostScoreEvent(score, levelNumber, activeBirdId);

            switch (levelNumber)
            {
                case 11:
                    levelData.levelData.level11Complete = true;
                    levelData.levelData.level11TimesPlayed += 1;
                    if (score > levelData.levelData.level11HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level11HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level11HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 12:
                    levelData.levelData.level12Complete = true;
                    levelData.levelData.level12TimesPlayed += 1;
                    if (score > levelData.levelData.level12HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level12HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level12HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 13:
                    levelData.levelData.level13Complete = true;
                    levelData.levelData.level13TimesPlayed += 1;
                    if (score > levelData.levelData.level13HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level13HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level13HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 14:
                    levelData.levelData.level14Complete = true;
                    levelData.levelData.level14TimesPlayed += 1;
                    if (score > levelData.levelData.level14HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level14HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level14HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 15:
                    levelData.levelData.level15Complete = true;
                    levelData.levelData.level15TimesPlayed += 1;
                    if (score > levelData.levelData.level15HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level15HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level15HighScore);
                    }

                    // Can't have a "next" level unlocked because there are only 5 levels

                    break;
            }

            // Update high score here!
            levelData.SaveUserData();
        }
        else
        {
            int levelNumber = LevelManager.GetCurrentLevel();
            switch (levelNumber)
            {
                case 11:
                    commonScript.UpdateHighScore(levelData.levelData.level11HighScore);
                    break;

                case 12:
                    commonScript.UpdateHighScore(levelData.levelData.level12HighScore);
                    break;

                case 13:
                    commonScript.UpdateHighScore(levelData.levelData.level13HighScore);
                    break;

                case 14:
                    commonScript.UpdateHighScore(levelData.levelData.level14HighScore);
                    break;

                case 15:
                    commonScript.UpdateHighScore(levelData.levelData.level15HighScore);
                    break;
            }
        }

        // Save user data or new score
        commonScript.CleanupDone(nextLevelUnlocked);
    }

    private float GetObjectsBoundCenter(Renderer r)
    {
        return r.bounds.center.x;
    }

    private void UpdateGroundObjects()
    {
        Vector3 pos1 = groundObject1.transform.localPosition;
        pos1.x -= speed;
        groundObject1.transform.localPosition = pos1;
        float ground1MaxX = ground1Renderer.bounds.max.x;

        Vector3 pos2 = groundObject2.transform.localPosition;
        pos2.x -= speed ;
        groundObject2.transform.localPosition = pos2;
        float ground2MaxX = ground2Renderer.bounds.max.x;

        if (ground1MaxX < (-1 * cameraMaxX))
        {
            // Time to transition backgroundObject1
            pos1.x = GetObjectsBoundCenter(ground2Renderer) + groundObjectsDistance;
            groundObject1.transform.localPosition = pos1;
        }
        else if (ground2MaxX < (-1 * cameraMaxX))
        {
            // Time to transition backgroundObject2
            pos2.x = GetObjectsBoundCenter(ground1Renderer) + groundObjectsDistance;
            groundObject2.transform.localPosition = pos2;
        }
    }

    private void UpdateWaterObjects()
    {
        Vector3 pos1 = waterObject1.transform.localPosition;
        pos1.x -= speed * 0.67f;
        waterObject1.transform.localPosition = pos1;
        float water1MaxX = waterObject1Renderer.bounds.max.x;

        Vector3 pos2 = waterObject2.transform.localPosition;
        pos2.x -= speed * 0.67f;
        waterObject2.transform.localPosition = pos2;
        float water2MaxX = waterObject2Renderer.bounds.max.x;

        if (water1MaxX < (-1 * cameraMaxX))
        {
            pos1.x = GetObjectsBoundCenter(waterObject2Renderer) + waterObjectsDistance;
            waterObject1.transform.localPosition = pos1;
        }
        else if (water2MaxX < (-1 * cameraMaxX))
        {
            pos2.x = GetObjectsBoundCenter(waterObject1Renderer) + waterObjectsDistance;
            waterObject2.transform.localPosition = pos2;
        }
    }

    private void UpdateStormCloudObjects()
    {
        if (stormCloudCluster1.activeInHierarchy)
        {
            Vector3 pos1 = stormCloudCluster1.transform.localPosition;
            pos1.x -= speed;
            stormCloudCluster1.transform.localPosition = pos1;
            float cloud1Max = cloudCluster1Renderer.bounds.max.x;

            Vector3 pos2 = stormCloudCluster2.transform.localPosition;
            pos2.x -= speed;
            stormCloudCluster2.transform.localPosition = pos2;
            float cloud2Max = cloudCluster2Renderer.bounds.max.x;

            if (cloud1Max < (-1 * cameraMaxX))
            {
                pos1.x = GetObjectsBoundCenter(cloudCluster2Renderer) + cloudClusterObjectsDistance;
                stormCloudCluster1.transform.localPosition = pos1;
            }
            else if (cloud2Max < (-1 * cameraMaxX))
            {
                pos2.x = GetObjectsBoundCenter(cloudCluster1Renderer) + cloudClusterObjectsDistance;
                stormCloudCluster2.transform.localPosition = pos2;
            }
        }
    }

    private void Level11Prep()
    {
        // Daytime
        stormCloudCluster1.gameObject.SetActive(false);
        stormCloudCluster2.gameObject.SetActive(false);


        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.0f;
        float enemyBirdMaxChance = 1.0f;

        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);


        // Setup the display at the top to show collectible items for the level
        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);



        // Add collectibles to the level
        float bananaMinChance = 0.0f;
        float bananaMaxChance = 0.19f;

        float coinMinChance = 0.20f;
        float coinMaxChance = 1.0f;
        gameItemObjectScript.AddBananaCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, bananaMinChance, bananaMaxChance,
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }

    private void Level12Prep()
    {
        // Evening
        background.GetComponent<SpriteRenderer>().sprite = eveningSky;
        stormCloudCluster1.gameObject.SetActive(false);
        stormCloudCluster2.gameObject.SetActive(false);


        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.11f;
        float enemyBirdMaxChance = 1.0f;

        float palmTreeMinSpeed = speed;
        float palmTreeMaxSpeed = speed;
        float palmTreeMinChance = 0.00f;
        float palmTreeMaxChance = 0.10f;
        float palmTreeMinGenY = -7f;
        float palmTreeMaxGenY = -5.5f;

        gameItemObjectScript.AddPalmTreeObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, palmTreeMinSpeed, palmTreeMaxSpeed,
                                                 palmTreeMinChance, palmTreeMaxChance, palmTreeMinGenY, palmTreeMaxGenY, 0, 0);
        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);


        // Setup the display at the top to show collectible items for the level
        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);



        // Add collectibles to the level
        float bananaMinChance = 0.0f;
        float bananaMaxChance = 0.19f;

        float coinMinChance = 0.20f;
        float coinMaxChance = 1.0f;
        gameItemObjectScript.AddBananaCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, bananaMinChance, bananaMaxChance,
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }

    private void Level13Prep()
    {
        // Night
        background.GetComponent<SpriteRenderer>().sprite = nightSky;
        groundObject1.GetComponent<SpriteRenderer>().sprite = nightSand;
        groundObject2.GetComponent<SpriteRenderer>().sprite = nightSand;
        waterObject1.GetComponent<SpriteRenderer>().sprite = nightWater;
        waterObject2.GetComponent<SpriteRenderer>().sprite = nightWater;
        stormCloudCluster1.gameObject.SetActive(false);
        stormCloudCluster2.gameObject.SetActive(false);


        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.21f;
        float enemyBirdMaxChance = 1.0f;

        float palmTreeMinSpeed = speed;
        float palmTreeMaxSpeed = speed;
        float palmTreeMinChance = 0.00f;
        float palmTreeMaxChance = 0.10f;
        float palmTreeMinGenY = -7f;
        float palmTreeMaxGenY = -5.5f;

        float footballMinSpeed = 0.09f;
        float footballMaxSpeed = 0.10f;
        float footballMinChance = 0.11f;
        float footballMaxChance = 0.15f;
        float footballMinGenY = -2f;
        float footballMaxGenY = 2f;
        float footballMinMoveY = 5f;
        float footballMaxMoveY = 7f;

        float beachBallMinSpeed = 0.08f;
        float beachBallMaxSpeed = 0.09f;
        float beachBallMinChance = 0.16f;
        float beachBallMaxChance = 0.20f;
        float beachBallMinGenY = -2f;
        float beachBallMaxGenY = 1f;
        float beachBallMinMoveY = 3f;
        float beachBallMaxMoveY = 5f;

        gameItemObjectScript.AddPalmTreeObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, palmTreeMinSpeed, palmTreeMaxSpeed,
                                                 palmTreeMinChance, palmTreeMaxChance, palmTreeMinGenY, palmTreeMaxGenY, 0, 0);
        gameItemObjectScript.AddFootballObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, footballMinSpeed, footballMaxSpeed,
                                                 footballMinChance, footballMaxChance, footballMinGenY, footballMaxGenY,
                                                 footballMinMoveY, footballMaxMoveY);
        gameItemObjectScript.AddBeachBallObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, beachBallMinSpeed, beachBallMaxSpeed,
                                                  beachBallMinChance, beachBallMaxChance, beachBallMinGenY, beachBallMaxGenY,
                                                  beachBallMinMoveY, beachBallMaxMoveY);
        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);


        // Setup the display at the top to show collectible items for the level
        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(2, gameItemObjectScript.blueberriesPrefab.GetComponent<SpriteRenderer>().sprite);



        // Add collectibles to the level
        float bananaMinChance = 0.0f;
        float bananaMaxChance = 0.20f;

        float blueberryMinChance = 0.21f;
        float blueberryMaxChance = 0.30f;

        float coinMinChance = 0.31f;
        float coinMaxChance = 1.0f;
        gameItemObjectScript.AddBananaCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, bananaMinChance, bananaMaxChance,
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }

    private void Level14Prep()
    {
        // Dusk
        background.GetComponent<SpriteRenderer>().sprite = duskSky;
        groundObject1.GetComponent<SpriteRenderer>().sprite = nightSand;
        groundObject2.GetComponent<SpriteRenderer>().sprite = nightSand;
        waterObject1.GetComponent<SpriteRenderer>().sprite = nightWater;
        waterObject2.GetComponent<SpriteRenderer>().sprite = nightWater;
        stormCloudCluster1.gameObject.SetActive(false);
        stormCloudCluster2.gameObject.SetActive(false);


        // XXX Change collectibles to use the maxNumInLevel vs maxNumInScene
        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.26f;
        float enemyBirdMaxChance = 1.0f;

        float palmTreeMinSpeed = speed;
        float palmTreeMaxSpeed = speed;
        float palmTreeMinChance = 0.00f;
        float palmTreeMaxChance = 0.10f;
        float palmTreeMinGenY = -7f;
        float palmTreeMaxGenY = -5.5f;

        float footballMinSpeed = 0.09f;
        float footballMaxSpeed = 0.10f;
        float footballMinChance = 0.11f;
        float footballMaxChance = 0.15f;
        float footballMinGenY = -2f;
        float footballMaxGenY = 2f;
        float footballMinMoveY = 5f;
        float footballMaxMoveY = 7f;

        float beachBallMinSpeed = 0.08f;
        float beachBallMaxSpeed = 0.09f;
        float beachBallMinChance = 0.16f;
        float beachBallMaxChance = 0.20f;
        float beachBallMinGenY = -2f;
        float beachBallMaxGenY = 1f;
        float beachBallMinMoveY = 3f;
        float beachBallMaxMoveY = 5f;

        float propPlaneMinSpeed = 0.10f;
        float propPlaneMaxSpeed = 0.12f;
        float propPlaneMinChance = 0.21f;
        float propPlaneMaxChance = 0.25f;
        float propPlaneMinGenY = 3.5f;
        float propPlaneMaxGenY = 7.5f;
        float propPlaneMinMoveY = 0;
        float propPlaneMaxMoveY = 0;

        gameItemObjectScript.AddPalmTreeObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, palmTreeMinSpeed, palmTreeMaxSpeed,
                                                 palmTreeMinChance, palmTreeMaxChance, palmTreeMinGenY, palmTreeMaxGenY, 0, 0);
        gameItemObjectScript.AddFootballObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, footballMinSpeed, footballMaxSpeed,
                                                 footballMinChance, footballMaxChance, footballMinGenY, footballMaxGenY,
                                                 footballMinMoveY, footballMaxMoveY);
        gameItemObjectScript.AddBeachBallObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, beachBallMinSpeed, beachBallMaxSpeed,
                                                  beachBallMinChance, beachBallMaxChance, beachBallMinGenY, beachBallMaxGenY,
                                                  beachBallMinMoveY, beachBallMaxMoveY);
        gameItemObjectScript.AddPropPlaneObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, propPlaneMinSpeed, propPlaneMaxSpeed,
                                                  propPlaneMinChance, propPlaneMaxChance, propPlaneMinGenY, propPlaneMaxGenY,
                                                  propPlaneMinMoveY, propPlaneMaxMoveY);
        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);


        // Setup the display at the top to show collectible items for the level
        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(2, gameItemObjectScript.blueberriesPrefab.GetComponent<SpriteRenderer>().sprite);



        // Add collectibles to the level
        float bananaMinChance = 0.0f;
        float bananaMaxChance = 0.20f;

        float blueberryMinChance = 0.21f;
        float blueberryMaxChance = 0.30f;

        float coinMinChance = 0.31f;
        float coinMaxChance = 1.0f;
        gameItemObjectScript.AddBananaCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, bananaMinChance, bananaMaxChance,
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }

    private void Level15Prep()
    {
        // Stormy day
        background.GetComponent<SpriteRenderer>().sprite = stormSky;


        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.36f;
        float enemyBirdMaxChance = 1.0f;

        float palmTreeMinSpeed = speed;
        float palmTreeMaxSpeed = speed;
        float palmTreeMinChance = 0.00f;
        float palmTreeMaxChance = 0.10f;
        float palmTreeMinGenY = -7f;
        float palmTreeMaxGenY = -5.5f;

        float footballMinSpeed = 0.09f;
        float footballMaxSpeed = 0.10f;
        float footballMinChance = 0.11f;
        float footballMaxChance = 0.15f;
        float footballMinGenY = -2f;
        float footballMaxGenY = 2f;
        float footballMinMoveY = 5f;
        float footballMaxMoveY = 7f;

        float beachBallMinSpeed = 0.08f;
        float beachBallMaxSpeed = 0.09f;
        float beachBallMinChance = 0.16f;
        float beachBallMaxChance = 0.20f;
        float beachBallMinGenY = -2f;
        float beachBallMaxGenY = 1f;
        float beachBallMinMoveY = 3f;
        float beachBallMaxMoveY = 5f;

        float propPlaneMinSpeed = 0.10f;
        float propPlaneMaxSpeed = 0.12f;
        float propPlaneMinChance = 0.21f;
        float propPlaneMaxChance = 0.25f;
        float propPlaneMinGenY = 3.5f;
        float propPlaneMaxGenY = 7.5f;
        float propPlaneMinMoveY = 0;
        float propPlaneMaxMoveY = 0;

        float stormCloudMinSpeed = 0.05f;
        float stormCloudMaxSpeed = 0.06f;
        float stormCloudMinGenY = 5.5f;
        float stormCloudMaxGenY = 7.5f;
        float stormCloudMinMoveY = 0;
        float stormCloudMaxMoveY = 0;

        float stormCloud1MinChance = 0.26f;
        float stormCloud1MaxChance = 0.30f;
        float stormCloud2MinChance = 0.31f;
        float stormCloud2MaxChance = 0.35f;

        gameItemObjectScript.AddPalmTreeObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, palmTreeMinSpeed, palmTreeMaxSpeed,
                                                 palmTreeMinChance, palmTreeMaxChance, palmTreeMinGenY, palmTreeMaxGenY, 0, 0);
        gameItemObjectScript.AddFootballObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, footballMinSpeed, footballMaxSpeed,
                                                 footballMinChance, footballMaxChance, footballMinGenY, footballMaxGenY,
                                                 footballMinMoveY, footballMaxMoveY);
        gameItemObjectScript.AddBeachBallObstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, beachBallMinSpeed, beachBallMaxSpeed,
                                                  beachBallMinChance, beachBallMaxChance, beachBallMinGenY, beachBallMaxGenY,
                                                  beachBallMinMoveY, beachBallMaxMoveY);
        gameItemObjectScript.AddPropPlaneObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, propPlaneMinSpeed, propPlaneMaxSpeed,
                                                  propPlaneMinChance, propPlaneMaxChance, propPlaneMinGenY, propPlaneMaxGenY,
                                                  propPlaneMinMoveY, propPlaneMaxMoveY);
        gameItemObjectScript.AddStormCloud1Obstacle(1, 2, stormCloudMinSpeed, stormCloudMaxSpeed,
                                                    stormCloud1MinChance, stormCloud1MaxChance, stormCloudMinGenY, stormCloudMaxGenY,
                                                    stormCloudMinMoveY, stormCloudMaxMoveY);
        gameItemObjectScript.AddStormCloud2Obstacle(1, 2, stormCloudMinSpeed, stormCloudMaxSpeed,
                                                    stormCloud2MinChance, stormCloud2MaxChance, stormCloudMinGenY, stormCloudMaxGenY,
                                                    stormCloudMinMoveY, stormCloudMaxMoveY);
        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);


        // Setup the display at the top to show collectible items for the level
        if (false == inventoryData.IsBirdInInventory(InventoryData.STEVEN_ID))
        {
            generateEggItem = true;
            // Pick a random time to generate the egg;
            // DO THIS BEFORE WE SEED UNITYENGINE RANDOM!!!
            timeForEggGeneration = Random.Range(1.0f, levelTimeLength);

            commonScript.InitItemCanvasImage(7, gameItemObjectScript.eggPrefab.GetComponent<SpriteRenderer>().sprite);
        }

        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(2, gameItemObjectScript.blueberriesPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(3, gameItemObjectScript.strawberryPrefab.GetComponent<SpriteRenderer>().sprite);



        // Add collectibles to the level
        float bananaMinChance = 0.0f;
        float bananaMaxChance = 0.20f;

        float blueberryMinChance = 0.21f;
        float blueberryMaxChance = 0.30f;

        float strawberryMinChance = 0.31f;
        float strawberryMaxChance = 0.34f;

        float coinMinChance = 0.35f;
        float coinMaxChance = 1.0f;
        gameItemObjectScript.AddBananaCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, bananaMinChance, bananaMaxChance,
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddStrawberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, 1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                      strawberryMinChance, strawberryMaxChance, minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }
}
