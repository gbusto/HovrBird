using UnityEngine;

public class BeachBallBehavior : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    private bool init;

    private float MAX_GRAVITY_SCALE = 0.16f;

    // NOTE: Collider needs to start in a disabled state to avoid an initial split-second
    // collision when the obstacle is generated in it's initial (0, 0) world position

    private void Start()
    {
        op = gameObstacleScript.op;

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        System.Random sysRandom = new System.Random();
        float verticalImpulse = (float)(sysRandom.NextDouble() * (3f - 1f) + 1f);
        rigidBody.AddForceAtPosition(new Vector2(-4, verticalImpulse), new Vector2(0, -1), ForceMode2D.Impulse);

        // Might not be necessary
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
            if (Mathf.Approximately(rigidBody.gravityScale, 0f))
            {
                rigidBody.gravityScale = MAX_GRAVITY_SCALE;
            }

            // Do whatever is needed to move the object
            if (gameObstacleScript.disableColliders && collider.enabled)
            {
                collider.enabled = false;
            }
            else if (!gameObstacleScript.disableColliders && !collider.enabled)
            {
                collider.enabled = true;
            }

            Vector3 pos = transform.localPosition;
            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }
        }
        else
        {
            if (Mathf.Approximately(rigidBody.gravityScale, MAX_GRAVITY_SCALE))
            {
                rigidBody.gravityScale = 0f;
            }
        }
    }
}
