using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameObstacle
{
    public GameObject gameObject;
    public Transform transform;
    public Renderer renderer;
    public PolygonCollider2D collider;
    public Direction direction;
    public float xDelta;
    public float yDelta;
    public float startY;
    public float endY;
    public bool used;
    public bool incrementY;
    public int _id;
}

public struct ObstaclePrefab
{
    public GameObject prefab;
    public float minSpeed;
    public float maxSpeed;
    public float minChance;
    public float maxChance;
    // Maximum number of this type of item that is allowed to be visible on the
    // screen at one time (prevent large items from cluttering the scene)
    public int maxNumInScene;
    public Direction[] directions;
    public int _id;
}

public enum Direction
{
    none = 0,
    up,
    down,
    wave,
};

public class GameItemsBehavior : MonoBehaviour
{
    // All obstacle sprites
    public GameObject enemyBird1Prefab;
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

    // All trigger prefabs
    public GameObject eggPrefab;
    public GameObject finishLinePrefab;
    public GameObject coinPrefab;
    public GameObject bananaPrefab;
    public GameObject blueberriesPrefab;
    public GameObject strawberryPrefab;

    public GameObstacle[] gameObstacles;
    public GameObstacle[] gameCollectibles;

    public const float BASE_SPEED = 0.04f;

    private Dictionary<int, int> obstacleCounts;
    private List<ObstaclePrefab> obstaclePrefabs;

