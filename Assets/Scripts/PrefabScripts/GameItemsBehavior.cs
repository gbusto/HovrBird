using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct ObstacleCounter
{
    public int id;
    public int countInScene;
    public int countInLevel;

    public ObstacleCounter(int id, int sceneCount, int levelCount)
    {
        this.id = id;
        countInScene = sceneCount;
        countInLevel = levelCount;
    }

    public void IncrementSceneCount()
    {
        countInScene += 1;
    }

    public void DecrementSceneCount()
    {
        countInScene -= 1;
    }

    public void IncrementLevelCount()
    {
        countInLevel += 1;
    }
}

public struct GameObstacle
{
    public GameObject gameObject;
    public Transform transform;
    public Renderer renderer;
    public PolygonCollider2D collider;
    public Direction direction;
    public float xDelta; // Left moving speed of an object each frame
    public float yDelta; // Vertical change of an object each frame
    public float startY; // The Y value at which an object starts (bottom of the "wave")
    public float endY;   // The Y value at which an object should begin descending (top of the "wave")
    public bool incrementY; // Boolean that says whether Y value should be incrementing or decrementing
    public bool used;
    public int _id;
}

public struct ObstaclePrefab
{
    public GameObject prefab;
    public float startX;
    public float endX;
    public float minSpeed;
    public float maxSpeed;
    public float minChance;
    public float maxChance;
    public float maxGenY; // Largest starting value for Y for an object
    public float minGenY; // Lowest starting value for Y for an object
    public float maxMoveY; // Largest value of Y when object is in motion before it should descend
    public float minMoveY; // Lowest value of Y when object is in motion before it should start ascending
    // Maximum number of this type of item that is allowed to be visible on the
    // screen at one time (prevent large items from cluttering the scene)
    public int maxNumInScene;
    // Max number of this item that should be generated in the level
    public int maxNumInLevel;
    public Direction[] directions;
    public int _id;
}

public enum Direction
{
    none = 0,
    up,
    down,
    wave,
    arch_up, // n
    arch_down, // u
};

public class GameItemsBehavior : MonoBehaviour
{
    // All obstacle sprites
    public GameObject enemyBird1Prefab;
    public GameObject enemyToucanPrefab;
    public GameObject enemyPelicanPrefab;
    public GameObject enemySeagullPrefab;

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
    public GameObject wavePrefab;
    public GameObject fish1Prefab;
    public GameObject fish2Prefab;
    public GameObject fish3Prefab;
    public GameObject boat1Prefab;
    public GameObject boat2Prefab;
    public GameObject shark1Prefab;
    public GameObject beachBallPrefab;
    public GameObject footballPrefab;
    public GameObject palmTreePrefab;
    public GameObject propPlanePrefab;
    public GameObject stormCloud1;
    public GameObject stormCloud2;

    // All trigger prefabs
    public GameObject eggPrefab;
    public GameObject finishLinePrefab;
    public GameObject coinPrefab;
    public GameObject bananaPrefab;
    public GameObject blueberriesPrefab;
    public GameObject strawberryPrefab;

    public GameObstacleBehavior[] gameObstacles;
    public bool[] gameObstacleIndices;

    public GameObstacleBehavior[] gameCollectibles;
    public bool[] gameCollectibleIndices;

    public const float BASE_SPEED = 0.04f;

    public const int ANY_AMOUNT_IN_LEVEL = -1;
    public const int ANY_AMOUNT_IN_SCENE = -1;

    public const float COLLECTIBLE_MIN_Y = -4f;
    public const float COLLECTIBLE_MAX_Y = 4f;

    private List<ObstacleCounter> obstacleCounts;
    private List<ObstaclePrefab> obstaclePrefabs;

    private List<ObstacleCounter> collectibleCounts;
    private List<ObstaclePrefab> collectiblePrefabs;

    private System.Random sysRandom;

    // XXX This should be based on the max allowed number of obstacles
    private const float OBSTACLE_CHANCE_CHANGE = 0.05f;
    private float obstacleMaxChance = 1f;

