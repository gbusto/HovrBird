using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LevelBehavior : MonoBehaviour
{
    // Public level variables
    public GameObject commonPrefab;
    private GameObject commonObject;
    private CommonBehavior commonScript;

    public GameObject treeUpLightPrefab;
    public GameObject treeDownLightPrefab;
    public GameObject treeUpDarkPrefab;
    public GameObject treeDownDarkPrefab;
    public GameObject scoreEdgePrefab;
    private readonly float treeZposition = -2f;

    // The cloud game objects
    public GameObject backgroundObject1;
    public GameObject backgroundObject2;
    private Renderer background1Renderer;
    private Renderer background2Renderer;
    // The behavior scripts for cloud game objects
    private Background1Behavior background1Script;
    private Background2Behavior background2Script;
    private float backgroundObjectsDistance;

    // The ground game objects
    public GameObject groundObject1;
    public GameObject groundObject2;
    private Renderer ground1Renderer;
    private Renderer ground2Renderer;
    // The behavior scripts for ground game objects
    private Ground1Behavior ground1Script;
    private Ground2Behavior ground2Script;
    private float groundObjectsDistance;

    private bool darkTrees;
    private List<GameObject> pipeList;
    private List<PolygonCollider2D> pipeColliderList;
    private float cameraBoundsX;
    private int pipeGenerationTurnCounter = 0;
    private int pipeGenerationTurns = 0;
    private bool collisionsWereDisabled;

    private readonly float moveBackgroundBy = 0.020f;
    private readonly float moveGroundBy = 0.040f;

    private LevelData levelData;


    #region UnityFunctions
    /*
     * Unity functions
     */

    void Awake()
    {
        levelData = LevelData.Instance();

        commonObject = Instantiate(commonPrefab);
        commonScript = commonObject.GetComponent<CommonBehavior>();

        background1Script = backgroundObject1.GetComponent<Background1Behavior>();
        background2Script = backgroundObject2.GetComponent<Background2Behavior>();
        background1Renderer = backgroundObject1.GetComponent<Renderer>();
        background2Renderer = backgroundObject2.GetComponent<Renderer>();

        groundObject1.tag = "Ground";
        groundObject2.tag = "Ground";
        ground1Script = groundObject1.GetComponent<Ground1Behavior>();
        ground2Script = groundObject2.GetComponent<Ground2Behavior>();
        ground1Renderer = groundObject1.GetComponent<Renderer>();
        ground2Renderer = groundObject2.GetComponent<Renderer>();

        pipeList = new List<GameObject>();
        pipeColliderList = new List<PolygonCollider2D>();

        // Level 0 is classic mode
        LevelManager.SetLevelNumber(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        // Step 1. Update the high score in the common script
        // Set the high score in the common game object when we get it
        commonScript.UpdateHighScore(levelData.levelData.level0HighScore);

        // Figure out the best place to start generating pipes off screen
        // It should be shortly off camera on the right side before becoming visible in the camera
        if (Screen.width > Screen.height)
        {
            // Calculate the offscreen startpoint when the screen is wider than it is tall
            cameraBoundsX = (Camera.main.orthographicSize * 2.0f) * (Screen.width / Screen.height);
        }
        else if (Screen.height > Screen.width)
        {
            // Calculate the offscreen startpoint when the screen is taller than it is wide (phones or portrait iPad)
            cameraBoundsX = (Camera.main.orthographicSize);
        }

        List<string> collisionTags = new List<string>
        {
            "Pipe",
            "Ground"
        };
        List<string> triggerCollisionTags = new List<string>
        {
            "Score"
        };

        // STEP 2. Add collisions to the common script
        commonScript.AddCollisionTags(collisionTags);
        commonScript.AddTriggerCollisionTags(triggerCollisionTags);

        #region ScreenOrientation
#if UNITY_ANDROID
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
#elif UNITY_IOS
        // FOR IPAD ONLY! (Need to add function to account for Android tablets)
        if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
        {
            // Check the starting screen orientation.
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                // If app starts off in landscape, only allow auto rotation to other landscape orientations
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
                // If app starts off in portrait, only allow auto rotation to other portrait orientations
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
            }
        }
        else
        {
            // All other devices
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
        }
#endif
        #endregion

        // Set the transition from/to points for ground and background now
        backgroundObjectsDistance = backgroundObject2.transform.localPosition.x - backgroundObject1.transform.localPosition.x;
        groundObjectsDistance = groundObject2.transform.localPosition.x - groundObject1.transform.localPosition.x;

        pipeGenerationTurns = PipeDistanceByBirdId();
        pipeGenerationTurnCounter = pipeGenerationTurns;
    }

    private int PipeDistanceByBirdId()
    {
        uint id = PlayerPrefsCommon.GetActiveBirdId();
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return 80;

            case InventoryData.SAM_ID:
                return 90;

            default:
                return 80;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (commonScript.GetGameState())
        {
            case CommonBehavior.GameState.Active:
                UpdateBackgroundObjects();
                UpdateGroundObjects();
                UpdatePipes();

                if (commonScript.disableColliders)
                {
                    collisionsWereDisabled = true;
                    DisablePipeColliders();
                }
                else if (false == commonScript.disableColliders && collisionsWereDisabled)
                {
                    collisionsWereDisabled = false;
                    EnablePipeColliders();
                }
                break;

            case CommonBehavior.GameState.Loss:
                GameOver();
                break;

        }
    }

    private void FixedUpdate()
    {
        // STEP 5: Check if the game is running
        // Should maybe move this to the update function...
        // If the UI lags, the pipes will be generated more closely together than normal
        switch (commonScript.GetGameState())
        {
            case CommonBehavior.GameState.Active:
                if (pipeGenerationTurnCounter == pipeGenerationTurns)
                {
                    pipeGenerationTurnCounter = 0;
                    GeneratePipePair();
                }

                pipeGenerationTurnCounter += 1;
                break;
        }
    }

    private void OnApplicationQuit()
    {
        // Save data right before the application exits
        levelData.SaveUserData();
    }

    /*
     * End Unity functions
     */
    #endregion


    // STEP 6: Gameover
    public void GameOver()
    {
        // Update the classic game object; if this returns true, user beat their
        // high score!
        int score = commonScript.GetScore();
        if (score > levelData.levelData.level0HighScore)
        {
            levelData.levelData.level0HighScore = score;
        }
        levelData.levelData.level0TimesPlayed += 1;

        commonScript.UpdateHighScore(levelData.levelData.level0HighScore);
        commonScript.CleanupDone();
    }



    #region PipeFunctions
    /*
     * Pipe functions
     */
    public void DisablePipeColliders()
    {
        foreach (PolygonCollider2D pc in pipeColliderList)
        {
            pc.enabled = false;
        }
    }

    public void EnablePipeColliders()
    {
        foreach (PolygonCollider2D pc in pipeColliderList)
        {
            pc.enabled = true;
        }
    }

    private void GeneratePipePair()
    {
        float pipeVerticalGap = 9.0f;
        float minPipeHeight = -6.2f;
        float maxPipeHeight = -1.8f;
        float pipeUpRandomHeight = UnityEngine.Random.Range(minPipeHeight, maxPipeHeight);
        float pipeDownHeight = pipeUpRandomHeight + pipeVerticalGap;

        GameObject treeUp;
        GameObject treeDown;
        if (darkTrees)
        {
            treeUp = Instantiate(treeUpDarkPrefab);
            treeDown = Instantiate(treeDownDarkPrefab);
            darkTrees = !darkTrees;
        }
        else
        {
            treeUp = Instantiate(treeUpLightPrefab);
            treeDown = Instantiate(treeDownLightPrefab);
            darkTrees = !darkTrees;
        }

        GameObject scoreEdge = Instantiate(scoreEdgePrefab);

        treeUp.transform.position = new Vector3(cameraBoundsX, pipeUpRandomHeight, treeZposition);
        treeDown.transform.position = new Vector3(cameraBoundsX, pipeDownHeight, treeZposition);
        scoreEdge.transform.position = new Vector3(cameraBoundsX, scoreEdge.transform.localPosition.y, treeZposition);

        treeUp.tag = "Pipe";
        treeDown.tag = "Pipe";
        scoreEdge.tag = "Score";

        pipeList.Add(treeUp);
        pipeList.Add(treeDown);
        pipeList.Add(scoreEdge);

        PolygonCollider2D pc1 = treeUp.GetComponent<PolygonCollider2D>();
        PolygonCollider2D pc2 = treeDown.GetComponent<PolygonCollider2D>();

        if (collisionsWereDisabled)
        {
            pc1.enabled = false;
            pc2.enabled = false;
        }

        pipeColliderList.Add(pc1);
        pipeColliderList.Add(pc2);
    }

    private void UpdatePipes()
    {
        List<GameObject> pipesToRemove = new List<GameObject>();

        foreach (GameObject pipe in pipeList)
        {
            pipe.transform.position = new Vector3(pipe.transform.position.x - moveGroundBy, pipe.transform.position.y, treeZposition);
            if (pipe.transform.position.x < (-1 * cameraBoundsX))
            {
                pipesToRemove.Add(pipe);
            }
        }

        foreach (GameObject pipe in pipesToRemove)
        {
            PolygonCollider2D pc = pipe.GetComponent<PolygonCollider2D>();
            pipeList.Remove(pipe);
            pipeColliderList.Remove(pc);
        }
    }

    /*
     * End pipe functions
     */
    #endregion


    #region MoveObjects
    /*
     * Object move functions
     */
    private float GetObjectsBoundCenter(Renderer r)
    {
        return r.bounds.center.x;
    }

    private void UpdateBackgroundObjects()
    {
        Vector3 pos1 = backgroundObject1.transform.localPosition;
        Vector3 newPos1 = new Vector3(pos1.x - moveBackgroundBy, pos1.y, pos1.z);
        backgroundObject1.transform.localPosition = newPos1;

        Vector3 pos2 = backgroundObject2.transform.localPosition;
        Vector3 newPos2 = new Vector3(pos2.x - moveBackgroundBy, pos2.y, pos2.z);
        backgroundObject2.transform.localPosition = newPos2;

        if (background1Script.shiftBackground)
        {
            // Time to transition backgroundObject1
            background1Script.shiftBackground = false;
            newPos1.x = GetObjectsBoundCenter(background2Renderer) + backgroundObjectsDistance;
            backgroundObject1.transform.localPosition = newPos1;
        }
        else if (background2Script.shiftBackground)
        {
            // Time to transition backgroundObject2
            background2Script.shiftBackground = false;
            newPos2.x = GetObjectsBoundCenter(background1Renderer) + backgroundObjectsDistance;
            backgroundObject2.transform.localPosition = newPos2;
        }
    }

    private void UpdateGroundObjects()
    {
        Vector3 pos1 = groundObject1.transform.localPosition;
        Vector3 newPos1 = new Vector3(pos1.x - moveGroundBy, pos1.y, pos1.z);
        groundObject1.transform.localPosition = newPos1;

        Vector3 pos2 = groundObject2.transform.localPosition;
        Vector3 newPos2 = new Vector3(pos2.x - moveGroundBy, pos2.y, pos2.z);
        groundObject2.transform.localPosition = newPos2;

        if (ground1Script.shiftGround)
        {
            // Time to transition backgroundObject1
            ground1Script.shiftGround = false;
            newPos1.x = GetObjectsBoundCenter(ground2Renderer) + groundObjectsDistance;
            groundObject1.transform.localPosition = newPos1;
        }
        else if (ground2Script.shiftGround)
        {
            // Time to transition backgroundObject2
            ground2Script.shiftGround = false;
            newPos2.x = GetObjectsBoundCenter(ground1Renderer) + groundObjectsDistance;
            groundObject2.transform.localPosition = newPos2;
        }
    }

    /*
     * End object move functions
     */
    #endregion
}