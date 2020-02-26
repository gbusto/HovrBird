using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkBehavior : MonoBehaviour
{
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    private float xDelta;
    private float yDelta;
    private float startY;
    private float endY;
    private bool incrementY;

    private bool init;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        xDelta = Random.Range(op.minSpeed, op.maxSpeed);

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        endY = Random.Range(op.minMoveY, op.maxMoveY);
        startY = pos.y;
        transform.localPosition = pos;

        // Should be determined by size of the object, speed at which
        // it's moving, and width/height of the screen
        yDelta = (endY - startY) / 160f;
        incrementY = true;

        transform.rotation = new Quaternion(0, 0, -0.35f, 1.0f);

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
            pos.x -= xDelta;

            if (incrementY)
            {
                // Football is ascending
                pos.y += yDelta;
                if (pos.y >= endY)
                {
                    incrementY = false;
                }
            }
            else
            {
                // Football is descending
                pos.y -= yDelta;
                // Object will continue to fall until it's removed from the game
            }

            transform.localPosition = pos;

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }
        }
    }
}