    // XXX This should be based on the max allowed number of collectibles
    private const float COLLECTIBLE_CHANCE_CHANGE = 0.10f;
    private float collectibleMaxChance = 1f;

    private float cameraMaxX;
    private float backgroundMinY;
    private float backgroundMaxY;

    public bool active;

    private enum ItemIds
    {
        eggId = 0x1,
        coinId,
        bananaId,
        blueberryId,
        strawberryId,
        finishLineId = 0x50,
        enemyBird1Id,
        blimp1Id,
        blimp2Id,
        plane1Id,
        plane2Id,
        parachute1Id,
        parachute2Id,
        parachute3Id,
        balloon1Id,
        balloon2Id,
        balloon3Id,
        waveId,
        fish1Id,
        fish2Id,
        fish3Id,
        boat1Id,
        boat2Id,
        shark1Id,
        beachBallId,
        footballId,
        palmTreeId,
        propPlaneId,
        stormCloud1Id,
        stormCloud2Id,
    }

    private void Awake()
    {
        // DO NOT SEED THIS RANDOM
        sysRandom = new System.Random();

        obstacleCounts = new List<ObstacleCounter>();
        obstaclePrefabs = new List<ObstaclePrefab>();

        collectibleCounts = new List<ObstacleCounter>();
        collectiblePrefabs = new List<ObstaclePrefab>();
    }

