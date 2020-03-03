using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public struct CollectiblePrefab
{
    public GameObject prefab;
    public float probability;
}

public struct Slot
{
    public float yPosition;
    public float chance;
}


public class Level1Behavior : MonoBehaviour
{
    public GameObject commonPrefab;
    private GameObject commonObject;
    private CommonBehavior commonScript;

    public GameObject progressLinePrefab;
    private const float progress25 = 0.25f;
    private bool showedProgress25;
    private const float progress50 = 0.50f;
    private bool showedProgress50;
    private const float progress75 = 0.75f;
    private bool showedProgress75;

    public GameObject gameItemObject;
    private GameItemsBehavior gameItemObjectScript;

    public GameObject gameCamera;
    public GameObject background;
    public GameObject finishLinePrefab;

    public Sprite sunsetSkySprite;
    public Sprite sunsetGroundCloudsSprite;

    public Sprite nightSkySprite;
    public Sprite nightGroundCloudsSprite;

    // Obstacle prefabs
    public GameObject blimp1Prefab;
    public GameObject blimp2Prefab;
    public GameObject plane1Prefab;
    public GameObject plane2Prefab;
    public GameObject parachute1Prefab;
    public GameObject parachute2Prefab;
    public GameObject parachute3Prefab;
    public GameObject balloon1Prefab;
    public GameObject balloon2Prefab;
    public GameObject balloon3Prefab;
    public GameObject enemyBirdPrefab;

    // Collectible prefabs
    public GameObject strawberryPrefab;
    public GameObject blueberryPrefab;
    public GameObject bananaPrefab;
    public GameObject coinPrefab;

    // Cloud prefabs; just objects to give the illusion that the level is moving
    // to the left
    public GameObject cloud1Prefab;
    public GameObject cloud2Prefab;
    public GameObject cloud3Prefab;
    public GameObject cloud4Prefab;

    public GameObject groundObject1;
    private Renderer ground1Renderer;
    public GameObject groundObject2;
    private Renderer ground2Renderer;
    private float groundObjectsDistance;

    public Sprite collectibleItem1Sprite;
    public Sprite collectibleItem2Sprite;
    public Sprite collectibleItem3Sprite;
    public Sprite collectibleItem4Sprite;
    public Sprite eggSprite;
    public GameObject eggPrefab;
    private int item1Count;
    private int item2Count;
    private int item3Count;
    private int item4Count;

    private GameObject bird;
    private CameraBehavior cameraScript;

    private bool userWon;

    private int collectibleGenerationCounter;
    private static readonly int collectibleGenerationTurns = 37;
    // The previous index used for generating a collectible

    private int obstacleGenerationCounter = obstacleGenerationTurns;
    private static readonly int obstacleGenerationTurns = 14;
    // The previous index used for generating an obstacle

    private static readonly float speed = 0.04f;

    private float cameraMaxX;
    private float backgroundMinY;
    private float backgroundMaxY;
    private float groundMaxY;

    private bool collidersWereDisabled;

    private bool generateItems = true;
    private float levelTimer;
    private readonly float levelTimeLength = 60.0f;

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


    List<(GameObject, float)> cloudPrefabs;
    List<(GameObject, Renderer)> cloudObjects;

    List<ObstaclePrefab> obstaclePrefabs;

    private const int NUM_OBSTACLES = 15;
    private const int NUM_COLLECTIBLES = 7;

    private LevelData levelData;
    private InventoryData inventoryData;

    void Awake()
    {
        cloudPrefabs = new List<(GameObject, float)>();
        cloudObjects = new List<(GameObject, Renderer)>();

        Screen.orientation = ScreenOrientation.Landscape;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        levelData = LevelData.Instance();
        inventoryData = InventoryData.Instance();
    }

