using UnityEngine;

public class Fish1Behavior : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    public float minHorizontalImpulse = 2.5f;
    public float maxHorizontalImpulse = 3.0f;

    public float minVerticalImpulse = 9f;
    public float maxVerticalImpulse = 11f;

    private float startY;
    private bool init;

    private float horizontalImpulseValue;
    private float verticalImpulseValue;
    private float MAX_GRAVITY_SCALE = 1f;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        startY = pos.y;

        System.Random sysRandom = new System.Random();
        verticalImpulseValue = (float)(sysRandom.NextDouble() * (maxVerticalImpulse - minVerticalImpulse) + minVerticalImpulse);
        horizontalImpulseValue = (float)(sysRandom.NextDouble() * (maxHorizontalImpulse - minHorizontalImpulse) + minHorizontalImpulse);
        rigidBody.AddForce(new Vector2(-1 * horizontalImpulseValue, verticalImpulseValue), ForceMode2D.Impulse);

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
            if (pos.y <= startY)
            {
                // Apply impulse
                rigidBody.velocity = new Vector2(0, 0);
                rigidBody.AddForce(new Vector2(-1 * horizontalImpulseValue, verticalImpulseValue), ForceMode2D.Impulse);
            }

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }

            // Rotate the fish based on its velocity
            rigidBody.rotation = rigidBody.velocity.y * -5f;
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
