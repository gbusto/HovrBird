using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct GameObstacle
{
    public GameObject gameObject;
    public Transform transform;
    public Renderer renderer;
    public PolygonCollider2D collider;
    public float xDelta;
    public float yDelta;
    public bool used;
}

public struct ObstaclePrefab
{
    public GameObject prefab;
    public float minSpeed;
    public float maxSpeed;
    public float minChance;
    public float maxChance;
    public Direction[] directions;
}

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

public enum Direction
{
    none = 0,
    up,
    down
};

public class Level1Behavior : MonoBehaviour
{
    public GameObject commonPrefab;
    private GameObject commonObject;
    private CommonBehavior commonScript;

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
    private int prevCollectibleSlotIndex;
    private int[] collectibleSlots;

    private int obstacleGenerationCounter = obstacleGenerationTurns;
    private static readonly int obstacleGenerationTurns = 14;
    // The previous index used for generating an obstacle
    private int prevObstacleSlotIndex;
    private int[] obstacleSlots;

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
    private bool addEggToSlot;
    private bool caughtEgg;

    private LevelData levelData;

    List<(GameObject, float)> cloudPrefabs;
    List<(GameObject, Renderer)> cloudObjects;

    List<ObstaclePrefab> obstaclePrefabs;

    private const int NUM_ITEMS = 15;
    private GameObstacle[] gameObstacles = new GameObstacle[NUM_ITEMS];
    // We will only use (n-1) of the (n) slots in this array for collectibles
    // The (nth) slot is ONLY for the egg if the level is supposed to generate it
    private GameObstacle[] gameCollectibles = new GameObstacle[7];

    private static readonly int numberOfSlots = 9;
    private Slot[] slots;

    private readonly bool TEST_SLOTS;
    private System.Random sysRandom;

    GameObject[] collectiblePrefabArray;

    private InventoryData inventoryData;