    // Start is called before the first frame update
    void Start()
    {
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

        cloudPrefabs.Add((cloud1Prefab, 0.10f));
        cloudPrefabs.Add((cloud2Prefab, 0.10f));
        cloudPrefabs.Add((cloud3Prefab, 0.10f));
        cloudPrefabs.Add((cloud4Prefab, 0.10f));

        ground1Renderer = groundObject1.GetComponent<Renderer>();
        ground2Renderer = groundObject2.GetComponent<Renderer>();
        groundObjectsDistance = groundObject2.transform.localPosition.x - groundObject1.transform.localPosition.x;

        Renderer backgroundRenderer = background.GetComponent<Renderer>();
        // This minimum value is the minimum Y value at which objects should be generated
        backgroundMinY = ground1Renderer.bounds.max.y + 2;
        backgroundMaxY = backgroundRenderer.bounds.max.y - 2;
        groundMaxY = groundObject1.transform.position.y + ground1Renderer.bounds.size.y;

        minCollectibleHeight = backgroundMinY;
        maxCollectibleHeight = backgroundMaxY;

        gameItemObjectScript.InitGameObjectManager(cameraMaxX, backgroundMinY, backgroundMaxY, NUM_OBSTACLES, NUM_COLLECTIBLES);

        int levelNumber = LevelManager.GetCurrentLevel();
        switch (levelNumber)
        {
            case 1:
                Level1Prep();
                break;

            case 2:
                Level2Prep();
                break;

            case 3:
                Level3Prep();
                break;

            case 4:
                Level4Prep();
                break;

            case 5:
                Level5Prep();
                break;
        }

        // Seed the random number generator here first
        Random.InitState(LevelManager.GetCurrentLevel());

        InitializeFirstClouds();

        // Start any continuous coroutine functions here
        StartCoroutine(CheckCollisions());
        StartCoroutine(CheckItemsCollected());
    }

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
                UpdateCloudObjects();
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
                    UpdateCloudObjects();
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
                    // Update the counters for obstacles and clouds
                    if (obstacleGenerationCounter == obstacleGenerationTurns)
                    {
                        obstacleGenerationCounter = 0;
                        // These InstantiateX functions could be called in a coroutine
                        InstantiateClouds();
                        gameItemObjectScript.GenerateNewObstacle(collidersWereDisabled);
                    }
                    obstacleGenerationCounter += 1;