    IEnumerator CheckItemsForRemoval()
    {
        for (; ; )
        {
            for (int i = 0; i < gameObstacleIndices.Length; ++i)
            {
                if (gameObstacleIndices[i])
                {
                    if (gameObstacles[i].cleanup)
                    {
                        int ocIndex = obstacleCounts.FindIndex(gc => gc.id == gameObstacles[i]._id);
                        if (ocIndex >= 0)
                        {
                            ObstacleCounter oc = obstacleCounts[ocIndex];
                            oc.DecrementSceneCount();
                            obstacleCounts[ocIndex] = oc;
                        }
                        Destroy(gameObstacles[i].gameObject);
                        gameObstacleIndices[i] = false;
                        obstacleMaxChance += OBSTACLE_CHANCE_CHANGE;
                    }
                }
            }

            for (int i = 0; i < gameCollectibleIndices.Length; ++i)
            {
                if (gameCollectibleIndices[i])
                {
                    if (gameCollectibles[i].cleanup)
                    {
                        int ocIndex = collectibleCounts.FindIndex(gc => gc.id == gameCollectibles[i]._id);
                        if (ocIndex >= 0)
                        {
                            ObstacleCounter oc = collectibleCounts[ocIndex];
                            oc.DecrementSceneCount();
                            collectibleCounts[ocIndex] = oc;
                        }
                        Destroy(gameCollectibles[i].gameObject);
                        gameCollectibleIndices[i] = false;
                        collectibleMaxChance += COLLECTIBLE_CHANCE_CHANGE;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void RemoveCollectibleFromGame(GameObject obj)
    {
        for (int i = 0; i < gameCollectibleIndices.Length; ++i)
        {
            if (gameCollectibleIndices[i])
            {
                if (gameCollectibles[i].gameObject == obj)
                {
                    int ocIndex = collectibleCounts.FindIndex(gc => gc.id == gameCollectibles[i]._id);
                    if (ocIndex >= 0)
                    {
                        ObstacleCounter oc = collectibleCounts[ocIndex];
                        oc.DecrementSceneCount();
                        collectibleCounts[ocIndex] = oc;
                    }
                    Destroy(gameCollectibles[i].gameObject);
                    gameCollectibleIndices[i] = false;
                    collectibleMaxChance += COLLECTIBLE_CHANCE_CHANGE;
                }
            }
        }
    }

    public void InitGameObjectManager(float cameraMaxX, float backgroundMinY,
                                      float backgroundMaxY, int numObstacles,
                                      int numCollectibles)
    {
        this.cameraMaxX = cameraMaxX;
        this.backgroundMinY = backgroundMinY;
        this.backgroundMaxY = backgroundMaxY;

        gameObstacles = new GameObstacleBehavior[numObstacles];
        gameObstacleIndices = new bool[numObstacles];

        gameCollectibles = new GameObstacleBehavior[numCollectibles];
        gameCollectibleIndices = new bool[numCollectibles];

        StartCoroutine(CheckItemsForRemoval());
    }

    private int SelectPrefab(List<ObstaclePrefab> itemList, bool isObstacle)
    {
        float choice = 0f;
        if (isObstacle)
        {
            choice = Random.Range(0f, 1f);
        }
        else
        {
            choice = (float)sysRandom.NextDouble();
        }

        for (int i = 0; i < itemList.Count; ++i)
        {
            if (choice <= itemList[i].maxChance && choice >= itemList[i].minChance)
            {
                return i;
            }
        }

        return 0;
    }

    private int FindAvailableArraySlot(bool[] array)
    {
        // Last index of the obstacle and collectible arrays are for special items
        for (int i = 0; i < array.Length - 1; ++i)
        {
            if (!array[i])
            {
                return i;
            }
        }

        return -1;
    }

    public void StopGame()
    {
        for (int i = 0; i < gameObstacleIndices.Length; ++i)
        {
            if (gameObstacleIndices[i])
            {
                gameObstacles[i].active = false;
            }
        }

        for (int i = 0; i < gameCollectibleIndices.Length; ++i)
        {
            if (gameCollectibleIndices[i])
            {
                gameCollectibles[i].active = false;
            }
        }

        active = false;
    }

    public void StartGame()
    {
        for (int i = 0; i < gameObstacleIndices.Length; ++i)
        {
            if (gameObstacleIndices[i])
            {
                gameObstacles[i].active = true;
            }
        }

        for (int i = 0; i < gameCollectibleIndices.Length; ++i)
        {
            if (gameCollectibleIndices[i])
            {
                gameCollectibles[i].active = true;
            }
        }

        active = true;
    }

    public void GenerateFinishLine()
    {
        int arrayIndex = gameObstacles.Length - 1;

        ObstaclePrefab op = new ObstaclePrefab();
        op._id = (int)ItemIds.finishLineId;
        op.startX = finishLinePrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = BASE_SPEED;
        op.maxSpeed = BASE_SPEED;

        GameObject finishLine = Instantiate(finishLinePrefab);
        GameObstacleBehavior script = finishLine.GetComponent<GameObstacleBehavior>();
        script.InitGameObstacle(op, false);

        gameObstacles[arrayIndex] = script;
        gameObstacleIndices[arrayIndex] = true;
    }

    public void GenerateNewObstacle(bool disableColliders)
    {
        int arrayIndex = FindAvailableArraySlot(gameObstacleIndices);
        if (arrayIndex < 0)
        {
            // No slots available at this time (saving the last one for the finish line)
            return;
        }

        float chance = Random.Range(0f, 1f);
        if (chance <= obstacleMaxChance)
        {
            int opIndex = SelectPrefab(obstaclePrefabs, true);
            ObstaclePrefab op = obstaclePrefabs[opIndex];
            int ocIndex = obstacleCounts.FindIndex(gc => gc.id == op._id);
            ObstacleCounter oc = obstacleCounts[ocIndex];
            if ((oc.countInScene >= op.maxNumInScene) && op.maxNumInScene > 0)
            {
                // Obey the limit for how many can be displayed in the scene at any given time
                return;
            }
            if ((oc.countInLevel >= op.maxNumInLevel) && op.maxNumInLevel > 0)
            {
                // Obey the limit for how many can be displayed in the level at any given time
                return;
            }

            // Increment the count
            oc.IncrementSceneCount();
            oc.IncrementLevelCount();
            obstacleCounts[ocIndex] = oc;

            GameObject obj = Instantiate(op.prefab);
            GameObstacleBehavior script = obj.GetComponent<GameObstacleBehavior>();
            script.InitGameObstacle(op, disableColliders);

            gameObstacles[arrayIndex] = script;
            gameObstacleIndices[arrayIndex] = true;

            obstacleMaxChance -= OBSTACLE_CHANCE_CHANGE;
        }
    }

    public void GenerateNewCollectible(bool timeToGenerateNewEgg)
    {
        int arrayIndex;

        if (timeToGenerateNewEgg)
        {
            arrayIndex = gameCollectibles.Length - 1;

            ObstaclePrefab op = new ObstaclePrefab();
            op._id = (int)ItemIds.eggId;
            op.startX = eggPrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
            op.endX = op.startX * -1;
            op.minGenY = -2f;
            op.maxGenY = 2f;
            op.minSpeed = BASE_SPEED;
            op.maxSpeed = BASE_SPEED;

            GameObject egg = Instantiate(eggPrefab);
            GameObstacleBehavior script = egg.GetComponent<GameObstacleBehavior>();
            script.InitGameObstacle(op, false);

            gameCollectibles[arrayIndex] = script;
            gameCollectibleIndices[arrayIndex] = true;

            return;
        }

        arrayIndex = FindAvailableArraySlot(gameCollectibleIndices);
        if (arrayIndex < 0)
        {
            // No slots available at this time (saving the last one for the finish line)
            return;
        }

        float chance = Random.Range(0f, 1f);
        if (chance <= collectibleMaxChance)
        {
            int opIndex = SelectPrefab(collectiblePrefabs, false);
            ObstaclePrefab op = collectiblePrefabs[opIndex];
            int ocIndex = collectibleCounts.FindIndex(gc => gc.id == op._id);
            ObstacleCounter oc = collectibleCounts[ocIndex];
            if ((oc.countInScene >= op.maxNumInScene) && op.maxNumInScene > 0)
            {
                // Obey the limit for how many can be displayed in the scene at any given time
                return;
            }
            if ((oc.countInLevel >= op.maxNumInLevel) && op.maxNumInLevel > 0)
            {
                // Obey the limit for how many can be displayed in the level at any given time
                return;
            }

            // Increment the count
            oc.IncrementSceneCount();
            oc.IncrementLevelCount();
            collectibleCounts[ocIndex] = oc;

            GameObject obj = Instantiate(op.prefab);
            GameObstacleBehavior script = obj.GetComponent<GameObstacleBehavior>();
            script.InitGameObstacle(op, false);

            gameCollectibles[arrayIndex] = script;
            gameCollectibleIndices[arrayIndex] = true;

            collectibleMaxChance -= COLLECTIBLE_CHANCE_CHANGE;
        }
    }

    public void DisableColliders()
    {
        for (int i = 0; i < gameObstacleIndices.Length; ++i)
        {
            if (gameObstacleIndices[i])
            {
                gameObstacles[i].disableColliders = true;
            }
        }
    }

    public void EnableColliders()
    {
        for (int i = 0; i < gameObstacleIndices.Length; ++i)
        {
            if (gameObstacleIndices[i])
            {
                gameObstacles[i].disableColliders = false;
            }
        }
    }


    /*
     * Obstacle generation helper functions
     */
    public void AddEnemyBirdObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                     float maxSpeed, float minChance, float maxChance,
                                     float minGenY, float maxGenY, float minMoveY,
                                     float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.down,
            Direction.up,
            Direction.none,
        };

        uint activeBirdId = PlayerPrefsCommon.GetActiveBirdId();
        GameObject prefab;
        switch (activeBirdId)
        {
            case InventoryData.KOKO_ID:
                prefab = enemyBird1Prefab;
                break;

            case InventoryData.ANGRY_KOKO_ID:
                prefab = enemyBird1Prefab;
                break;

            case InventoryData.SAM_ID:
                prefab = enemyToucanPrefab;
                break;

            case InventoryData.ANGRY_SAM_ID:
                prefab = enemyToucanPrefab;
                break;

            case InventoryData.NIGEL_ID:
                prefab = enemyPelicanPrefab;
                break;

            case InventoryData.ANGRY_NIGEL_ID:
                prefab = enemyPelicanPrefab;
                break;

            case InventoryData.STEVEN_ID:
                prefab = enemySeagullPrefab;
                break;

            case InventoryData.ANGRY_STEVEN_ID:
                prefab = enemySeagullPrefab;
                break;

            default:
                prefab = enemyBird1Prefab;
                break;
        }

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = prefab;
        op.startX = prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.enemyBird1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBlimp1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                  float maxSpeed, float minChance, float maxChance,
                                  float minGenY, float maxGenY, float minMoveY,
                                  float maxMoveY)
    {

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = blimp1Prefab;
        op.startX = blimp1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.blimp1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBlimp2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                  float maxSpeed, float minChance, float maxChance,
                                  float minGenY, float maxGenY, float minMoveY,
                                  float maxMoveY)
    {

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = blimp2Prefab;
        op.startX = blimp2Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.blimp2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddParachute1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                      float maxSpeed, float minChance, float maxChance,
                                      float minGenY, float maxGenY, float minMoveY,
                                      float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = parachute1Prefab;
        op.startX = parachute1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.parachute1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddParachute2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                      float maxSpeed, float minChance, float maxChance,
                                      float minGenY, float maxGenY, float minMoveY,
                                      float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = parachute2Prefab;
        op.startX = parachute2Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.parachute2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddParachute3Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                      float maxSpeed, float minChance, float maxChance,
                                      float minGenY, float maxGenY, float minMoveY,
                                      float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = parachute3Prefab;
        op.startX = parachute3Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.parachute3Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddPlane1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                  float maxSpeed, float minChance, float maxChance,
                                  float minGenY, float maxGenY, float minMoveY,
                                  float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = plane1Prefab;
        op.startX = plane1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.plane1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddPlane2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                  float maxSpeed, float minChance, float maxChance,
                                  float minGenY, float maxGenY, float minMoveY,
                                  float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = plane2Prefab;
        op.startX = plane2Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.plane2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBalloon1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                    float maxSpeed, float minChance, float maxChance,
                                    float minGenY, float maxGenY, float minMoveY,
                                    float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = balloon1Prefab;
        op.startX = balloon1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.balloon1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBalloon2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                    float maxSpeed, float minChance, float maxChance,
                                    float minGenY, float maxGenY, float minMoveY,
                                    float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = balloon2Prefab;
        op.startX = balloon2Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.balloon2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBalloon3Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                    float maxSpeed, float minChance, float maxChance,
                                    float minGenY, float maxGenY, float minMoveY,
                                    float maxMoveY)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = balloon3Prefab;
        op.startX = balloon3Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op._id = (int)ItemIds.balloon3Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddWaveObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                float maxSpeed, float minChance, float maxChance,
                                float minGenY, float maxGenY, float minMoveY,
                                float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = wavePrefab;
        op.startX = wavePrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.waveId;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddFish1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                 float maxSpeed, float minChance, float maxChance,
                                 float minGenY, float maxGenY, float minMoveY,
                                 float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = fish1Prefab;
        op.startX = fish1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.fish1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddFish2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                 float maxSpeed, float minChance, float maxChance,
                                 float minGenY, float maxGenY, float minMoveY,
                                 float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = fish2Prefab;
        op.startX = fish2Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.fish2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddFish3Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                 float maxSpeed, float minChance, float maxChance,
                                 float minGenY, float maxGenY, float minMoveY,
                                 float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = fish3Prefab;
        op.startX = fish3Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.fish3Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBoat1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                 float maxSpeed, float minChance, float maxChance,
                                 float minGenY, float maxGenY, float minMoveY,
                                 float maxMoveY)
    {
        Direction[] directions =
        {
           Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = boat1Prefab;
        op.startX = boat1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.boat1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddShark1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                  float maxSpeed, float minChance, float maxChance,
                                  float minGenY, float maxGenY, float minMoveY,
                                  float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = shark1Prefab;
        op.startX = shark1Prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.shark1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddBeachBallObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                     float maxSpeed, float minChance, float maxChance,
                                     float minGenY, float maxGenY, float minMoveY,
                                     float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.arch_up
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = beachBallPrefab;
        op.startX = beachBallPrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.beachBallId;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddFootballObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                    float maxSpeed, float minChance, float maxChance,
                                    float minGenY, float maxGenY, float minMoveY,
                                    float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.arch_up
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = footballPrefab;
        op.startX = footballPrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.footballId;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddPalmTreeObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                    float maxSpeed, float minChance, float maxChance,
                                    float minGenY, float maxGenY, float minMoveY,
                                    float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = palmTreePrefab;
        op.startX = palmTreePrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.palmTreeId;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddPropPlaneObstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                     float maxSpeed, float minChance, float maxChance,
                                     float minGenY, float maxGenY, float minMoveY,
                                     float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = propPlanePrefab;
        op.startX = propPlanePrefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.propPlaneId;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddStormCloud1Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                      float maxSpeed, float minChance, float maxChance,
                                      float minGenY, float maxGenY, float minMoveY,
                                      float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = stormCloud1;
        op.startX = stormCloud1.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.stormCloud1Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }

    public void AddStormCloud2Obstacle(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                      float maxSpeed, float minChance, float maxChance,
                                      float minGenY, float maxGenY, float minMoveY,
                                      float maxMoveY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = stormCloud2;
        op.startX = stormCloud2.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxGenY = maxGenY;
        op.minGenY = minGenY;
        op.maxMoveY = maxMoveY;
        op.minMoveY = minMoveY;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.directions = directions;
        op._id = (int)ItemIds.stormCloud2Id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }


    /*
     * Collectible generation helper functions
     */
    public void AddCoinCollectible(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                   float maxSpeed, float minChance, float maxChance,
                                   float minGenY, float maxGenY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(coinPrefab, directions, maxNumInScene, maxNumInLevel,
                       minSpeed, maxSpeed, minChance, maxChance, minGenY, maxGenY,
                       ItemIds.coinId);
    }

    public void AddBananaCollectible(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                     float maxSpeed, float minChance, float maxChance,
                                     float minGenY, float maxGenY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(bananaPrefab, directions, maxNumInScene, maxNumInLevel,
                       minSpeed, maxSpeed, minChance, maxChance, minGenY, maxGenY,
                       ItemIds.bananaId);
    }

    public void AddBlueberryCollectible(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                        float maxSpeed, float minChance, float maxChance,
                                        float minGenY, float maxGenY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(blueberriesPrefab, directions, maxNumInScene, maxNumInLevel,
                       minSpeed, maxSpeed, minChance, maxChance, minGenY, maxGenY,
                       ItemIds.blueberryId);
    }

    public void AddStrawberryCollectible(int maxNumInScene, int maxNumInLevel, float minSpeed,
                                         float maxSpeed, float minChance, float maxChance,
                                         float minGenY, float maxGenY)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(strawberryPrefab, directions, maxNumInScene, maxNumInLevel,
                       minSpeed, maxSpeed, minChance, maxChance, minGenY, maxGenY,
                       ItemIds.strawberryId);
    }

    private void AddCollectible(GameObject prefab, Direction[] directions, int maxNumInScene,
                                int maxNumInLevel, float minSpeed, float maxSpeed, float minChance,
                                float maxChance, float minGenY, float maxGenY, ItemIds _id)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = prefab;
        op.startX = prefab.GetComponent<Renderer>().bounds.max.x + cameraMaxX;
        op.endX = op.startX * -1;
        op.maxNumInScene = maxNumInScene;
        op.maxNumInLevel = maxNumInLevel;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.minGenY = minGenY;
        op.maxGenY = maxGenY;
        op.directions = directions;
        op._id = (int)_id;

        collectiblePrefabs.Add(op);
        collectibleCounts.Add(new ObstacleCounter(op._id, 0, 0));
    }
}
