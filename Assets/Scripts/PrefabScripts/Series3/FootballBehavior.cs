using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootballBehavior : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    private bool init;

    private float MAX_GRAVITY_SCALE = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        // Have the football point straight up
        transform.rotation = new Quaternion(0, 0, 1.0f, 1.0f);

        System.Random sysRandom = new System.Random();
        float verticalImpulse = (float)(sysRandom.NextDouble() * (6f - 4f) + 4f);
        rigidBody.AddForceAtPosition(new Vector2(-10, verticalImpulse), new Vector2(0, -1), ForceMode2D.Impulse);

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