                    // Update the counters for collectibles
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
                    collectibleGenerationCounter += 1;
                }
                break;
        }
    }



    #region EnumeratorFunctions
    IEnumerator CheckCollisions()
    {
        for (; ;)
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
        for (; ;)
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
                inventoryData.AddEggToInventory(InventoryData.SAM_ID);
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
                case 1:
                    levelData.levelData.level1Complete = true;
                    levelData.levelData.level1TimesPlayed += 1;
                    if (score > levelData.levelData.level1HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level1HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level1HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 2:
                    levelData.levelData.level2Complete = true;
                    levelData.levelData.level2TimesPlayed += 1;
                    if (score > levelData.levelData.level2HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level2HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level2HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 3:
                    levelData.levelData.level3Complete = true;
                    levelData.levelData.level3TimesPlayed += 1;
                    if (score > levelData.levelData.level3HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level3HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level3HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 4:
                    levelData.levelData.level4Complete = true;
                    levelData.levelData.level4TimesPlayed += 1;
                    if (score > levelData.levelData.level4HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level4HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level4HighScore);
                    }

                    nextLevelUnlocked = true;

                    break;

                case 5:
                    levelData.levelData.level5Complete = true;
                    levelData.levelData.level5TimesPlayed += 1;
                    if (score > levelData.levelData.level5HighScore)
                    {
                        // User beat their high score for level!
                        levelData.levelData.level5HighScore = score;
                        commonScript.UpdateHighScore(score, true);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level5HighScore);
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
                case 1:
                    commonScript.UpdateHighScore(levelData.levelData.level1HighScore);
                    break;

                case 2:
                    commonScript.UpdateHighScore(levelData.levelData.level2HighScore);
                    break;

                case 3:
                    commonScript.UpdateHighScore(levelData.levelData.level3HighScore);
                    break;

                case 4:
                    commonScript.UpdateHighScore(levelData.levelData.level4HighScore);
                    break;

                case 5:
                    commonScript.UpdateHighScore(levelData.levelData.level5HighScore);
                    break;
            }
        }

        // Save user data or new score
        commonScript.CleanupDone(nextLevelUnlocked);
    }

    private void InitializeFirstClouds()
    {
        int numClouds = Random.Range(2, 5);
        for (int i = 0; i < numClouds; ++i)
        {
            int cloudPrefabNumber = Random.Range(0, cloudPrefabs.Count);
            GameObject prefab = cloudPrefabs[cloudPrefabNumber].Item1;
            GameObject cloud = Instantiate(prefab);
            Renderer r = cloud.GetComponent<Renderer>();
            float cloudWidth = r.bounds.size.x;
            float randomX = Random.Range((-1 * cameraMaxX) + cloudWidth, cameraMaxX - cloudWidth);
            float randomY = Random.Range(backgroundMinY, backgroundMaxY);

            Vector3 pos = cloud.transform.localPosition;
            pos.x = randomX;
            pos.y = randomY;
            cloud.transform.localPosition = pos;
            cloudObjects.Add((cloud, r));
        }
    }

    private void InstantiateClouds()
    {
        List<GameObject> objectsToCreate = new List<GameObject>();
        if (cloudObjects.Count > 4)
        {
            return;
        }

        foreach ((GameObject, float) thing in cloudPrefabs)
        {
            GameObject prefab = thing.Item1;
            float chance = thing.Item2;

            if (Random.Range(0.0f, 1.0f) < chance)
            {
                objectsToCreate.Add(prefab);
            }

            if (cloudObjects.Count + objectsToCreate.Count > 4)
            {
                break;
            }
        }

        foreach (GameObject prefab in objectsToCreate)
        {
            GameObject obj = Instantiate(prefab);
            Renderer r = obj.GetComponent<Renderer>();
            float objHeight = r.bounds.size.y;
            float objWidth = r.bounds.size.x;
            float randomY = Random.Range(backgroundMinY, backgroundMaxY);
            float adjustedX = cameraMaxX + (objWidth / 2);
            Vector3 pos = obj.transform.localPosition;
            pos.x = adjustedX;
            pos.y = randomY;
            obj.transform.localPosition = pos;

            cloudObjects.Add((obj, r));
        }
    }

    private void UpdateCloudObjects()
    {
        List<(GameObject, Renderer)> toRemove = new List<(GameObject, Renderer)>();

        foreach ((GameObject, Renderer) thing in cloudObjects)
        {
            GameObject obj = thing.Item1;
            Renderer r = thing.Item2;
            Vector3 pos = obj.transform.localPosition;
            pos.x -= speed;
            obj.transform.localPosition = pos;
            if (r.bounds.max.x < (-1 * cameraMaxX))
            {
                toRemove.Add(thing);
            }
        }

        foreach ((GameObject, Renderer) thing in toRemove)
        {
            cloudObjects.Remove(thing);
            Destroy(thing.Item1);
        }
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

    private void Level1Prep()
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
                                                  minCollectibleHeight, maxCollectibleHeight);
        gameItemObjectScript.AddCoinCollectible(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                GameItemsBehavior.BASE_SPEED, GameItemsBehavior.BASE_SPEED, coinMinChance, coinMaxChance,
                                                minCollectibleHeight, maxCollectibleHeight);
    }

    private void Level2Prep()
    {
        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.41f;
        float enemyBirdMaxChance = 1.0f;

        float blimp1MinSpeed = 0.055f;
        float blimp1MaxSpeed = 0.065f;
        float blimp1MinChance = 0.0f;
        float blimp1MaxChance = 0.20f;

        float blimp2MinSpeed = 0.060f;
        float blimp2MaxSpeed = 0.070f;
        float blimp2MinChance = 0.21f;
        float blimp2MaxChance = 0.40f;

        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp1MinSpeed, blimp1MaxSpeed, blimp1MinChance, blimp1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp2Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                       blimp2MinSpeed, blimp2MaxSpeed, blimp2MinChance, blimp2MaxChance,
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

    private void Level3Prep()
    {
        background.GetComponent<SpriteRenderer>().sprite = sunsetSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;

        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.67f;
        float enemyBirdMaxChance = 1.0f;

        float blimp1MinSpeed = 0.055f;
        float blimp1MaxSpeed = 0.065f;
        float blimp1MinChance = 0.0f;
        float blimp1MaxChance = 0.16f;

        float blimp2MinSpeed = 0.060f;
        float blimp2MaxSpeed = 0.070f;
        float blimp2MinChance = 0.17f;
        float blimp2MaxChance = 0.33f;

        float parachuteMinSpeed = GameItemsBehavior.BASE_SPEED;
        float parachuteMaxSpeed = 0.045f;
        float parachuteMinGenY = 0f;
        float parachuteMaxGenY = backgroundMaxY;

        float parachute1MinChance = 0.34f;
        float parachute1MaxChance = 0.44f;

        float parachute2MinChance = 0.45f;
        float parachute2MaxChance = 0.55f;

        float parachute3MinChance = 0.56f;
        float parachute3MaxChance = 0.66f;

        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp1MinSpeed, blimp1MaxSpeed, blimp1MinChance, blimp1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp2MinSpeed, blimp2MaxSpeed, blimp2MinChance, blimp2MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddParachute1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute1MinChance, parachute1MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute2Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                           parachuteMinSpeed, parachuteMaxSpeed, parachute2MinChance, parachute2MaxChance,
                                           parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute3Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute3MinChance, parachute3MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);


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

    private void Level4Prep()
    {
        background.GetComponent<SpriteRenderer>().sprite = sunsetSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;

        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.79f;
        float enemyBirdMaxChance = 1.0f;

        float blimp1MinSpeed = 0.055f;
        float blimp1MaxSpeed = 0.065f;
        float blimp1MinChance = 0.0f;
        float blimp1MaxChance = 0.12f;

        float blimp2MinSpeed = 0.060f;
        float blimp2MaxSpeed = 0.070f;
        float blimp2MinChance = 0.13f;
        float blimp2MaxChance = 0.25f;

        float parachuteMinSpeed = GameItemsBehavior.BASE_SPEED;
        float parachuteMaxSpeed = 0.045f;
        float parachuteMinGenY = 0f;
        float parachuteMaxGenY = backgroundMaxY;

        float parachute1MinChance = 0.52f;
        float parachute1MaxChance = 0.60f;

        float parachute2MinChance = 0.61f;
        float parachute2MaxChance = 0.69f;

        float parachute3MinChance = 0.70f;
        float parachute3MaxChance = 0.78f;

        float planeMinSpeed = 0.10f;
        float planeMaxSpeed = 0.12f;

        float plane1MinChance = 0.26f;
        float plane1MaxChance = 0.38f;

        float plane2MinChance = 0.39f;
        float plane2MaxChance = 0.51f;

        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp1MinSpeed, blimp1MaxSpeed, blimp1MinChance, blimp1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp2MinSpeed, blimp2MaxSpeed, blimp2MinChance, blimp2MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddParachute1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute1MinChance, parachute1MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                           parachuteMinSpeed, parachuteMaxSpeed, parachute2MinChance, parachute2MaxChance,
                                           parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute3Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute3MinChance, parachute3MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddPlane1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               planeMinSpeed, planeMaxSpeed, plane1MinChance, plane1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddPlane2Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               planeMinSpeed, planeMaxSpeed, plane2MinChance, plane2MaxChance,
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

    private void Level5Prep()
    {
        background.GetComponent<SpriteRenderer>().sprite = nightSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = nightGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = nightGroundCloudsSprite;

        // Add obstacles to the level
        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;
        float enemyBirdMinChance = 0.88f;
        float enemyBirdMaxChance = 1.0f;

        float blimp1MinSpeed = 0.055f;
        float blimp1MaxSpeed = 0.065f;
        float blimp1MinChance = 0.0f;
        float blimp1MaxChance = 0.10f;

        float blimp2MinSpeed = 0.060f;
        float blimp2MaxSpeed = 0.070f;
        float blimp2MinChance = 0.11f;
        float blimp2MaxChance = 0.21f;

        float parachuteMinSpeed = GameItemsBehavior.BASE_SPEED;
        float parachuteMaxSpeed = 0.045f;
        float parachuteMinGenY = 0f;
        float parachuteMaxGenY = backgroundMaxY;

        float parachute1MinChance = 0.67f;
        float parachute1MaxChance = 0.73f;

        float parachute2MinChance = 0.74f;
        float parachute2MaxChance = 0.80f;

        float parachute3MinChance = 0.81f;
        float parachute3MaxChance = 0.87f;

        float planeMinSpeed = 0.10f;
        float planeMaxSpeed = 0.12f;

        float plane1MinChance = 0.22f;
        float plane1MaxChance = 0.32f;

        float plane2MinChance = 0.33f;
        float plane2MaxChance = 0.43f;

        float balloonMinSpeed = GameItemsBehavior.BASE_SPEED;
        float balloonMaxSpeed = 0.045f;
        float balloonMinGenY = backgroundMinY;
        float balloonMaxGenY = 0;

        float balloon1MinChance = 0.44f;
        float balloon1MaxChance = 0.54f;

        float balloon2MinChance = 0.55f;
        float balloon2MaxChance = 0.65f;

        float balloon3MinChance = 0.66f;
        float balloon3MaxChance = 0.76f;

        gameItemObjectScript.AddEnemyBirdObstacle(GameItemsBehavior.ANY_AMOUNT_IN_SCENE, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                  enemyBirdMinSpeed, enemyBirdMaxSpeed, enemyBirdMinChance, enemyBirdMaxChance,
                                                  backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp1MinSpeed, blimp1MaxSpeed, blimp1MinChance, blimp1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBlimp2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               blimp2MinSpeed, blimp2MaxSpeed, blimp2MinChance, blimp2MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddParachute1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute1MinChance, parachute1MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                           parachuteMinSpeed, parachuteMaxSpeed, parachute2MinChance, parachute2MaxChance,
                                           parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddParachute3Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                   parachuteMinSpeed, parachuteMaxSpeed, parachute3MinChance, parachute3MaxChance,
                                                   parachuteMinGenY, parachuteMaxGenY, 0, 0);
        gameItemObjectScript.AddPlane1Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               planeMinSpeed, planeMaxSpeed, plane1MinChance, plane1MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddPlane2Obstacle(2, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                               planeMinSpeed, planeMaxSpeed, plane2MinChance, plane2MaxChance,
                                               backgroundMinY, backgroundMaxY, 0, 0);
        gameItemObjectScript.AddBalloon1Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                 balloonMinSpeed, balloonMaxSpeed, balloon1MinChance, balloon1MaxChance,
                                                 balloonMinGenY, balloonMaxGenY, 0, 0);
        gameItemObjectScript.AddBalloon2Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                 balloonMinSpeed, balloonMaxSpeed, balloon2MinChance, balloon2MaxChance,
                                                 balloonMinGenY, balloonMaxGenY, 0, 0);
        gameItemObjectScript.AddBalloon3Obstacle(1, GameItemsBehavior.ANY_AMOUNT_IN_LEVEL,
                                                 balloonMinSpeed, balloonMaxSpeed, balloon3MinChance, balloon3MaxChance,
                                                 balloonMinGenY, balloonMaxGenY, 0, 0);



        if (false == inventoryData.IsBirdInInventory(InventoryData.SAM_ID))
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