    private Dictionary<int, int> collectibleCounts;
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
        blimd2Id,
        blimp3Id,
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
    }

    private void Awake()
    {
        // DO NOT SEED THIS RANDOM
        sysRandom = new System.Random();

        obstacleCounts = new Dictionary<int, int>();
        obstaclePrefabs = new List<ObstaclePrefab>();

        collectibleCounts = new Dictionary<int, int>();
        collectiblePrefabs = new List<ObstaclePrefab>();
    }

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
                        RemoveObstacleFromGame(obj);
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
                        RemoveCollectibleFromGame(obj);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateGameObstaclePositions()
    {
        UpdateGameObjectPositions(gameObstacles);
    }

    public void UpdateGameCollectiblePositions()
    {
        UpdateGameObjectPositions(gameCollectibles);
    }

    private void UpdateGameObjectPositions(GameObstacle[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i].used)
            {
                Transform t = array[i].transform;
                Vector3 pos = t.localPosition;
                pos.x -= array[i].xDelta;

                switch (array[i].direction)
                {
                    case Direction.down:
                        pos.y += array[i].yDelta;
                        break;

                    case Direction.up:
                        pos.y += array[i].yDelta;
                        break;

                    case Direction.wave:
                        if (array[i].incrementY)
                        {
                            pos.y += array[i].yDelta;
                            if (pos.y >= array[i].endY)
                            {
                                array[i].incrementY = false;
                                array[i].transform.rotation = new Quaternion(0, 0, 0.3f, 1.0f);
                            }
                        }
                        else
                        {
                            pos.y -= array[i].yDelta;
                            if (pos.y <= array[i].startY)
                            {
                                array[i].incrementY = true;
                                array[i].transform.rotation = new Quaternion(0, 0, -0.3f, 1.0f);
                            }
                        }
                        break;
                }

                t.localPosition = pos;
            }
        }
    }

    public void RemoveCollectibleFromGame(GameObject obj)
    {
        for (int i = 0; i < gameCollectibles.Length; ++i)
        {
            GameObject go = gameCollectibles[i].gameObject;
            if (obj == go)
            {
                Destroy(go);
                gameCollectibles[i].used = false;
            }
        }

        collectibleMaxChance += COLLECTIBLE_CHANCE_CHANGE;
    }

    public void RemoveObstacleFromGame(GameObject obj)
    {
        for (int i = 0; i < gameObstacles.Length; ++i)
        {
            GameObject go = gameObstacles[i].gameObject;
            if (obj == go)
            {
                Destroy(go);
                gameObstacles[i].used = false;
                obstacleCounts[gameObstacles[i]._id] -= 1;
            }
        }
        obstacleMaxChance += OBSTACLE_CHANCE_CHANGE;
    }

    public void InitGameObjectManager(float cameraMaxX, float backgroundMinY,
                                      float backgroundMaxY, int numObstacles,
                                      int numCollectibles)
    {
        this.cameraMaxX = cameraMaxX;
        this.backgroundMinY = backgroundMinY;
        this.backgroundMaxY = backgroundMaxY;
        gameObstacles = new GameObstacle[numObstacles];
        gameCollectibles = new GameObstacle[numCollectibles];

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

    private bool IsFloatingObject(GameObject prefab)
    {
        if (prefab == enemyBird1Prefab ||
            prefab == blimp1Prefab ||
            prefab == blimp2Prefab ||
            prefab == plane1Prefab ||
            prefab == plane2Prefab ||
            prefab == parachute1Prefab ||
            prefab == parachute2Prefab ||
            prefab == parachute3Prefab ||
            prefab == balloon1Prefab ||
            prefab == balloon2Prefab ||
            prefab == balloon3Prefab)
        {
            return true;
        }

        return false;
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

    public void GenerateFinishLine()
    {
        int arrayIndex = gameObstacles.Length - 1;

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

        GameObstacle go = new GameObstacle();
        go.gameObject = finishLine;
        go._id = (int)ItemIds.finishLineId;
        go.transform = go.gameObject.transform;
        go.renderer = go.gameObject.GetComponent<Renderer>();
        // Don't need to get the collider for this object
        go.xDelta = BASE_SPEED;
        go.direction = Direction.none;
        go.used = true;

        gameObstacles[arrayIndex] = go;
    }

    public void GenerateNewObstacle(bool disableColliders)
    {
        int arrayIndex = FindAvailableArraySlot(gameObstacles);
        if (arrayIndex < 0 || (gameObstacles.Length - 1) == arrayIndex)
        {
            // No slots available at this time (saving the last one for the finish line)
            return;
        }

        float chance = Random.Range(0f, 1f);
        if (chance <= obstacleMaxChance)
        {
            int opIndex = SelectPrefab(obstaclePrefabs, true);
            ObstaclePrefab op = obstaclePrefabs[opIndex];
            if (obstacleCounts[op._id] >= op.maxNumInScene && op.maxNumInScene > 0)
            {
                /*
                 * Some obstacles are large or difficult to avoid, so we use
                 * .maxNumInScene to determine a limit for how many of these
                 * obstacles should be allowed to be on screen at any given moment.
                 * We increment this value below, but decrement it when removing
                 * that item from the scene.
                 * 
                 * maxNumInScene of -1 means there is no limit; if it's >0, then
                 * we abide by that limit.
                 */
                return;
            }

            // Increment the count
            obstacleCounts[op._id] += 1;

            GameObstacle go = new GameObstacle();
            go.gameObject = Instantiate(op.prefab);
            go._id = op._id;
            go.transform = go.gameObject.transform;
            go.renderer = go.gameObject.GetComponent<Renderer>();
            go.collider = go.gameObject.GetComponent<PolygonCollider2D>();
            go.xDelta = Random.Range(op.minSpeed, op.maxSpeed);

            Direction d = op.directions[(Random.Range(0, op.directions.Length))];
            switch (d)
            {
                case Direction.up:
                    go.yDelta = Random.Range(0.0005f, 0.006f);
                    break;

                case Direction.down:
                    go.yDelta = -1 * Random.Range(0.0005f, 0.006f);
                    break;

                case Direction.wave:
                    go.yDelta = Random.Range(0.08f, 0.10f);
                    go.startY = go.transform.localPosition.y;
                    go.endY = go.startY + Random.Range(backgroundMaxY * 0.6f, backgroundMaxY * 0.8f);
                    go.incrementY = true;
                    break;
            }
            go.direction = d;

            // Set the position of the new obstacle within the game
            Vector3 pos = go.transform.localPosition;
            float objWidth = go.renderer.bounds.size.x;
            pos.x = cameraMaxX + objWidth;
            if (IsFloatingObject(op.prefab))
            {
                // If the object isn't something that needs to "remain" on the
                // ground, randomize it's starting Y position
                pos.y = Random.Range(backgroundMinY, backgroundMaxY);
            }
            go.transform.localPosition = pos;

            if (disableColliders)
            {
                go.collider.enabled = false;
            }

            go.used = true;

            gameObstacles[arrayIndex] = go;
            obstacleMaxChance -= OBSTACLE_CHANCE_CHANGE;
        }
    }

    private void GenerateEgg()
    {
        int arrayIndex = gameCollectibles.Length - 1;

        GameObstacle go = new GameObstacle();
        GameObject egg = Instantiate(eggPrefab);
        go.gameObject = egg;
        go.transform = go.gameObject.transform;
        go.renderer = go.gameObject.GetComponent<Renderer>();
        go.xDelta = BASE_SPEED;
        go.direction = Direction.none;
        go.used = true;

        Vector3 pos = go.transform.localPosition;
        float objWidth = go.renderer.bounds.size.x;
        pos.x = cameraMaxX + objWidth;
        pos.y = (sysRandom.Next((int)backgroundMinY * 100, (int)backgroundMaxY * 100) / 100);
        go.transform.localPosition = pos;

        gameCollectibles[arrayIndex] = go;
    }

    public void GenerateNewCollectible(bool generateEgg)
    {
        if (generateEgg)
        {
            GenerateEgg();
            return;
        }

        int arrayIndex = FindAvailableArraySlot(gameCollectibles);
        if (arrayIndex < 0 || (gameCollectibles.Length - 1) == arrayIndex)
        {
            // No slots available at this time
            return;
        }

        float chance = (float) sysRandom.NextDouble();
        if (chance < collectibleMaxChance)
        {
            int cpIndex = SelectPrefab(collectiblePrefabs, false);
            ObstaclePrefab op = collectiblePrefabs[cpIndex];

            if (collectibleCounts[op._id] >= op.maxNumInScene && op.maxNumInScene > 0)
            {
                /*
                 * Some collectibles should only be generated X number of times in
                 * a given level/scene. Once we reach that number, we don't generate
                 * anymore. This differs from game obstacles (above) in which
                 * .maxNumInScene means the number of that type of item that is
                 * allowed to be on screen at any given time.
                 *
                 * maxNumInScene of -1 means there is no limit; if it's >0, then
                 * we abide by that limit.
                 *
                 * We increment this number in this function but never decrement it.
                 */
                return;
            }

            collectibleCounts[op._id] += 1;

            GameObstacle go = new GameObstacle();
            go.gameObject = Instantiate(op.prefab);
            go.transform = go.gameObject.transform;
            go.renderer = go.gameObject.GetComponent<Renderer>();
            // XXX Maybe change this up a bit; collectibles should really only move at 1 speed
            go.xDelta = op.minSpeed;

            Vector3 pos = go.transform.localPosition;
            float objWidth = go.renderer.bounds.size.x;
            pos.x = cameraMaxX + objWidth;
            pos.y = (sysRandom.Next((int)backgroundMinY * 100, (int)backgroundMaxY * 100) / 100);
            go.transform.localPosition = pos;

            go.used = true;
            gameCollectibles[arrayIndex] = go;
            collectibleMaxChance -= COLLECTIBLE_CHANCE_CHANGE;
        }
    }

    public void DisableColliders()
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

    public void EnableColliders()
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


    /*
     * Obstacle generation helper functions
     */
    public void AddEnemyBirdObstacle(int maxNum, float minSpeed, float maxSpeed,
                                     float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.down,
            Direction.up
        };

        AddObstacle(enemyBird1Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.enemyBird1Id);
    }

    public void AddWaveObstacle(int maxNum, float minSpeed, float maxSpeed,
                                     float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddObstacle(wavePrefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.waveId);
    }

    public void AddFish1Obstacle(int maxNum, float minSpeed, float maxSpeed,
                                 float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        AddObstacle(fish1Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.fish1Id);
    }

    public void AddFish2Obstacle(int maxNum, float minSpeed, float maxSpeed,
                                 float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        AddObstacle(fish2Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.fish2Id);
    }

    public void AddFish3Obstacle(int maxNum, float minSpeed, float maxSpeed,
                                 float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.wave
        };

        AddObstacle(fish3Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.fish3Id);
    }

    public void AddBoat1Obstacle(int maxNum, float minSpeed, float maxSpeed,
                                 float minChance, float maxChance)
    {
        Direction[] directions =
        {
           Direction.none
        };

        AddObstacle(boat1Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.boat1Id);
    }

    public void AddShark1Obstacle(int maxNum, float minSpeed, float maxSpeed,
                                  float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddObstacle(shark1Prefab, directions, maxNum, minSpeed, maxSpeed,
                    minChance, maxChance, ItemIds.shark1Id);
    }

    private void AddObstacle(GameObject prefab, Direction[] directions, int maxNum,
                             float minSpeed, float maxSpeed, float minChance,
                             float maxChance, ItemIds _id)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = prefab;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.maxNumInScene = maxNum;
        op.directions = directions;
        op._id = (int)_id;

        obstaclePrefabs.Add(op);
        obstacleCounts.Add(op._id, 0);
    }


    /*
     * Collectible generation helper functions
     */
    public void AddCoinCollectible(int maxNum, float minSpeed, float maxSpeed,
                                   float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(coinPrefab, directions, maxNum, minSpeed, maxSpeed,
                       minChance, maxChance, ItemIds.coinId);
    }

    public void AddBananaCollectible(int maxNum, float minSpeed, float maxSpeed,
                                     float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(bananaPrefab, directions, maxNum, minSpeed, maxSpeed,
                       minChance, maxChance, ItemIds.bananaId);
    }

    public void AddBlueberryCollectible(int maxNum, float minSpeed, float maxSpeed,
                                        float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(blueberriesPrefab, directions, maxNum, minSpeed, maxSpeed,
                       minChance, maxChance, ItemIds.blueberryId);
    }

    public void AddStrawberryCollectible(int maxNum, float minSpeed, float maxSpeed,
                                         float minChance, float maxChance)
    {
        Direction[] directions =
        {
            Direction.none
        };

        AddCollectible(strawberryPrefab, directions, maxNum, minSpeed, maxSpeed,
                       minChance, maxChance, ItemIds.strawberryId);
    }

    private void AddCollectible(GameObject prefab, Direction[] directions, int maxNum,
                                float minSpeed, float maxSpeed, float minChance,
                                float maxChance, ItemIds _id)
    {
        ObstaclePrefab op = new ObstaclePrefab();
        op.prefab = prefab;
        op.maxNumInScene = maxNum;
        op.minSpeed = minSpeed;
        op.maxSpeed = maxSpeed;
        op.minChance = minChance;
        op.maxChance = maxChance;
        op.directions = directions;
        op._id = (int)_id;

        collectiblePrefabs.Add(op);
        collectibleCounts.Add(op._id, 0);
    }
}
