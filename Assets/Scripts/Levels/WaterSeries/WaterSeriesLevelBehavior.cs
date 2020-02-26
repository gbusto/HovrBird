using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterSeriesLevelBehavior : MonoBehaviour
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

    private int collectibleGenerationCounter = collectibleGenerationTurns;
    private const int collectibleGenerationTurns = 37;

    private int obstacleGenerationCounter = obstacleGenerationTurns;
    private const int obstacleGenerationTurns = 14;

    // NOTE: NOT common level attributes
    // Collectible prefabs
    // XXX Add them here (maybe new ones?)

    // NOTE: Could be common level attributes
    // Cloud prefabs; just objects to give the illusion that the level is moving
    // to the left
    public GameObject cloud1Prefab;
    public GameObject cloud2Prefab;
    public GameObject cloud3Prefab;
    public GameObject cloud4Prefab;

    private bool sharkDone;
    private bool generatedShark;
    private bool sharkGoingUp;
    private float sharkGenerateTime = levelTimeLength / 3;
    public GameObject sharkPrefab;
    private GameObject shark;
    private const float SHARK_X_DELTA = 0.1f;
    private const float SHARK_Y_DELTA = 0.06f;
    private const float SHARK_MAX_HEIGHT = -8.0f;

    // NOTE: Common level attributes
    public GameObject groundObject1;
    private Renderer ground1Renderer;
    public GameObject groundObject2;
    private Renderer ground2Renderer;
    private float groundObjectsDistance;

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

    private const int NUM_OBSTACLES = 15;
    private const int NUM_COLLECTIBLES = 7;

    // NOTE: Common level attributes
    private LevelData levelData;
    private InventoryData inventoryData;
    private System.Random sysRandom;

    // NOTE: Anything pertaining to generating game obstacles will be level-specific


    private void Awake()
    {
        // NOTE: Common level initialization
        // DO NOT SEED THIS RANDOM
        sysRandom = new System.Random();

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

        Physics2D.IgnoreLayerCollision(8, 9);

        if (Screen.width > Screen.height)
        {
            // Calculate the offscreen startpoint when the screen is wider than it is tall
            cameraMaxX = (Camera.main.orthographicSize * 2.0f) * (Screen.width / Screen.height);
        }
        else if (Screen.height > Screen.width)
        {
            // Calculate the offscreen startpoint when the screen is taller than it is wide (phones or portrait iPad)
            cameraMaxX = (Camera.main.orthographicSize);
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

        Renderer backgroundRenderer = background.GetComponent<Renderer>();
        // This minimum value is the minimum Y value at which objects should be generated
        backgroundMinY = backgroundRenderer.bounds.min.y + backgroundRenderer.bounds.size.y * 0.12f;
        backgroundMaxY = backgroundRenderer.bounds.max.y * 0.95f;
        groundMaxY = groundObject1.transform.position.y + ground1Renderer.bounds.size.y;

        gameItemObjectScript.InitGameObjectManager(cameraMaxX, backgroundMinY, backgroundMaxY, NUM_OBSTACLES, NUM_COLLECTIBLES);

        // Switch level prep based on level number
        int levelNumber = LevelManager.GetCurrentLevel();
        switch (levelNumber)
        {
            case 6:
                Level6Prep();
                break;

            case 7:
                Level7Prep();
                break;

            case 8:
                Level8Prep();
                break;

            case 9:
                Level9Prep();
                break;

            case 10:
                Level10Prep();
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
                inventoryData.AddEggToInventory(InventoryData.NIGEL_ID);
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

            // XXX FirebaseManager.LogPostScoreEvent(score, levelNumber, activeBirdId);

            switch (levelNumber)
            {
                case 6:
                    levelData.levelData.level6Complete = true;
                    levelData.levelData.level6TimesPlayed += 1;
                    if (score > levelData.levelData.level6HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level6HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level6HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 7:
                    levelData.levelData.level7Complete = true;
                    levelData.levelData.level7TimesPlayed += 1;
                    if (score > levelData.levelData.level7HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level7HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level7HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 8:
                    levelData.levelData.level8Complete = true;
                    levelData.levelData.level8TimesPlayed += 1;
                    if (score > levelData.levelData.level8HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level8HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level8HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 9:
                    levelData.levelData.level9Complete = true;
                    levelData.levelData.level9TimesPlayed += 1;
                    if (score > levelData.levelData.level9HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level9HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level9HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 10:
                    levelData.levelData.level10Complete = true;
                    levelData.levelData.level10TimesPlayed += 1;
                    if (score > levelData.levelData.level10HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level10HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level10HighScore);
                    }

                    nextLevelUnlocked = true;

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
                case 6:
                    commonScript.UpdateHighScore(levelData.levelData.level6HighScore);
                    break;

                case 7:
                    commonScript.UpdateHighScore(levelData.levelData.level7HighScore);
                    break;

                case 8:
                    commonScript.UpdateHighScore(levelData.levelData.level8HighScore);
                    break;

                case 9:
                    commonScript.UpdateHighScore(levelData.levelData.level9HighScore);
                    break;

                case 10:
                    commonScript.UpdateHighScore(levelData.levelData.level10HighScore);
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
        pos1.x -= speed * 0.67f;
        groundObject1.transform.localPosition = pos1;
        float ground1MaxX = ground1Renderer.bounds.max.x;

        Vector3 pos2 = groundObject2.transform.localPosition;
        pos2.x -= speed * 0.67f;
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


    private void Level6Prep()
    {
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
                                                  backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                backgroundMinY, backgroundMaxY);
    }

    private void Level7Prep()
    {
        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.11f;
        float enemyBirdMaxChance = 1.0f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;
        float waveMinChance = 0.00f;
        float waveMaxChance = 0.10f;
        float waveMinGenY = -7f;
        float waveMaxGenY = -5.5f;

        gameItemObjectScript.AddWaveObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, waveMinSpeed, waveMaxSpeed,
                                             waveMinChance, waveMaxChance, waveMinGenY, waveMaxGenY, 0, 0);
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
                                                  backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                backgroundMinY, backgroundMaxY);
    }

    private void Level8Prep()
    {
        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.21f;
        float enemyBirdMaxChance = 1.0f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;
        float waveMinChance = 0.00f;
        float waveMaxChance = 0.10f;
        float waveMinGenY = -7f;
        float waveMaxGenY = -5.5f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;
        float fish1MinChance = 0.11f;
        float fish1MaxChance = 0.15f;
        float fish1MinGenY = -7f;
        float fish1MaxGenY = -7f;
        float fish1MinMoveY = -4f;
        float fish1MaxMoveY = -2f;

        float fish3MinSpeed = 0.058f;
        float fish3MaxSpeed = 0.061f;
        float fish3MinChance = 0.16f;
        float fish3MaxChance = 0.20f;
        float fish3MinGenY = -7f;
        float fish3MaxGenY = -7f;
        float fish3MinMoveY = -3.5f;
        float fish3MaxMoveY = -1.5f;

        gameItemObjectScript.AddWaveObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, waveMinSpeed, waveMaxSpeed,
                                             waveMinChance, waveMaxChance, waveMinGenY, waveMaxGenY, 0, 0);
        gameItemObjectScript.AddFish1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish1MinSpeed, fish1MaxSpeed,
                                              fish1MinChance, fish1MaxChance, fish1MinGenY, fish1MaxGenY,
                                              fish1MinMoveY, fish1MaxMoveY);
        gameItemObjectScript.AddFish3Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish3MinSpeed, fish3MaxSpeed,
                                              fish3MinChance, fish3MaxChance, fish3MinGenY, fish3MaxGenY,
                                              fish3MinMoveY, fish3MaxMoveY);
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
                                                  backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                backgroundMinY, backgroundMaxY);
    }

    private void Level9Prep()
    {
        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.26f;
        float enemyBirdMaxChance = 1.0f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;
        float waveMinChance = 0.00f;
        float waveMaxChance = 0.10f;
        float waveMinGenY = -7f;
        float waveMaxGenY = -5.5f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;
        float fish1MinChance = 0.11f;
        float fish1MaxChance = 0.15f;
        float fish1MinGenY = -7f;
        float fish1MaxGenY = -7f;
        float fish1MinMoveY = -4f;
        float fish1MaxMoveY = -2f;

        float fish3MinSpeed = 0.058f;
        float fish3MaxSpeed = 0.061f;
        float fish3MinChance = 0.16f;
        float fish3MaxChance = 0.20f;
        float fish3MinGenY = -7f;
        float fish3MaxGenY = -7f;
        float fish3MinMoveY = -3.5f;
        float fish3MaxMoveY = -1.5f;

        float boat1MinSpeed = 0.06f;
        float boat1MaxSpeed = 0.07f;
        float boat1MinChance = 0.21f;
        float boat1MaxChance = 0.25f;
        float boat1MinGenY = -4f;
        float boat1MaxGenY = -4f;
        float boat1MinMoveY = -4f;
        float boat1MaxMoveY = -4f;

        gameItemObjectScript.AddWaveObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, waveMinSpeed, waveMaxSpeed,
                                             waveMinChance, waveMaxChance, waveMinGenY, waveMaxGenY, 0, 0);
        gameItemObjectScript.AddFish1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish1MinSpeed, fish1MaxSpeed,
                                              fish1MinChance, fish1MaxChance, fish1MinGenY, fish1MaxGenY,
                                              fish1MinMoveY, fish1MaxMoveY);
        gameItemObjectScript.AddFish3Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish3MinSpeed, fish3MaxSpeed,
                                              fish3MinChance, fish3MaxChance, fish3MinGenY, fish3MaxGenY,
                                              fish3MinMoveY, fish3MaxMoveY);
        gameItemObjectScript.AddBoat1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, boat1MinSpeed, boat1MaxSpeed,
                                              boat1MinChance, boat1MaxChance, boat1MinGenY, boat1MaxGenY,
                                              boat1MinMoveY, boat1MaxMoveY);
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
                                                  backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                backgroundMinY, backgroundMaxY);
    }

    private void Level10Prep()
    {
        // Add obstacles to the level
        float sharkMinSpeed = 0.08f;
        float sharkMaxSpeed = 0.10f;
        float sharkMinChance = 0.26f;
        float sharkMaxChance = 0.27f;
        float sharkMinGenY = -18f;
        float sharkMaxGenY = -18f;
        float sharkMinMoveY = -7.5f;
        float sharkMaxMoveY = -7.5f;

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.28f;
        float enemyBirdMaxChance = 1.0f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;
        float waveMinChance = 0.00f;
        float waveMaxChance = 0.10f;
        float waveMinGenY = -7f;
        float waveMaxGenY = -5.5f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;
        float fish1MinChance = 0.11f;
        float fish1MaxChance = 0.15f;
        float fish1MinGenY = -7f;
        float fish1MaxGenY = -7f;
        float fish1MinMoveY = -4f;
        float fish1MaxMoveY = -2f;

        float fish3MinSpeed = 0.058f;
        float fish3MaxSpeed = 0.061f;
        float fish3MinChance = 0.16f;
        float fish3MaxChance = 0.20f;
        float fish3MinGenY = -7f;
        float fish3MaxGenY = -7f;
        float fish3MinMoveY = -3.5f;
        float fish3MaxMoveY = -1.5f;

        float boat1MinSpeed = 0.06f;
        float boat1MaxSpeed = 0.07f;
        float boat1MinChance = 0.21f;
        float boat1MaxChance = 0.25f;
        float boat1MinGenY = -4f;
        float boat1MaxGenY = -4f;
        float boat1MinMoveY = -4f;
        float boat1MaxMoveY = -4f;

        gameItemObjectScript.AddShark1Obstacle(1, 2, sharkMinSpeed, sharkMaxSpeed, sharkMinChance, sharkMaxChance,
                                               sharkMinGenY, sharkMaxGenY, sharkMinMoveY, sharkMaxMoveY);
        gameItemObjectScript.AddWaveObstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, waveMinSpeed, waveMaxSpeed,
                                             waveMinChance, waveMaxChance, waveMinGenY, waveMaxGenY, 0, 0);
        gameItemObjectScript.AddFish1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish1MinSpeed, fish1MaxSpeed,
                                              fish1MinChance, fish1MaxChance, fish1MinGenY, fish1MaxGenY,
                                              fish1MinMoveY, fish1MaxMoveY);
        gameItemObjectScript.AddFish3Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, fish3MinSpeed, fish3MaxSpeed,
                                              fish3MinChance, fish3MaxChance, fish3MinGenY, fish3MaxGenY,
                                              fish3MinMoveY, fish3MaxMoveY);
        gameItemObjectScript.AddBoat1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL, boat1MinSpeed, boat1MaxSpeed,
                                              boat1MinChance, boat1MaxChance, boat1MinGenY, boat1MaxGenY,
                                              boat1MinMoveY, boat1MaxMoveY);
        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);

        if (false == inventoryData.IsBirdInInventory(InventoryData.NIGEL_ID))
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
                                                  backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddBlueberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                     GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, blueberryMinChance, blueberryMaxChance,
                                                     backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddStrawberryCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, 1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                      strawberryMinChance, strawberryMaxChance, backgroundMinY, backgroundMaxY);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                backgroundMinY, backgroundMaxY);
    }
}
