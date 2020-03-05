using UnityEngine;

public class CollectibleItemBehavior : MonoBehaviour
{
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    private float xDelta;

    private bool init;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        xDelta = Random.Range(op.minSpeed, op.maxSpeed);

        System.Random sysRandom = new System.Random();

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = (float)(sysRandom.NextDouble() * (op.maxGenY - op.minGenY) + op.minGenY);
        transform.localPosition = pos;

        collider.enabled = true;

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
            Vector3 pos = transform.localPosition;
            pos.x -= xDelta;
            transform.localPosition = pos;

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }
        }
    }
}
