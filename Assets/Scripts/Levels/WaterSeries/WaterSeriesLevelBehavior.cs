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
    public GameObject finishLinePrefab;

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
    public GameObject eggPrefab;
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

        // XXX Seed this with the level number!!!
        // Seed the random number generator here first
        Random.InitState(levelNumber);

        // Start coroutines for checking:
        // 1. Collisions
        // 2. Items collected
        // 3. Items that need to be removed
        StartCoroutine(CheckCollisions());
        StartCoroutine(CheckItemsCollected());
        // XXX REMOVE ME StartCoroutine(CheckItemsForRemoval());
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
                    if (generatedShark && false == sharkDone)
                    {
                        shark.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
                    }
                }
                else if (false == commonScript.disableColliders && collidersWereDisabled)
                {
                    // Enable collisions; they will be updated when object positions are updated
                    collidersWereDisabled = false;
                    gameItemObjectScript.EnableColliders();
                    if (generatedShark && false == sharkDone)
                    {
                        shark.gameObject.GetComponent<PolygonCollider2D>().enabled = true;
                    }
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

    /* XXX REMOVE ME
    IEnumerator CheckItemsForRemoval()
    {
        for (; ; )
        {
            for (int i = 0; i < gameObstacles.Length; ++i)
            {
                if (gameObstacles[i].used)
                {
                    Renderer r = gameObstacles[i].renderer;
                    if (r.bounds.max.x < (-1 * cameraMaxX))
                    {
                        GameObject obj = gameObstacles[i].gameObject;
                        gameItemObjectScript.RemoveObstacleFromGame(obj);
                        // XXX REMOVE ME gameItemObjectScript.RemoveObjectFromGame(gameObstacles, obj);
                    }
                }
            }

            for (int i = 0; i < gameCollectibles.Length; ++i)
            {
                if (gameCollectibles[i].used)
                {
                    Renderer r = gameCollectibles[i].renderer;
                    if (r.bounds.max.x < (-1 * cameraMaxX))
                    {
                        GameObject obj = gameCollectibles[i].gameObject;
                        gameItemObjectScript.RemoveCollectibleFromGame(obj);
                        // XXX REMOVE ME gameItemObjectScript.RemoveObjectFromGame(gameCollectibles, obj);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    */

    #endregion

    // Update is called once per frame
    void Update()
    {
        switch (commonScript.GetGameState())
        {
            case CommonBehavior.GameState.Active:
                if (0f == cameraMaxX)
                {
                    // Figure out the best place to start generating pipes off screen
                    // It should be shortly off camera on the right side before becoming visible in the camera
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

                    gameItemObjectScript.InitGameObjectManager(cameraMaxX, backgroundMinY, backgroundMaxY, NUM_OBSTACLES, NUM_COLLECTIBLES);
                }

                // Update object positions in the game
                // XXX UpdateCloudObjects();
                gameItemObjectScript.UpdateGameObstaclePositions();
                gameItemObjectScript.UpdateGameCollectiblePositions();
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

                if (LevelManager.GetCurrentLevel() == 10)
                {
                    // Generate the shark if it's time
                    if (levelTimer >= sharkGenerateTime && false == generatedShark)
                    {
                        shark = Instantiate(sharkPrefab);
                        Renderer _rend = shark.GetComponent<Renderer>();
                        float objWidth = _rend.bounds.size.x / 2;
                        Vector3 pos = shark.transform.localPosition;
                        pos.x = cameraMaxX + objWidth;
                        shark.transform.localPosition = pos;

                        // Rotate the shark 
                        shark.transform.rotation = new Quaternion(0, 0, -0.35f, 1.0f);

                        sharkGoingUp = true;
                        generatedShark = true;
                    }

                    // Move the shark if it's been generated
                    if (generatedShark && false == sharkDone)
                    {
                        Vector3 pos = shark.transform.localPosition;
                        pos.x -= SHARK_X_DELTA;
                        if (sharkGoingUp)
                        {
                            pos.y += SHARK_Y_DELTA;
                            if (pos.y >= SHARK_MAX_HEIGHT)
                            {
                                sharkGoingUp = false;
                            }
                        }
                        else
                        {
                            pos.y -= SHARK_Y_DELTA;
                        }
                        shark.transform.localPosition = pos;

                        Renderer _rend = shark.GetComponent<Renderer>();
                        float maxX = _rend.bounds.max.x;
                        if (maxX < -cameraMaxX)
                        {
                            // Remove the shark from the game
                            Destroy(shark);
                            sharkDone = true;
                        }
                    }
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
                GameOver();
                break;

            case CommonBehavior.GameState.Win:
                GameOver(success: true);
                break;

            case CommonBehavior.GameState.Wait:
                // Don't do anything
                break;

            case CommonBehavior.GameState.Done:
                // Keep objects moving after user wins
                if (userWon)
                {
                    // Ensure all items continue moving after game is over
                    // It looks better than everything just stopping
                    // XXX UpdateCloudObjects();
                    gameItemObjectScript.UpdateGameObstaclePositions();
                    gameItemObjectScript.UpdateGameCollectiblePositions();
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
                case 1:
                    commonScript.UpdateHighScore(levelData.levelData.level6HighScore);
                    break;

                case 2:
                    commonScript.UpdateHighScore(levelData.levelData.level7HighScore);
                    break;

                case 3:
                    commonScript.UpdateHighScore(levelData.levelData.level8HighScore);
                    break;

                case 4:
                    commonScript.UpdateHighScore(levelData.levelData.level9HighScore);
                    break;

                case 5:
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
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        gameItemObjectScript.AddEnemyBirdObstacle(-1, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.0f, 1.0f);

        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        gameItemObjectScript.AddBananaCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                  0.0f, 0.19f);
        gameItemObjectScript.AddCoinCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                0.20f, 1.0f);
    }

    private void Level7Prep()
    {
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;

        gameItemObjectScript.AddWaveObstacle(1, waveMinSpeed, waveMaxSpeed, 0.00f, 0.10f);
        gameItemObjectScript.AddEnemyBirdObstacle(-1, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.11f, 1.0f);

        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        gameItemObjectScript.AddBananaCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                  0.0f, 0.19f);
        gameItemObjectScript.AddCoinCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                0.20f, 1.0f);
    }

    private void Level8Prep()
    {
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;

        gameItemObjectScript.AddWaveObstacle(1, waveMinSpeed, waveMaxSpeed, 0.00f, 0.10f);
        gameItemObjectScript.AddFish1Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.11f, 0.15f);
        gameItemObjectScript.AddFish3Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.16f, 0.20f);
        gameItemObjectScript.AddEnemyBirdObstacle(-1, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.21f, 1.0f);

        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(2, gameItemObjectScript.blueberriesPrefab.GetComponent<SpriteRenderer>().sprite);
        gameItemObjectScript.AddBananaCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                  0.0f, 0.20f);
        gameItemObjectScript.AddBlueberryCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                     0.21f, 0.30f);
        gameItemObjectScript.AddCoinCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                0.31f, 1.0f);
    }

    private void Level9Prep()
    {
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;

        float fish2MinSpeed = 0.060f;
        float fish2MaxSpeed = 0.065f;

        float boatMinSpeed = 0.06f;
        float boatMaxSpeed = 0.07f;

        gameItemObjectScript.AddWaveObstacle(1, waveMinSpeed, waveMaxSpeed, 0.00f, 0.10f);
        gameItemObjectScript.AddFish1Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.11f, 0.15f);
        gameItemObjectScript.AddFish2Obstacle(1, fish2MinSpeed, fish2MaxSpeed, 0.16f, 0.20f);
        gameItemObjectScript.AddFish3Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.21f, 0.25f);
        gameItemObjectScript.AddBoat1Obstacle(1, boatMinSpeed, boatMaxSpeed, 0.26f, 0.30f);
        gameItemObjectScript.AddEnemyBirdObstacle(-1, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.31f, 1.0f);

        commonScript.InitItemCanvasImage(0, gameItemObjectScript.coinPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(1, gameItemObjectScript.bananaPrefab.GetComponent<SpriteRenderer>().sprite);
        commonScript.InitItemCanvasImage(2, gameItemObjectScript.blueberriesPrefab.GetComponent<SpriteRenderer>().sprite);
        gameItemObjectScript.AddBananaCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                          0.0f, 0.20f);
        gameItemObjectScript.AddBlueberryCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                     0.21f, 0.30f);
        gameItemObjectScript.AddCoinCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                0.31f, 1.0f);
    }

    private void Level10Prep()
    {
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float waveMinSpeed = 0.07f;
        float waveMaxSpeed = 0.08f;

        float fish1MinSpeed = 0.055f;
        float fish1MaxSpeed = 0.058f;

        float fish2MinSpeed = 0.060f;
        float fish2MaxSpeed = 0.065f;

        float boatMinSpeed = 0.06f;
        float boatMaxSpeed = 0.07f;

        gameItemObjectScript.AddWaveObstacle(1, waveMinSpeed, waveMaxSpeed, 0.00f, 0.10f);
        gameItemObjectScript.AddFish1Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.11f, 0.15f);
        gameItemObjectScript.AddFish2Obstacle(1, fish2MinSpeed, fish2MaxSpeed, 0.16f, 0.20f);
        gameItemObjectScript.AddFish3Obstacle(1, fish1MinSpeed, fish1MaxSpeed, 0.21f, 0.25f);
        gameItemObjectScript.AddBoat1Obstacle(1, boatMinSpeed, boatMaxSpeed, 0.26f, 0.30f);
        gameItemObjectScript.AddEnemyBirdObstacle(-1, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.31f, 1.0f);

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
        gameItemObjectScript.AddBananaCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                          0.0f, 0.20f);
        gameItemObjectScript.AddBlueberryCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                     0.21f, 0.30f);
        gameItemObjectScript.AddStrawberryCollectible(1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                      0.31f, 0.34f);
        gameItemObjectScript.AddCoinCollectible(-1, GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED,
                                                0.35f, 1.0f);
    }
}