    void Awake()
    {
        // DO NOT SEED THIS RANDOM
        sysRandom = new System.Random();

        slots = new Slot[numberOfSlots];

        // The slot numbers in which collectibles are generated
        collectibleSlots = new int[]
        {
            2, 4, 6
        };

        obstacleSlots = new int[]
        {
            0, 1, 3, 5, 7, 8
        };

        cloudPrefabs = new List<(GameObject, float)>();
        cloudObjects = new List<(GameObject, Renderer)>();

        obstaclePrefabs = new List<ObstaclePrefab>();

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

        commonObject = Instantiate(commonPrefab);
        commonScript = commonObject.GetComponent<CommonBehavior>();

        bird = commonScript.GetBirdObject();

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

        cameraScript = gameCamera.GetComponent<CameraBehavior>();
        cameraScript.bird = bird;

        cloudPrefabs.Add((cloud1Prefab, 0.10f));
        cloudPrefabs.Add((cloud2Prefab, 0.10f));
        cloudPrefabs.Add((cloud3Prefab, 0.10f));
        cloudPrefabs.Add((cloud4Prefab, 0.10f));

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

        // Camera orientation and off-screen item start calculation is now done
        // after the user presses the Start button

        ground1Renderer = groundObject1.GetComponent<Renderer>();
        ground2Renderer = groundObject2.GetComponent<Renderer>();
        groundObjectsDistance = groundObject2.transform.localPosition.x - groundObject1.transform.localPosition.x;

        Renderer backgroundRenderer = background.GetComponent<Renderer>();
        // This minimum value is the minimum Y value at which objects should be generated
        backgroundMinY = backgroundRenderer.bounds.min.y + backgroundRenderer.bounds.size.y * 0.10f;
        backgroundMaxY = backgroundRenderer.bounds.max.y;
        groundMaxY = groundObject1.transform.position.y + ground1Renderer.bounds.size.y;

        // Seed the random number generator here first
        Random.InitState(1);

        // These are percentages for generating obstacles in a given slot
        float[] percentages =
        {
            0.50f,
            0.30f,
            0.60f,
            0.50f,
            0.80f,
            0.50f,
            0.60f,
            0.30f,
            0.50f
        };

        // Calculate the mid points for each of the slots
        float diff = (backgroundMaxY - backgroundMinY) / numberOfSlots;
        for (int i = 0; i < numberOfSlots; ++i)
        {
            float slotMid = (backgroundMinY + diff / 2) + (diff * i);
            Slot slot = new Slot();
            slot.yPosition = slotMid;
            slot.chance = percentages[i];
            slots[i] = slot;
        }

        InitializeFirstClouds();

        // Start any continuous coroutine functions here
        StartCoroutine(CheckCollisions());
        StartCoroutine(CheckItemsCollected());
        StartCoroutine(CheckItemsForRemoval());
    }

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
                }

                // Update object positions in the game
                UpdateCloudObjects();
                UpdateGameObjectsPositions(gameCollectibles);
                UpdateGameObjectsPositions(gameObstacles);
                UpdateGroundObjects();

                // Update the level timer to know when to end the level
                levelTimer += Time.deltaTime;
                if (levelTimer >= levelTimeLength && generateItems)
                {
                    generateItems = false;

                    // Generate a finish line
                    int availableArrayIndex = gameObstacles.Length - 1;
                    GameObject finishLine = GenerateFinishLine();
                    AddObjectToArray(gameObstacles, availableArrayIndex, finishLine, finishLine.transform, speed);
                }
                else if (levelTimer >= timeForEggGeneration && generateEggItem && false == eggHasBeenGenerated)
                {
                    addEggToSlot = true;
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
                    UpdateCloudObjects();
                    UpdateGameObjectsPositions(gameObstacles);
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
                        // Generate a random object at a random height off screen
                        InstantiateObstacle();
                    }
                    obstacleGenerationCounter += 1;

                    // Update the counters for collectibles
                    if (collectibleGenerationCounter == collectibleGenerationTurns)
                    {
                        collectibleGenerationCounter = 0;
                        InstantiateCollectible();
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
                    DisableColliders();
                }
                else if (false == commonScript.disableColliders && collidersWereDisabled)
                {
                    // Enable collisions; they will be updated when object positions are updated
                    collidersWereDisabled = false;
                    EnableColliders();
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

                RemoveObjectFromGame(gameCollectibles, obj);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CheckItemsForRemoval()
    {
        for (; ;)
        {
            for (int i = 0; i < gameObstacles.Length; ++i)
            {
                if (gameObstacles[i].used)
                {
                    Renderer r = gameObstacles[i].renderer;
                    if (r.bounds.max.x < (-1 * cameraMaxX))
                    {
                        GameObject obj = gameObstacles[i].gameObject;
                        RemoveObjectFromGame(gameObstacles, obj);
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
                        RemoveObjectFromGame(gameCollectibles, obj);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion



    private void GameOver(bool success = false)
    {
        collidersWereDisabled = true;
        DisableColliders();

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
                        commonScript.UpdateHighScore(score);
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
                        commonScript.UpdateHighScore(score);
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
                        commonScript.UpdateHighScore(score);
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
                        commonScript.UpdateHighScore(score);
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
                        commonScript.UpdateHighScore(score);
                    }
                    else
                    {
                        commonScript.UpdateHighScore(levelData.levelData.level5HighScore);
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

    private GameObject GenerateFinishLine()
    {
        GameObject finishLine = Instantiate(finishLinePrefab);
        float lineWidth = finishLine.GetComponent<Renderer>().bounds.size.x;
        Vector3 pos = finishLine.transform.localPosition;
        Vector3 newPos = new Vector3(cameraMaxX + lineWidth, pos.y, pos.z);

        float maxObjectX = GetMaxObjectBound();
        if (maxObjectX > newPos.x)
        {
            newPos.x = maxObjectX + lineWidth;
            finishLine.transform.localPosition = newPos;
        }
        else
        {
            finishLine.transform.localPosition = newPos;
        }

        return finishLine;
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

    private ObstaclePrefab SelectPrefab()
    {
        ObstaclePrefab op = new ObstaclePrefab();
        float choice = Random.Range(0f, 1f);
        foreach (ObstaclePrefab thing in obstaclePrefabs)
        {
            if (choice <= thing.maxChance && choice >= thing.minChance)
            {
                return thing;
            }
        }

        return op;
    }

    private void TestSlots()
    {
        for (int i = 0; i < numberOfSlots; ++i)
        {
            int availableArrayIndex = FindAvailableArraySlot(gameObstacles);
            if (availableArrayIndex < 0)
            {
                // Don't generate a new obstacle because there aren't any available slots
                return;
            }

            Slot slot = slots[i];
            GameObject obj = Instantiate(blimp2Prefab);
            Renderer r = obj.GetComponent<Renderer>();
            PolygonCollider2D pc = obj.GetComponent<PolygonCollider2D>();
            float objWidth = r.bounds.size.x;
            float xStart = cameraMaxX + (objWidth / 2);
            float yStart = slot.yPosition;

            Vector3 pos = obj.transform.localPosition;
            pos.x = xStart;
            pos.y = yStart;
            obj.transform.localPosition = pos;

            Direction d = Direction.none;
            AddObjectToArray(gameObstacles, availableArrayIndex, obj, obj.transform, speed, d, r, pc);
        }
    }

    private void GenerateEgg()
    {
        int availableArrayIndex = gameCollectibles.Length - 1;

        int si = collectibleSlots[sysRandom.Next(0, collectibleSlots.Length)];
        while (si == prevCollectibleSlotIndex)
        {
            si = collectibleSlots[sysRandom.Next(0, collectibleSlots.Length)];
        }

        Slot slot = slots[si];

        // Generate this collectible item type
        GameObject obj = Instantiate(eggPrefab);
        Renderer r = obj.GetComponent<Renderer>();
        PolygonCollider2D pc = obj.GetComponent<PolygonCollider2D>();
        float objWidth = r.bounds.size.x;
        float xStart = cameraMaxX + (objWidth / 2);
        float yStart = slot.yPosition;

        Vector3 pos = obj.transform.localPosition;
        pos.x = xStart;
        pos.y = yStart;
        obj.transform.localPosition = pos;

        Direction d = Direction.none;
        AddObjectToArray(gameCollectibles, availableArrayIndex, obj, obj.transform, speed, d, r, pc);
    }

    private int FindAvailableArraySlot(GameObstacle[] array)
    {
        if (array.Length > 0)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (false == array[i].used)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void InstantiateCollectible()
    {
        if (addEggToSlot)
        {
            GenerateEgg();
            addEggToSlot = false;
            eggHasBeenGenerated = true;
            return;
        }

        int availableArrayIndex = FindAvailableArraySlot(gameCollectibles);
        if (availableArrayIndex < 0 || (gameCollectibles.Length - 1) == availableArrayIndex)
        {
            // Don't generate a new collectible because there aren't any available slots
            return;
        }

        int si = collectibleSlots[sysRandom.Next(0, collectibleSlots.Length)];
        // Ensure we don't use the same slot as time

        Slot slot = slots[si];

        float chance = (float)sysRandom.NextDouble();
        if (chance < slot.chance)
        {
            prevCollectibleSlotIndex = si;

            // Generate a collectible item in slots 3, 5, and 7
            GameObject prefab = collectiblePrefabArray[sysRandom.Next(0, collectiblePrefabArray.Length)];
            if (null != prefab)
            {
                // Generate this collectible item type
                GameObject obj = Instantiate(prefab);
                Renderer r = obj.GetComponent<Renderer>();
                PolygonCollider2D pc = obj.GetComponent<PolygonCollider2D>();
                float objWidth = r.bounds.size.x;
                float xStart = cameraMaxX + (objWidth / 2);
                float yStart = slot.yPosition;

                Vector3 pos = obj.transform.localPosition;
                pos.x = xStart;
                pos.y = yStart;
                obj.transform.localPosition = pos;

                Direction d = Direction.none;
                AddObjectToArray(gameCollectibles, availableArrayIndex, obj, obj.transform, speed, d, r, pc);
            }
        }
    }

    private void InstantiateObstacle()
    {
        int availableArrayIndex = FindAvailableArraySlot(gameObstacles);
        if (availableArrayIndex < 0 || (gameObstacles.Length - 1) == availableArrayIndex)
        {
            // Don't generate a new obstacle because there aren't any available slots
            return;
        }

        int si = obstacleSlots[Random.Range(0, obstacleSlots.Length - 1)];
        // Ensure we don't use the same slot as time
        while (si == prevObstacleSlotIndex)
        {
            si = obstacleSlots[Random.Range(0, obstacleSlots.Length - 1)];
        }

        Slot slot = slots[si];

        float chance = Random.Range(0f, 1f);
        if (chance < slot.chance)
        {
            prevObstacleSlotIndex = si;

            // Generate an obstacle in this slot
            ObstaclePrefab op = SelectPrefab();
            if (null != op.prefab)
            {
                Direction d = op.directions[Random.Range(0, op.directions.Length)];
                GameObject obj = Instantiate(op.prefab);
                Renderer r = obj.GetComponent<Renderer>();
                PolygonCollider2D pc = obj.GetComponent<PolygonCollider2D>();
                float objWidth = r.bounds.size.x;
                float xStart = cameraMaxX + (objWidth / 2);
                float yStart = slot.yPosition;

                Vector3 pos = obj.transform.localPosition;
                pos.x = xStart;
                pos.y = yStart;
                obj.transform.localPosition = pos;

                // Check to see if collider should be disabled for now
                if (collidersWereDisabled)
                {
                    pc.enabled = false;
                }

                float _speed = Random.Range(op.minSpeed, op.maxSpeed);

                AddObjectToArray(gameObstacles, availableArrayIndex, obj, obj.transform, _speed, d, r, pc);
            }
        }
    }

    private void UpdateGameObjectsPositions(GameObstacle[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i].used)
            {
                Transform t = array[i].transform;

                Vector3 pos = t.localPosition;
                pos.x -= array[i].xDelta;
                pos.y += array[i].yDelta;
                t.localPosition = pos;
            }
        }
    }

    private void RemoveObjectFromGame(GameObstacle[] array, GameObject obj)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            GameObject go = array[i].gameObject;
            if (obj == go)
            {
                Destroy(go);
                array[i].used = false;
            }
        }
    }

    private void DisableColliders()
    {
        for (int i = 0; i < gameObstacles.Length; ++i)
        {
            if (gameObstacles[i].used)
            {
                GameObject obj = gameObstacles[i].gameObject;
                PolygonCollider2D pc = gameObstacles[i].collider;
                if (obj.tag != "FinishLine" && obj.tag != "Collectible")
                {
                    pc.enabled = false;
                }
            }
        }
    }

    private void EnableColliders()
    {
        for (int i = 0; i < gameObstacles.Length; ++i)
        {
            if (gameObstacles[i].used)
            {
                GameObject obj = gameObstacles[i].gameObject;
                PolygonCollider2D pc = gameObstacles[i].collider;
                if (obj.tag != "FinishLine" && obj.tag != "Collectible")
                {
                    pc.enabled = true;
                }
            }
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

    private float GetMaxObjectBound()
    {
        float max = 0f;
        for (int i = 0; i < gameObstacles.Length; ++i)
        {
            if (gameObstacles[i].used)
            {
                Renderer r = gameObstacles[i].renderer;
                if (null != r)
                {
                    if (r.bounds.max.x > max)
                    {
                        max = r.bounds.max.x;
                    }
                }
            }
        }

        return max;
    }

    private void AddObjectToArray(GameObstacle[] array, int index, GameObject obj, Transform transform,
                                  float _speed, Direction d = Direction.none, Renderer _renderer = null,
                                  PolygonCollider2D _collider = null)
    {
        Renderer r;
        PolygonCollider2D pc;

        if (null == _renderer)
        {
            r = obj.GetComponent<Renderer>();
        }
        else
        {
            r = _renderer;
        }

        if (null == _collider)
        {
            pc = obj.GetComponent<PolygonCollider2D>();
        }
        else
        {
            pc = _collider;
        }

        float yDelta = 0f;
        switch (d)
        {
            case Direction.up:
                yDelta = Random.Range(0.0005f, 0.006f);
                break;

            case Direction.down:
                yDelta = -1 * Random.Range(0.0005f, 0.006f);
                break;
        }

        GameObstacle obstacle = new GameObstacle();
        obstacle.gameObject = obj;
        obstacle.transform = transform;
        obstacle.renderer = r;
        obstacle.collider = pc;
        obstacle.yDelta = yDelta;
        obstacle.xDelta = _speed * 1.10f;
        obstacle.used = true;

        array[index] = obstacle;
    }

    private void AddNewPrefab(GameObject prefab, float minSpeed, float maxSpeed, float minChance, float maxChance, Direction[] directions)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = prefab;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.directions = directions;
        obstaclePrefabs.Add(op);
    }

    private void Level1Prep()
    {
        // Setting up the obstacle prefabs
        Direction[] enemyDirections =
        {
            Direction.up,
            Direction.down
        };

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        AddNewPrefab(enemyBirdPrefab, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.00f, 1.00f, enemyDirections);

        // 1 banana
        // 3 coins
        collectiblePrefabArray = new GameObject[6];
        collectiblePrefabArray[0] = bananaPrefab;
        for (int i = 1; i < 4; ++i)
        {
            collectiblePrefabArray[i] = coinPrefab;
        }

        commonScript.InitItemCanvasImage(0, collectibleItem1Sprite);
        commonScript.InitItemCanvasImage(1, collectibleItem2Sprite);
    }

    private void Level2Prep()
    {
        Direction[] blimpDirections =
        {
            Direction.up,
            Direction.down
        };
        Direction[] enemyDirections =
        {
            Direction.up,
            Direction.down
        };

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float blimpMinSpeed = 0.055f;
        float blimpMaxSpeed = 0.065f;

        AddNewPrefab(blimp1Prefab, blimpMinSpeed, blimpMaxSpeed, 0.00f, 0.20f, blimpDirections);
        AddNewPrefab(blimp2Prefab, blimpMinSpeed, blimpMaxSpeed, 0.21f, 0.40f, blimpDirections);
        AddNewPrefab(enemyBirdPrefab, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.41f, 1.00f, enemyDirections);

        // 1 banana
        // 3 coins
        collectiblePrefabArray = new GameObject[6];
        collectiblePrefabArray[0] = bananaPrefab;
        for (int i = 1; i < 4; ++i)
        {
            collectiblePrefabArray[i] = coinPrefab;
        }

        commonScript.InitItemCanvasImage(0, collectibleItem1Sprite);
        commonScript.InitItemCanvasImage(1, collectibleItem2Sprite);
    }

    private void Level3Prep()
    {
        Direction[] blimpDirections = {
            Direction.up,
            Direction.down
        };
        Direction[] parachuteDirections =
        {
            Direction.down
        };
        Direction[] enemyDirections =
        {
            Direction.up,
            Direction.down
        };

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float blimpMinSpeed = 0.055f;
        float blimpMaxSpeed = 0.065f;

        float parachuteMinSpeed = 0.04f;
        float parachuteMaxSpeed = 0.045f;

        background.GetComponent<SpriteRenderer>().sprite = sunsetSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        AddNewPrefab(blimp1Prefab, blimpMinSpeed, blimpMaxSpeed, 0.00f, 0.16f, blimpDirections);
        AddNewPrefab(blimp2Prefab, blimpMinSpeed, blimpMaxSpeed, 0.17f, 0.33f, blimpDirections);
        AddNewPrefab(parachute1Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.34f, 0.44f, parachuteDirections);
        AddNewPrefab(parachute2Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.45f, 0.55f, parachuteDirections);
        AddNewPrefab(parachute3Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.56f, 0.66f, parachuteDirections);
        AddNewPrefab(enemyBirdPrefab, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.67f, 1.00f, enemyDirections);

        // 1 blueberry
        // 2 bananas
        // 6 coins
        collectiblePrefabArray = new GameObject[12];
        collectiblePrefabArray[0] = blueberryPrefab;
        for (int i = 1; i < 3; ++i)
        {
            collectiblePrefabArray[i] = bananaPrefab;
        }
        for (int i = 3; i < 9; ++i)
        {
            collectiblePrefabArray[i] = coinPrefab;
        }

        commonScript.InitItemCanvasImage(0, collectibleItem1Sprite);
        commonScript.InitItemCanvasImage(1, collectibleItem2Sprite);
        commonScript.InitItemCanvasImage(2, collectibleItem3Sprite);
    }

    private void Level4Prep()
    {
        Direction[] blimpDirections = {
            Direction.up,
            Direction.down
        };
        Direction[] planeDirections =
        {
            Direction.none
        };
        Direction[] parachuteDirections =
        {
            Direction.down
        };
        Direction[] enemyDirections =
        {
            Direction.up,
            Direction.down
        };

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float blimpMinSpeed = 0.055f;
        float blimpMaxSpeed = 0.065f;

        float planeMinSpeed = 0.10f;
        float planeMaxSpeed = 0.12f;

        float parachuteMinSpeed = 0.04f;
        float parachuteMaxSpeed = 0.045f;

        background.GetComponent<SpriteRenderer>().sprite = sunsetSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = sunsetGroundCloudsSprite;
        AddNewPrefab(blimp1Prefab, blimpMinSpeed, blimpMaxSpeed, 0.00f, 0.12f, blimpDirections);
        AddNewPrefab(blimp2Prefab, blimpMinSpeed, blimpMaxSpeed, 0.13f, 0.25f, blimpDirections);
        AddNewPrefab(plane1Prefab, planeMinSpeed, planeMaxSpeed, 0.26f, 0.38f, planeDirections);
        AddNewPrefab(plane2Prefab, planeMinSpeed, planeMaxSpeed, 0.39f, 0.51f, planeDirections);
        AddNewPrefab(parachute1Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.52f, 0.60f, parachuteDirections);
        AddNewPrefab(parachute2Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.61f, 0.69f, parachuteDirections);
        AddNewPrefab(parachute3Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.70f, 0.78f, parachuteDirections);
        AddNewPrefab(enemyBirdPrefab, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.79f, 1.00f, enemyDirections);

        // 1 blueberry
        // 2 bananas
        // 6 coins
        collectiblePrefabArray = new GameObject[12];
        collectiblePrefabArray[0] = blueberryPrefab;
        for (int i = 1; i < 3; ++i)
        {
            collectiblePrefabArray[i] = bananaPrefab;
        }
        for (int i = 3; i < 9; ++i)
        {
            collectiblePrefabArray[i] = coinPrefab;
        }

        commonScript.InitItemCanvasImage(0, collectibleItem1Sprite);
        commonScript.InitItemCanvasImage(1, collectibleItem2Sprite);
        commonScript.InitItemCanvasImage(2, collectibleItem3Sprite);
    }

    private void Level5Prep()
    {
        Direction[] blimpDirections = {
            Direction.up,
            Direction.down
        };
        Direction[] planeDirections =
        {
            Direction.none
        };
        Direction[] parachuteDirections =
        {
            Direction.down
        };
        Direction[] balloonDirections =
        {
            Direction.up
        };
        Direction[] enemyDirections =
        {
            Direction.up,
            Direction.down
        };

        float enemyBirdMinSpeed = 0.05f;
        float enemyBirdMaxSpeed = 0.055f;

        float blimpMinSpeed = 0.055f;
        float blimpMaxSpeed = 0.065f;

        float planeMinSpeed = 0.10f;
        float planeMaxSpeed = 0.12f;

        float parachuteMinSpeed = 0.04f;
        float parachuteMaxSpeed = 0.045f;

        float balloonMinSpeed = 0.04f;
        float balloonMaxSpeed = 0.045f;

        background.GetComponent<SpriteRenderer>().sprite = nightSkySprite;
        groundObject1.GetComponent<SpriteRenderer>().sprite = nightGroundCloudsSprite;
        groundObject2.GetComponent<SpriteRenderer>().sprite = nightGroundCloudsSprite;
        AddNewPrefab(blimp1Prefab, blimpMinSpeed, blimpMaxSpeed, 0.00f, 0.10f, blimpDirections);
        AddNewPrefab(blimp2Prefab, blimpMinSpeed, blimpMaxSpeed, 0.11f, 0.21f, blimpDirections);
        AddNewPrefab(plane1Prefab, planeMinSpeed, planeMaxSpeed, 0.22f, 0.32f, planeDirections);
        AddNewPrefab(plane2Prefab, planeMinSpeed, planeMaxSpeed, 0.33f, 0.43f, planeDirections);
        AddNewPrefab(balloon1Prefab, balloonMinSpeed, balloonMaxSpeed, 0.44f, 0.54f, balloonDirections);
        AddNewPrefab(balloon2Prefab, balloonMinSpeed, balloonMaxSpeed, 0.55f, 0.65f, balloonDirections);
        AddNewPrefab(balloon3Prefab, balloonMinSpeed, balloonMaxSpeed, 0.66f, 0.66f, balloonDirections);
        AddNewPrefab(parachute1Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.67f, 0.73f, parachuteDirections);
        AddNewPrefab(parachute2Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.74f, 0.80f, parachuteDirections);
        AddNewPrefab(parachute3Prefab, parachuteMinSpeed, parachuteMaxSpeed, 0.81f, 0.87f, parachuteDirections);
        AddNewPrefab(enemyBirdPrefab, enemyBirdMinSpeed, enemyBirdMaxSpeed, 0.88f, 1.00f, enemyDirections);

        // 1 strawberry
        // 8 blueberries
        // 16 bananas
        // 50 coins
        collectiblePrefabArray = new GameObject[100];
        collectiblePrefabArray[0] = strawberryPrefab;
        for (int i = 1; i < 9; ++i)
        {
            collectiblePrefabArray[i] = blueberryPrefab;
        }
        for (int i = 9; i < 25; ++i)
        {
            collectiblePrefabArray[i] = bananaPrefab;
        }
        for (int i = 25; i < 75; ++i)
        {
            collectiblePrefabArray[i] = coinPrefab;
        }

        // Only generate the egg if it hasn't been caught yet
        if (false == inventoryData.IsBirdInInventory(InventoryData.SAM_ID))
        {
            generateEggItem = true;
            // Pick a random time to generate the egg;
            // DO THIS BEFORE WE SEED UNITYENGINE RANDOM!!!
            timeForEggGeneration = Random.Range(1.0f, levelTimeLength);

            commonScript.InitItemCanvasImage(7, eggSprite);
        }

        commonScript.InitItemCanvasImage(0, collectibleItem1Sprite);
        commonScript.InitItemCanvasImage(1, collectibleItem2Sprite);
        commonScript.InitItemCanvasImage(2, collectibleItem3Sprite);
        commonScript.InitItemCanvasImage(3, collectibleItem4Sprite);
    }
}
