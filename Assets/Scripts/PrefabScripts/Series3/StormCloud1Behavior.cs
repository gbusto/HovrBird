using UnityEngine;

public class StormCloud1Behavior : MonoBehaviour
{
    public GameObstacleBehavior gameObstacleScript;
    public GameObject lightning;
    private PolygonCollider2D lightningCollider;

    public ObstaclePrefab op;

    private float xDelta;
    private float showLightning;
    private int frameCounter;
    private const int MAX_FRAMES_LIGHTNING = 4;

    private const double MIN_X_VALUE = -2.0;
    private const double MAX_X_VALUE = 2.0;

    private bool init;

    System.Random sysRandom;

    // Start is called before the first frame update
    void Start()
    {
        sysRandom = new System.Random();
        lightningCollider = lightning.GetComponent<PolygonCollider2D>();
        lightning.SetActive(false);
        showLightning = (float)sysRandom.NextDouble();

        op = gameObstacleScript.op;

        xDelta = Random.Range(op.minSpeed, op.maxSpeed);

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        init = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (false == init)
        {
            Start();
        }

        if (gameObstacleScript.active)
        {
            // Do whatever is needed to move the object
            if (lightning.activeInHierarchy)
            {
                if (gameObstacleScript.disableColliders && lightningCollider.enabled)
                {
                    lightningCollider.enabled = false;
                }
                else if (!gameObstacleScript.disableColliders && !lightningCollider.enabled)
                {
                    lightningCollider.enabled = true;
                }

                frameCounter += 1;
                if (frameCounter >= MAX_FRAMES_LIGHTNING)
                {
                    lightning.SetActive(false);
                    frameCounter = 0;
                }
            }
            else
            {
                float chance = (float)sysRandom.NextDouble();
                if (Approximately(chance, showLightning))
                {
                    // Show the lightning for a few frames
                    lightning.SetActive(true);
                }
            }

            Vector3 pos = transform.localPosition;
            pos.x -= xDelta;
            transform.localPosition = pos;

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }
        }
    }

    private bool Approximately(float a, float b)
    {
        float threshold = 0.005f;
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }
}
